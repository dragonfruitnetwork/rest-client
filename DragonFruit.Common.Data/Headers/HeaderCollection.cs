// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DragonFruit.Common.Data.Headers
{
    public class HeaderCollection
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        private readonly ConcurrentQueue<HeaderChange> _changes = new ConcurrentQueue<HeaderChange>();

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
                var lastTrackedChange = _changes.LastOrDefault(x => x.Key == key);

                if (lastTrackedChange != null)
                {
                    return lastTrackedChange.Value;
                }

                return _values.ContainsKey(key) ? _values[key] : null;
            }

            set => _changes.Enqueue(new HeaderChange(key, value));
        }

        /// <summary>
        /// Clears all queued changes and queues all active headers to be removed
        /// </summary>
        public void Clear()
        {
            while (_changes.TryDequeue(out _))
            {
                //do nothing
            }

            foreach (var item in _values)
            {
                this[item.Key] = null;
            }
        }

        /// <summary>
        /// All the keys currently in use. Ingores queued changes
        /// </summary>
        public IEnumerable<string> Keys => _values.Keys;

        internal bool ChangesAvailable => _changes.Any();

        /// <summary>
        /// Applies the <see cref="KeyValuePair{TKey,TValue}"/>s to the provided <see cref="HttpClient"/>
        /// </summary>
        internal void ProcessAndApplyTo(HttpClient client)
        {
            while (_changes.TryDequeue(out var change))
            {
                if (string.IsNullOrEmpty(change.Value))
                {
                    _values.Remove(change.Key);
                }
                else
                {
                    _values[change.Key] = change.Value;
                }
            }

            foreach (var header in _values)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }
}
