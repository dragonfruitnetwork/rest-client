// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;

#endregion

namespace DragonFruit.Data.Headers
{
    public class HeaderCollection
    {
        private readonly ApiClient _client;
        private readonly ConcurrentDictionary<string, string> _values;

        public HeaderCollection(ApiClient client)
        {
            _values = new ConcurrentDictionary<string, string>();
            _client = client;
        }

        /// <summary>
        /// Gets or sets the specified value for the key provided.
        /// <para>
        /// Getting the value for a non-existent key will return <value>null</value>
        /// </para>
        /// <para>
        /// To queue a removal, pass <value>null</value> as the value
        /// </para>
        /// </summary>
        /// <remarks>
        /// This will check the queued changes first, and if there are no matching changes, attempt to find it in the "live" headers.
        /// </remarks>
        public string this[string key]
        {
            get
            {
                _values.TryGetValue(key, out var value);
                return value;
            }

            set
            {
                _values[key] = value;
                _client.RequestClientReset(false);
            }
        }

        /// <summary>
        /// Clears all queued changes and queues all active headers to be removed
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }

        /// <summary>
        /// Applies the <see cref="KeyValuePair{TKey,TValue}"/>s to the provided <see cref="HttpClient"/>
        /// </summary>
        internal void ApplyTo(HttpClient client)
        {
            client.DefaultRequestHeaders.Clear();

            foreach (var header in _values) client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }
}
