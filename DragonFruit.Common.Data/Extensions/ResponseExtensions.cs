// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Net.Http;

namespace DragonFruit.Common.Data.Extensions
{
    public static class ResponseExtensions
    {
        public static T To<T>(this HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var targetType = typeof(T);

            switch (targetType.IsPrimitive)
            {
                case true:
                case false when targetType == typeof(string):
                    return (T)Convert.ChangeType(response.Content.ReadAsStringAsync().Result, targetType);

                default:
                    throw new ArgumentException($"Cannot convert HTTP response to {targetType}. It must be a primitive type or a string", nameof(T));
            }
        }
    }
}
