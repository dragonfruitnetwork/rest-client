﻿// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using DragonFruit.Common.Data.Serializers;

namespace DragonFruit.Common.Data
{
    /// <summary>
    /// A <see cref="ApiClient"/> superclass designed to allow better serializer configuration
    /// </summary>
    /// <typeparam name="T">The <see cref="ApiSerializer"/> to use</typeparam>
    public class ApiClient<T> : ApiClient where T : ApiSerializer, new()
    {
        public ApiClient(Action<T> configurationOptions = null)
            : base(Activator.CreateInstance<T>())
        {
            if (configurationOptions != null)
            {
                Serializer.Configure(configurationOptions);
            }
        }
    }
}