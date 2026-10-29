using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class BulkImportService : IBulkImportService
    {
        private readonly IBulkImportRepository _repository;

        public BulkImportService(IBulkImportRepository repository)
        {
            _repository = repository;
        }

        // METHOD 1: CSV ko parse karke Staging tables mein daalna (POST)
        public async Task<(bool IsSuccess, Guid? BatchId, string ErrorMessage)> StageImportBatchAsync(BulkImportRequestDto request, Guid organizationId, Guid userId)
        {
            // Check if actual file is uploaded
            bool hasFile = request.CsvFile != null && request.CsvFile.Length > 0;
            string rawData = request.RawCsvData ?? string.Empty;

            if (!hasFile && string.IsNullOrWhiteSpace(rawData))
            {
                return (false, null, "Please provide either a valid CSV file or raw CSV text.");
            }

            var batchId = Guid.NewGuid();
            var batch = new BulkImportBatch
            {
                BatchId = batchId,
                OrganizationId = organizationId,
                UploadedByUserId = userId,
                FileName = request.FileName ?? (hasFile ? request.CsvFile!.FileName : "Pasted_Data.csv"),
                ImportType = request.ImportType,
                Status = "Staged",
                Rows = new List<BulkImportRow>()
            };

            // Read from File if available, otherwise read from Raw text
            TextReader reader = hasFile
                ? (TextReader)new StreamReader(request.CsvFile!.OpenReadStream())
                : (TextReader)new StringReader(rawData);

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var csv = new CsvReader(reader, csvConfig);
            var records = csv.GetRecords<ConsumerCsvRecord>().ToList();

            int rowNumber = 1;

            foreach (var record in records)
            {
                rowNumber++;
                var validationErrors = new List<string>();

                // Validations
                if (string.IsNullOrWhiteSpace(record.Mobile) &&
                    string.IsNullOrWhiteSpace(record.Email) &&
                    string.IsNullOrWhiteSpace(record.ConsumerNo))
                {
                    validationErrors.Add("At least one identifier (Mobile, Email, or ConsumerNo) is mandatory.");
                }

                if (!string.IsNullOrWhiteSpace(record.Mobile) && !Regex.IsMatch(record.Mobile, @"^\d{10}$"))
                {
                    validationErrors.Add("Mobile number must be exactly 10 digits.");
                }

                if (!string.IsNullOrWhiteSpace(record.DateOfBirth) && !DateTime.TryParse(record.DateOfBirth, out _))
                {
                    validationErrors.Add("Invalid Date of Birth format.");
                }

                var validClasses = new[] { "adult", "child", "person_with_disability" };
                if (string.IsNullOrWhiteSpace(record.Class) || !validClasses.Contains(record.Class.ToLower()))
                {
                    validationErrors.Add("Class must be adult, child, or person_with_disability.");
                }

                var validLocales = new[] { "mr-IN", "en-IN", "hi-IN" };
                if (string.IsNullOrWhiteSpace(record.PreferredLocale) || !validLocales.Contains(record.PreferredLocale))
                {
                    validationErrors.Add("Locale must be mr-IN, en-IN, or hi-IN.");
                }

                var isRowValid = !validationErrors.Any();

                var importRow = new BulkImportRow
                {
                    RowId = Guid.NewGuid(),
                    BatchId = batchId,
                    RowNumber = rowNumber,
                    RawData = JsonSerializer.Serialize(record),
                    Status = isRowValid ? "Valid" : "Invalid",
                    ErrorMessage = isRowValid ? null : string.Join(" | ", validationErrors)
                };

                batch.Rows.Add(importRow);

                if (isRowValid) batch.ProcessedRows++;
                else batch.FailedRows++;
            }

            batch.TotalRows = batch.Rows.Count;

            await _repository.AddBatchAsync(batch);
            await _repository.SaveChangesAsync();

            return (true, batchId, string.Empty);
        }

        // METHOD 2: Saare Batches uthana (GET List)
        public async Task<IEnumerable<BulkImportBatch>> GetBatchesAsync(Guid organizationId, string? status)
        {
            return await _repository.GetBatchesAsync(organizationId, status);
        }

        // METHOD 3: Ek Batch aur uski rows uthana (GET Details)
        public async Task<object?> GetBatchDetailsAsync(Guid batchId, Guid organizationId, string? rowStatus, int page, int pageSize)
        {
            var batch = await _repository.GetBatchByIdAsync(batchId, organizationId);
            if (batch == null) return null;

            var rows = await _repository.GetBatchRowsAsync(batchId, rowStatus, page, pageSize);

            return new
            {
                BatchDetails = new
                {
                    batch.BatchId,
                    batch.FileName,
                    batch.Status,
                    batch.TotalRows,
                    batch.ProcessedRows,
                    batch.FailedRows,
                    batch.CreatedAt
                },
                Rows = rows.Select(r => new
                {
                    r.RowNumber,
                    r.Status,
                    r.ErrorMessage,
                    RawData = JsonSerializer.Deserialize<object>(r.RawData)
                })
            };
        }

        // ... (purane methods ke baad)

        public async Task<(bool IsSuccess, string Message)> ExecuteImportBatchAsync(Guid batchId, Guid organizationId)
        {
            var batch = await _repository.GetBatchByIdAsync(batchId, organizationId);

            if (batch == null) return (false, "Batch not found.");
            if (batch.Status != "Staged") return (false, $"Batch is already {batch.Status}. Idempotency enforced.");

            // Batch ko 'Executing' mark kar rahe hain
            batch.Status = "Executing";
            await _repository.SaveChangesAsync();

            var validRows = await _repository.GetValidRowsForExecutionAsync(batchId);

            if (!validRows.Any())
            {
                batch.Status = "CompletedWithErrors";
                batch.CompletedAt = DateTime.UtcNow;
                await _repository.SaveChangesAsync();
                return (false, "No valid rows found to execute.");
            }

            var consumersToInsert = new List<Consumer>();

            // Random ID generate karne ke liye
            var random = new Random();

            foreach (var row in validRows)
            {
                try
                {
                    // JSON se wapas ConsumerCsvRecord mein convert kar rahe hain
                    var record = System.Text.Json.JsonSerializer.Deserialize<ConsumerCsvRecord>(row.RawData);

                    // DPDP Act Compliance Rule: Child ya Disabled ke liye consent true
                    bool requiresGuardian = record!.Class?.ToLower() == "child" || record.Class?.ToLower() == "person_with_disability";

                    var consumer = new Consumer
                    {
                        ConsumerId = Guid.NewGuid(),
                        OrganizationId = organizationId,
                        // Reference format: PSD-XXXXXXXX[cite: 3]
                        ReferenceId = $"PSD-{random.Next(10000000, 99999999)}",
                        Mobile = record.Mobile,
                        Email = record.Email,
                        ConsumerNo = record.ConsumerNo,
                        DisplayName = record.DisplayName,
                        FullName = record.FullName,
                        DateOfBirth = string.IsNullOrWhiteSpace(record.DateOfBirth) ? null : DateTime.Parse(record.DateOfBirth),
                        Class = record.Class?.ToLower() ?? "adult",
                        PreferredLocale = record.PreferredLocale ?? "en-IN",
                        RequiresGuardianConsent = requiresGuardian // Applied Rule
                    };

                    consumersToInsert.Add(consumer);

                    row.Status = "Executed"; // Row processing track ho gayi[cite: 4]
                }
                catch (Exception ex)
                {
                    row.Status = "ExecutionFailed";
                    row.ErrorMessage = "Failed during DB conversion: " + ex.Message;
                    batch.FailedRows++;
                    batch.ProcessedRows--;
                }
            }

            // DB mein naye consumers daalna aur batch/rows update karna
            await _repository.AddConsumersAsync(consumersToInsert);

            batch.Status = batch.FailedRows > 0 ? "CompletedWithErrors" : "Completed";
            batch.CompletedAt = DateTime.UtcNow;

            await _repository.UpdateBatchAndRowsAsync(batch, validRows);

            return (true, $"Executed successfully. {consumersToInsert.Count} consumers created.");
        }
    }
}