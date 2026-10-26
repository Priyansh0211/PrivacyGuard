using System;

namespace PrivacyGuard.Helpers
{
    public static class PrivacyHelper
    {
     
        public static string GenerateUserReferenceId()
        {
           
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return $"PSD-{new string(stringChars)}";
        }

      
        public static string GenerateGrievanceReferenceId()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd"); // 20261026

           
            var random = new Random();
            var randomPart = random.Next(1000, 9999).ToString();

            return $"GRV-{datePart}-{randomPart}";
        }
    }
}