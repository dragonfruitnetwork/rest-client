// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            Register<TSerializer>(typeof(T), direction);
        }

        /// <summary>
        /// Registers a serializer for the specified type. This applies to all <see cref="ApiClient"/>s
        /// </summary>
        /// <param name="targetType">The object type to specify the serializer for</param>
        /// <param name="direction">Whether this serializer should apply to incoming/outgoing data</param>
        /// <typeparam name="TSerializer">The serializer to apply</typeparam>
        public static void Register<TSerializer>(Type targetType, DataDirection direction = DataDirection.All) where TSerializer : ApiSerializer, new()
        {
            if (!targetType.IsClass)
            {
                throw new ArgumentException($"{targetType.Name} is not a class", nameof(targetType));
            }

            if ((direction & DataDirection.In) == DataDirection.In)
            {
                DeserializerMap[targetType] = typeof(TSerializer);
            }

            if ((direction & DataDirection.Out) == DataDirection.Out)
            {
                SerializerMap[targetType] = typeof(TSerializer);
            }
        }

        /// <summary>
        /// Removes the registered serializer for the type. This applies to all <see cref="ApiClient"/>s
        /// </summary>
        /// <param name="direction">Whether this serializer should be removed from incoming/outgoing data</param>
        /// <typeparam name="T">The object type to remove the serializer for</typeparam>
        public static void Unregister<T>(DataDirection direction = DataDirection.All) where T : class
        {
            Unregister(typeof(T), direction);
        }

        /// <summary>
        /// Removes the registered serializer for the type. This applies to all <see cref="ApiClient"/>s
        /// </summary>
        /// <param name="targetType">The object type to remove the serializer for</param>
        /// <param name="direction">Whether this serializer should be removed from incoming/outgoing data</param>
        public static void Unregister(Type targetType, DataDirection direction = DataDirection.All)
        {
            if (!targetType.IsClass)
            {
                throw new ArgumentException($"{targetType.Name} is not a class", nameof(targetType));
            }

            if ((direction & DataDirection.In) == DataDirection.In)
            {
                DeserializerMap.Remove(targetType);
            }

            if ((direction & DataDirection.Out) == DataDirection.Out)
            {
                SerializerMap.Remove(targetType);
            }
        }

        /// <summary>
        /// Resolves the <see cref="ApiSerializer"/> for the type provided
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        public ApiSerializer Resolve<T>(DataDirection direction) where T : class
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

            Type serializerType;

            // if the map has the type registered, check the type in cache
            if (mapping.TryGetValue(objectType, out serializerType) || (objectType.IsConstructedGenericType && mapping.TryGetValue(objectType.GetGenericTypeDefinition(), out serializerType)))
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

    [Flags]
    public enum DataDirection
    {
        /// <summary>
        /// Applies to incoming data (deserialization)
        /// </summary>
        In = 1,

        /// <summary>
        /// Applies to outgoing data (serialization)
        /// </summary>
        Out = 2,

        /// <summary>
        /// Applies to all data directions
        /// </summary>
        All = In | Out
    }
}