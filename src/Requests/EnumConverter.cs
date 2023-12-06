﻿// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Globalization;
using System.Text;

namespace DragonFruit.Data.Requests
{
    public static class EnumConverter
    {
        public static void AppendEnum<T>(StringBuilder destination, T value, EnumOption mode, string propertyName) where T : Enum
        {
            destination.AppendFormat("{0}={1}&", propertyName, GetEnumValue(value, mode));
        }

        public static string GetEnumValue<T>(T value, EnumOption mode) where T : Enum
        {
            switch (mode)
            {
                case EnumOption.Numeric:
                    return Convert.ToInt32(value, NumberFormatInfo.InvariantInfo).ToString();

                case EnumOption.StringLower:
                    return value.ToString().ToLowerInvariant();

                case EnumOption.StringUpper:
                    return value.ToString().ToUpperInvariant();

                case EnumOption.None:
                default:
                    return value.ToString();
            }
        }
    }
}
