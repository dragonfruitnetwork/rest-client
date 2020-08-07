// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DragonFruit.Common.Data.Utils
{
    public class ApiClientHeaderCollection
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        private readonly ConcurrentQueue<ApiClientHeaderChange> _changes = new ConcurrentQueue<ApiClientHeaderChange>();

        /// <summary>
        /// Get the specified value for the key provided. Returns <value>null</value> if the header wasn't found
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

            set => Add(key, value);
        }

        /// <summary>
        /// Adds a key-value pair to the dictionary
        /// </summary>
        private void Add(string key, string value)
        {
            _changes.Enqueue(new ApiClientHeaderChange(key, value, false));
        }

        /// <summary>
        /// Removes a key-value pair to the dictionary
        /// </summary>
        private void Remove(string key)
        {
            _changes.Enqueue(new ApiClientHeaderChange(key, null, true));
        }

        internal bool ChangesAvailable => _changes.Any();

        /// <summary>
        /// Processes the changes from <see cref="_changes"/> and applies them to <see cref="_values"/>
        /// </summary>
        internal void ProcessChanges()
        {
            while (_changes.TryDequeue(out var change))
            {
                if (change.Remove)
                {
                    _values.Remove(change.Key);
                }
                else
                {
                    _values[change.Key] = change.Value;
                }
            }
        }

        /// <summary>
        /// Applies the <see cref="KeyValuePair{TKey,TValue}"/>s to the provided <see cref="HttpClient"/>
        /// </summary>
        internal void ApplyTo(HttpClient client)
        {
            client.DefaultRequestHeaders.Clear();

            foreach (var header in _values)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }
}
