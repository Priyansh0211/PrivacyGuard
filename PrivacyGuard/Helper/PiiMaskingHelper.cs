namespace PrivacyGuard.Helpers
{
    public static class PiiMaskingHelper
    {
        public static string MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@')) return "";
            var parts = email.Split('@');
            if (parts[0].Length <= 1) return email;
            return $"{parts[0][0]}{new string('*', parts[0].Length - 1)}@{parts[1]}";
        }

        public static string MaskMobile(string? mobile)
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Length < 10) return "";
            return $"{mobile.Substring(0, 2)}****{mobile.Substring(mobile.Length - 4)}";
        }

        public static string MaskConsumerNo(string? consumerNo)
        {
            if (string.IsNullOrEmpty(consumerNo) || consumerNo.Length < 8) return "";
            return $"{consumerNo.Substring(0, 2)}******{consumerNo.Substring(consumerNo.Length - 4)}";
        }

        public static string MaskName(string? fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "";
            var parts = fullName.Split(' ');
            if (parts.Length == 1) return $"{parts[0][0]}. ***";
            return $"{parts[0][0]}. {parts[parts.Length - 1]}"; 
        }
    }
}