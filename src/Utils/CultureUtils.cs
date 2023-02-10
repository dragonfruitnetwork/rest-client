// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Globalization;

namespace DragonFruit.Data.Utils
{
    public static class CultureUtils
    {
        private static CultureInfo _defaultCulture;

        public static CultureInfo DefaultCulture
        {
            get => _defaultCulture ?? CultureInfo.InvariantCulture;
            set => _defaultCulture = value;
        }

        internal static string AsString(this object value, CultureInfo culture = null)
        {
            culture ??= DefaultCulture;
            return value switch
            {
                null => null,
                bool boolVar => boolVar.ToString().ToLower(),
                IFormattable formattableVar => formattableVar.ToString(null, culture),

                _ => value.ToString()
            };
        }
    }
}
