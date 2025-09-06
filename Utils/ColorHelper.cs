using MudBlazor;
using System;

namespace CMetalsWS.Utils
{
    public static class ColorHelper
    {
        public static Color GetDeterministicColor(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Color.Primary;
            }

            var hash = input.GetHashCode();
            var index = Math.Abs(hash % 7);

            return index switch
            {
                0 => Color.Primary,
                1 => Color.Secondary,
                2 => Color.Tertiary,
                3 => Color.Info,
                4 => Color.Success,
                5 => Color.Warning,
                6 => Color.Error,
                _ => Color.Primary,
            };
        }
    }
}
