// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DragonFruit.Common.Data.Utils
{
    public class HeaderCollection
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        private readonly ConcurrentQueue<HeaderChange> _changes = new ConcurrentQueue<HeaderChange>();
        private readonly object _lock = new object();

        /// <summary>
        /// Get the specified value for the key provided. Returns null if the header wasn't found
        /// </summary>
        public string this[string key]
        {
            get
            {
                if (_values.ContainsKey(key))
                {
                    return _values[key];
                }

                return _changes.FirstOrDefault(x => x.Key == key)?.Value;
            }

            set => Add(key, value);
        }

        public bool ChangesAvailable => _changes.Any();

        /// <summary>
        /// Adds a key-value pair to the dictionary
        /// </summary>
        public void Add(string key, string value)
        {
            _changes.Enqueue(new HeaderChange(key, value, false));
        }

        /// <summary>
        /// Removes a key-value pair to the dictionary
        /// </summary>
        public void Remove(string key)
        {
            _changes.Enqueue(new HeaderChange(key, null, true));
        }

        internal void ProcessChanges()
        {
            lock (_lock)
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
        }

        internal void ApplyTo(HttpClient client)
        {
            lock (_lock)
            {
                foreach (var header in _values)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }
    }
}
