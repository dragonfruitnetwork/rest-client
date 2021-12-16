// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

#endregion

#pragma warning disable 618

namespace DragonFruit.Data.Serializers
{
    public class SerializerResolver
    {
        private static readonly Dictionary<Type, Type> SerializerMap = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, Type> DeserializerMap = new Dictionary<Type, Type>();

        private readonly ConcurrentDictionary<Type, ApiSerializer> _serializerCache = new ConcurrentDictionary<Type, ApiSerializer>();

        /// <summary>
        /// Initialises a new instance of <see cref="SerializerResolver"/>, providing a default <see cref="ApiSerializer"/>
        /// </summary>
        /// <param name="default"></param>
        internal SerializerResolver(ApiSerializer @default)
        {
            Default = @default;
        }

        /// <summary>
        /// The default <see cref="ApiSerializer"/> in use.
        /// </summary>
        public ApiSerializer Default { get; }

        /// <summary>
        /// Registers a serializer for the specified type. This applies to all <see cref="ApiClient"/>s
        /// </summary>
        /// <param name="direction">Whether this serializer should apply to incoming/outgoing data</param>
        /// <typeparam name="T">The object type to specify the serializer for</typeparam>
        /// <typeparam name="TSerializer">The serializer to apply</typeparam>
        public static void Register<T, TSerializer>(DataDirection direction = DataDirection.All)
            where T : class
            where TSerializer : ApiSerializer, new()
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

        /// <summary>
        /// Resolves the <see cref="ApiSerializer"/> for the type provided
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        public ApiSerializer Resolve<T>(DataDirection direction)
        {
            return Resolve(typeof(T), direction);
        }

        /// <summary>
        /// Resolves the <see cref="ApiSerializer"/> for the type provided
        /// </summary>
        public ApiSerializer Resolve(Type objectType, DataDirection direction)
        {
            if (!objectType.IsClass)
            {
                // at this point in time, we only support non-generic class
                // this is because this isn't designed to filter generic types
                return Default;
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
                return _serializerCache.GetOrAdd(serializerType, _ => (ApiSerializer)Activator.CreateInstance(serializerType));
            }

            // use generic
            return Default;
        }

        /// <summary>
        /// Configures the specified <see cref="TSerializer"/>, creating a client-specific instance if needed
        /// </summary>
        /// <param name="options">The options to set</param>
        /// <typeparam name="TSerializer">The <see cref="ApiSerializer"/> to configure</typeparam>
        public void Configure<TSerializer>(Action<TSerializer> options) where TSerializer : ApiSerializer
        {
            if (Default.GetType() == typeof(TSerializer))
            {
                options?.Invoke((TSerializer)Default);
            }
            else if (DeserializerMap.ContainsValue(typeof(TSerializer)) || SerializerMap.ContainsValue(typeof(TSerializer)))
            {
                var serializer = _serializerCache.GetOrAdd(typeof(TSerializer), _ => Activator.CreateInstance<TSerializer>());
                options?.Invoke((TSerializer)serializer);
            }
            else
            {
                throw new ArgumentException("The specified serializer was not registered anywhere. It needs to be registered or set as the default before configuration can occur", nameof(TSerializer));
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
