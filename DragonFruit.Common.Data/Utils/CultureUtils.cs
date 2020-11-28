// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Globalization;

namespace DragonFruit.Common.Data.Utils
{
    public static class CultureUtils
    {
        private static CultureInfo _defaultCulture;

        public static CultureInfo DefaultCulture
        {
            get => _defaultCulture ?? CultureInfo.InvariantCulture;
            set => _defaultCulture = value;
        }

        internal static string AsString(this object value, CultureInfo culture = null) => value switch
        {
            bool boolVar => boolVar.ToString().ToLower(culture ?? DefaultCulture),
            null => null,

            _ => value.ToString()
        };
    }
}
