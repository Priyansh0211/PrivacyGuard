using System;
using System.Linq;
using PrivacyGuard.Models;

namespace PrivacyGuard.Helpers
{
    public static class DataPrincipalAnonymizer
    {
        // Retention job aur DSAR erasure dono isi ko call karenge, taaki behavior same rahe.
        // dp.Identifiers load (Include) hona chahiye.
        public static void Anonymize(DataPrincipal dp, DateTime now)
        {
            var shortToken = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();

            dp.FullName = "Anonymized User";
            dp.DisplayName = "Anonymized User";
            dp.Email = $"anon_{dp.DataPrincipalId:N}@deleted.local";
            dp.Mobile = null;
            dp.ConsumerNo = null;
            dp.DateOfBirth = null;
            dp.PasswordHash = null;
            dp.ReferenceId = $"ANON-{shortToken}";
            dp.IsActive = false;
            dp.AnonymizedAt = now;
            dp.UpdatedAt = now;

            if (dp.Identifiers != null && dp.Identifiers.Any())
            {
                foreach (var identifier in dp.Identifiers)
                {
                    identifier.Value = $"REDACTED_{identifier.IdentifierId:N}";
                    identifier.IsVerified = false;
                }
            }
        }
    }
}