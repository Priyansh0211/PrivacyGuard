using System.Globalization;
using System.Reflection;
using PrivacyGuard.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PrivacyGuard.Services;

public sealed class PdfReportRenderer
{
    private static readonly Color Navy = Color.FromHex("123B5D");
    private static readonly Color Teal = Color.FromHex("2DB7A3");
    private static readonly Color LightBlue = Color.FromHex("EAF3F7");
    public PdfReportRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Render<T>(string reportTitle, string reportType, IEnumerable<T> records)
    {
        var rows = records.ToList();
        if (rows.FirstOrDefault() is CampaignAuditReportDto campaignAudit)
        {
            return RenderCampaignAudit(campaignAudit);
        }

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(34);
                page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(8).FontColor(Color.FromHex("243746")));
                page.Header().Element(header => ComposeHeader(header, reportTitle, reportType));
                page.Content().PaddingTop(18).Element(content => ComposeTable(content, reportType, properties, rows));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static byte[] RenderCampaignAudit(CampaignAuditReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(34);
                page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(8).FontColor(Color.FromHex("243746")));
                page.Header().Element(header => ComposeHeader(header, "Campaign Audit Report", "Campaign Audit"));
                page.Content().PaddingTop(16).Column(column =>
                {
                    column.Item().Element(content => ComposeCampaignMetadata(content, report));
                    column.Item().PaddingTop(12).Element(content => ComposeSummary(content, report));
                    column.Item().PaddingTop(12).Element(content => ComposeCompliance(content, report));
                    column.Item().PaddingTop(12).Element(content => ComposeTimeline(content, report));
                    column.Item().PaddingTop(12).Element(content => ComposeResponses(content, report));
                });
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeCampaignMetadata(IContainer container, CampaignAuditReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Background(LightBlue).Padding(8).Text("CAMPAIGN DETAILS").FontSize(9).Bold().FontColor(Navy);
            column.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                AddDetail(table, "Campaign", report.Title);
                AddDetail(table, "Campaign ID", report.CampaignId.ToString());
                AddDetail(table, "Type", report.CampaignType);
                AddDetail(table, "Status", report.CampaignStatus);
                AddDetail(table, "Target Audience", report.TargetAudience);
                AddDetail(table, "Target Role", report.TargetRole);
                AddDetail(table, "Data Inventory", report.LinkedDataInventory);
                AddDetail(table, "Description", report.Description);
            });
        });
    }

    private static void AddDetail(TableDescriptor table, string label, string value)
    {
        table.Cell().Background(Color.FromHex("F6FAFC")).BorderBottom(1).BorderColor(Color.FromHex("D7E3E9")).Padding(6)
            .Text(text =>
            {
                text.Span($"{label}: ").Bold().FontColor(Navy);
                text.Span(string.IsNullOrWhiteSpace(value) ? "Not available" : value);
            });
    }

    private static void ComposeSummary(IContainer container, CampaignAuditReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("RESPONSE SUMMARY").FontSize(9).Bold().FontColor(Navy);
            column.Item().PaddingTop(6).Row(row =>
            {
                AddMetric(row, "TOTAL RESPONSES", report.TotalResponses.ToString());
                AddMetric(row, "ACCEPTED", $"{report.AcceptedCount} ({report.AcceptanceRate:0.##}%)");
                AddMetric(row, "REJECTED", $"{report.RejectedCount} ({report.RejectionRate:0.##}%)");
                AddMetric(row, "COMPLETED", $"{report.CompletedCount} ({report.CompletionRate:0.##}%)");
            });
        });
    }

    private static void AddMetric(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().PaddingRight(5).Background(LightBlue).Padding(8).Column(column =>
        {
            column.Item().Text(label).FontSize(7).Bold().FontColor(Color.FromHex("657786"));
            column.Item().PaddingTop(4).Text(value).FontSize(13).Bold().FontColor(Navy);
        });
    }

    private static void ComposeCompliance(IContainer container, CampaignAuditReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("PRIVACY COMPLIANCE SUMMARY").FontSize(9).Bold().FontColor(Navy);
                row.AutoItem().Background(report.AuditResult == "Compliant" ? Color.FromHex("DDF4E8") : Color.FromHex("FCE3E3")).Padding(6)
                    .Text(report.AuditResult).FontSize(8).Bold().FontColor(report.AuditResult == "Compliant" ? Color.FromHex("167346") : Color.FromHex("B42318"));
            });
            column.Item().PaddingTop(6).Column(checks =>
            {
                foreach (var check in report.ComplianceChecks)
                {
                    checks.Item().PaddingBottom(3).Text($"• {check}").FontSize(8);
                }
            });
            column.Item().PaddingTop(4).Text(report.AuditRemarks).FontSize(8).Italic().FontColor(Color.FromHex("657786"));
        });
    }

    private static void ComposeTimeline(IContainer container, CampaignAuditReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("AUDIT TIMELINE").FontSize(9).Bold().FontColor(Navy);
            column.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });
                AddTimelineCell(table, "Created", report.CreatedAt);
                AddTimelineCell(table, "Starts", report.StartDate);
                AddTimelineCell(table, "Ends", report.EndDate);
                AddTimelineCell(table, "Last Updated", report.UpdatedAt);
            });
        });
    }

    private static void AddTimelineCell(TableDescriptor table, string label, DateTime value)
    {
        table.Cell().Background(Color.FromHex("F6FAFC")).BorderBottom(1).BorderColor(Color.FromHex("D7E3E9")).Padding(6)
            .Text(text =>
            {
                text.Span($"{label}\n").Bold().FontColor(Navy);
                text.Span(value.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture));
            });
    }

    private static void ComposeResponses(IContainer container, CampaignAuditReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text("DETAILED RESPONSE LOG").FontSize(9).Bold().FontColor(Navy);
            column.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn();
                    columns.RelativeColumn(1.3f);
                });

                foreach (var header in new[] { "REFERENCE", "ACTOR", "EMAIL", "STATUS", "RESPONDED AT" })
                {
                    table.Cell().Background(Navy).Padding(5).Text(header).FontSize(7).Bold().FontColor(Colors.White);
                }

                foreach (var response in report.Responses)
                {
                    AddResponseCell(table, response.ResponseReference);
                    AddResponseCell(table, response.ActorType);
                    AddResponseCell(table, string.IsNullOrWhiteSpace(response.MaskedEmail) ? "Not available" : response.MaskedEmail);
                    AddResponseCell(table, response.ResponseStatus);
                    AddResponseCell(table, response.RespondedAt?.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) ?? "Not responded");
                }
            });
        });
    }

    private static void AddResponseCell(TableDescriptor table, string value)
    {
        table.Cell().BorderBottom(1).BorderColor(Color.FromHex("D7E3E9")).Padding(5).Text(value).FontSize(7);
    }

    private static void ComposeHeader(IContainer container, string reportTitle, string reportType)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(176).Height(46).Background(Navy).Padding(8).AlignMiddle()
                    .Text("KAVACH").FontSize(15).Bold().FontColor(Colors.White);

                row.RelativeItem().AlignRight().Column(details =>
                {
                    details.Item().Text("CONFIDENTIAL DOCUMENT").FontSize(8).Bold().FontColor(Teal);
                    details.Item().Text(DateTime.UtcNow.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)).FontSize(8).FontColor(Color.FromHex("657786"));
                });
            });

            column.Item().PaddingTop(12).LineHorizontal(2).LineColor(Teal);
            column.Item().PaddingTop(12).Text(reportTitle).FontSize(18).Bold().FontColor(Navy);
            column.Item().Text($"{reportType}  •  Privacy and compliance reporting").FontSize(9).FontColor(Color.FromHex("657786"));
            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Color.FromHex("D7E3E9"));
        });
    }

    private static void ComposeTable<T>(IContainer container, string reportType, PropertyInfo[] properties, IReadOnlyCollection<T> rows)
    {
        container.Column(column =>
        {
            column.Item().Background(LightBlue).Padding(9).Column(summary =>
            {
                summary.Item().Text("REPORT OVERVIEW").FontSize(9).Bold().FontColor(Navy);
                summary.Item().PaddingTop(3).Text(GetReportDescription(reportType)).FontSize(8).FontColor(Color.FromHex("657786"));
                summary.Item().PaddingTop(8).Row(row =>
                {
                    AddGenericMetric(row, "RECORDS", rows.Count.ToString());
                    foreach (var metric in GetSummaryValues(properties, rows))
                    {
                        AddGenericMetric(row, metric.Label, metric.Value);
                    }
                });
            });

            column.Item().PaddingTop(14).Text("DETAILS").FontSize(9).Bold().FontColor(Navy);
            column.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    foreach (var _ in properties)
                    {
                        columns.RelativeColumn();
                    }
                });

                foreach (var property in properties)
                {
                    table.Cell().Background(Navy).Padding(5).Text(GetHeader(property.Name)).FontSize(7).Bold().FontColor(Colors.White);
                }

                var rowIndex = 0;
                foreach (var item in rows)
                {
                    foreach (var property in properties)
                    {
                        var text = FormatValue(property, property.GetValue(item));
                        var cell = table.Cell().Background(rowIndex % 2 == 0 ? Colors.White : Color.FromHex("F6FAFC")).BorderBottom(1).BorderColor(Color.FromHex("D7E3E9")).Padding(5);
                        cell.Text(text).FontSize(7);
                    }
                    rowIndex++;
                }
            });
        });
    }

    private static void AddGenericMetric(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().PaddingRight(5).Column(column =>
        {
            column.Item().Text(label).FontSize(7).Bold().FontColor(Color.FromHex("657786"));
            column.Item().PaddingTop(3).Text(value).FontSize(11).Bold().FontColor(Navy);
        });
    }

    private static IReadOnlyList<(string Label, string Value)> GetSummaryValues<T>(PropertyInfo[] properties, IReadOnlyCollection<T> rows)
    {
        var first = rows.FirstOrDefault();
        if (first == null)
        {
            return Array.Empty<(string, string)>();
        }

        var preferredNames = new[] { "Status", "CurrentStatus", "ComplianceStatus", "Outcome", "ResidualRisk", "Severity", "Decision" };
        return preferredNames
            .Select(name => properties.FirstOrDefault(property => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .Where(property => property != null)
            .Select(property => (GetHeader(property!.Name), FormatValue(property, property.GetValue(first))))
            .Where(metric => !string.IsNullOrWhiteSpace(metric.Item2) && metric.Item2 != "—")
            .Take(3)
            .ToList();
    }

    private static string GetReportDescription(string reportType)
    {
        return reportType switch
        {
            "GrievanceReport" => "A consolidated view of grievance references, categories, severity, status, and lifecycle dates.",
            "GrievanceAuditReport" => "An audit-focused grievance register with masked principal and assigned administrator details.",
            "ConsentAuditReport" => "A review of consent decisions, purpose links, active status, and expiry information.",
            "DataBreachReport" => "A data-breach register showing severity, affected-record estimates, and incident lifecycle dates.",
            "DpiaReport" => "A privacy impact assessment register covering residual risk, purposes, ownership, and status.",
            "DsarReport" => "A DSAR processing report showing request progress, task completion, and resolution targets.",
            "DataInventoryReport" => "A data inventory register describing categories, purposes, sources, storage, and retention.",
            "DataProcessingReport" => "A processing activity register showing purpose, data category, legal basis, and status.",
            "DataRetentionReport" => "A retention compliance register showing deletion dates, compliance state, and lifecycle status.",
            "DataSharingReport" => "A data-sharing register describing processors, relationships, locations, and cross-border movement.",
            "SecurityAuditReport" => "A tamper-evident security activity register containing actors, actions, outcomes, and timestamps.",
            "ComplianceGapReport" => "A compliance improvement register showing requirements, gaps, and recommended actions.",
            _ => "A structured PrivacyGuard report prepared for review and audit purposes."
        };
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Color.FromHex("D7E3E9"));
            column.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text("KAVACH  •  Protecting privacy through accountable reporting").FontSize(7).FontColor(Color.FromHex("657786"));
                row.AutoItem().Text(text =>
                {
                    text.Span("Page ").FontSize(7).FontColor(Color.FromHex("657786"));
                    text.CurrentPageNumber().FontSize(7).FontColor(Navy);
                    text.Span(" of ").FontSize(7).FontColor(Color.FromHex("657786"));
                    text.TotalPages().FontSize(7).FontColor(Navy);
                });
            });
        });
    }

    private static string GetHeader(string name)
    {
        return string.Concat(name.Select((character, index) => index > 0 && char.IsUpper(character) ? $" {character}" : character.ToString())).ToUpperInvariant();
    }

    private static string FormatValue(PropertyInfo property, object? value)
    {
        if (property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase))
        {
            return MaskEmail(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        return value switch
        {
            null => "—",
            DateTime dateTime => dateTime.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture),
            bool boolean => boolean ? "Yes" : "No",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "—"
        };
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return string.IsNullOrWhiteSpace(email) ? "—" : email;
        }

        var parts = email.Split('@', 2);
        return parts[0].Length <= 1 ? email : $"{parts[0][0]}***@{parts[1]}";
    }
}
