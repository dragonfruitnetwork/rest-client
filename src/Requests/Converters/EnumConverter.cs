// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Text;

namespace DragonFruit.Data.Requests.Converters
{
    public static class EnumConverter
    {
        public static void WriteEnumValue(object enumValue, EnumOptions mode, StringBuilder target)
        {
            switch (mode)
            {
                case EnumOptions.Numeric:
                    target.Append((int)enumValue);
                    break;

                case EnumOptions.StringLower:
                    target.Append(enumValue.ToString().ToLowerInvariant().Replace(" ", string.Empty));
                    break;

                case EnumOptions.StringUpper:
                    target.Append(enumValue.ToString().ToUpperInvariant().Replace(" ", string.Empty));
                    break;
            }
        }
    }
}
