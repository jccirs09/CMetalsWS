using System;

namespace CMetalsWS.Utils
{
    public static class UiHelper
    {
        public static string GetInitials(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();

            return $"{parts[0][0]}".ToUpper();
        }
    }
}
