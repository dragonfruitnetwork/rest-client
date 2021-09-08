// DragonFruit.Common Copyright 2021 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DragonFruit.Common.Data.Serializers
{
    public class SerializerResolver
    {
        private static Dictionary<Type, Type> DeserializerMap { get; } = new();
        private static Dictionary<Type, Type> SerializerMap { get; } = new();

        /// <summary>
        /// Registers a serializer for the specified type. This applies to all <see cref="ApiClient"/>s
        /// </summary>
        /// <param name="direction">Whether this serializer should apply to incoming/outgoing data</param>
        /// <typeparam name="T">The object type to specify the serializer for</typeparam>
        /// <typeparam name="TSerializer">The serializer to apply</typeparam>
        public static void Register<T, TSerializer>(DataDirection direction = DataDirection.All)
            where T : class
            where TSerializer : ISerializer
        {
            if (direction.HasFlag(DataDirection.In))
            {
                DeserializerMap[typeof(T)] = typeof(TSerializer);
            }

            if (direction.HasFlag(DataDirection.Out))
            {
                SerializerMap[typeof(T)] = typeof(TSerializer);
            }
        }

        /// <summary>
        /// Removes the registered serializer for the type. This applies to all <see cref="ApiClient"/>s
        /// </summary>
        /// <param name="direction">Whether this serializer should be removed from incoming/outgoing data</param>
        /// <typeparam name="T">The object type to remove the serializer for</typeparam>
        public static void Unregister<T>(DataDirection direction = DataDirection.All)
            where T : class
        {
            if (direction.HasFlag(DataDirection.In))
            {
                DeserializerMap.Remove(typeof(T));
            }

            if (direction.HasFlag(DataDirection.Out))
            {
                SerializerMap.Remove(typeof(T));
            }
        }

        public ISerializer Default { get; set; }
        private ConcurrentDictionary<Type, ISerializer> SerializerCache { get; } = new();

        /// <summary>
        /// Resolves the <see cref="ISerializer"/> for the type provided
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        public ISerializer Resolve<T>(DataDirection direction) where T : class => Resolve(typeof(T), direction);

        /// <summary>
        /// Resolves the <see cref="ISerializer"/> for the type provided
        /// </summary>
        public ISerializer Resolve(Type objectType, DataDirection direction)
        {
            if (!objectType.IsClass)
            {
                throw new ArgumentException("Provided object type is not a class", nameof(objectType));
            }

            var mapping = direction switch
            {
                DataDirection.In => DeserializerMap,
                DataDirection.Out => SerializerMap,

                _ => throw new ArgumentException(nameof(direction))
            };

            // if the map has the type registered, check the type in cache
            if (mapping.TryGetValue(objectType, out var serializerType))
            {
                return SerializerCache.GetOrAdd(serializerType, _ => (ISerializer)Activator.CreateInstance(serializerType));
            }

            // use generic
            return Default;
        }

        /// <summary>
        /// Configures the specified <see cref="TSerializer"/>, creating a client-specific instance if needed
        /// </summary>
        /// <param name="options">The options to set</param>
        /// <typeparam name="TSerializer">The <see cref="ISerializer"/> to configure</typeparam>
        public void Configure<TSerializer>(Action<TSerializer> options) where TSerializer : ISerializer
        {
            if (Default.GetType() == typeof(TSerializer))
            {
                options?.Invoke((TSerializer)Default);
            }
            else
            {
                var serializer = SerializerCache.GetOrAdd(typeof(TSerializer), _ => Activator.CreateInstance<TSerializer>());
                options?.Invoke((TSerializer)serializer);
            }
        }
    }

    public enum DataDirection
    {
        /// <summary>
        /// Applies to incoming data (deserialization)
        /// </summary>
        In,

        /// <summary>
        /// Applies to outgoing data (serialization)
        /// </summary>
        Out,

        /// <summary>
        /// Applies to all data directions
        /// </summary>
        All = In | Out
    }
}
