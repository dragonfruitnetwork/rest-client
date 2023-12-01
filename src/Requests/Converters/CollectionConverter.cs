// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.Text;

namespace DragonFruit.Data.Requests.Converters
{
    public class CollectionConverter
    {
        // todo move to sourcegen
        public static void WriteCollectionValue<T>(IEnumerable<T> collection, string keyPrefix, CollectionOptions mode, StringBuilder destination)
        {
            switch (mode)
            {
                case CollectionOptions.Recursive:
                {
                    foreach (var item in collection)
                    {
                        destination.Append($"{keyPrefix}={item}&");
                    }

                    break;
                }

                case CollectionOptions.Unordered:
                {
                    foreach (var item in collection)
                    {
                        destination.Append($"{keyPrefix}[]={item}&");
                    }

                    break;
                }

                case CollectionOptions.Indexed:
                {
                    var index = 0;

                    foreach (var item in collection)
                    {
                        destination.Append($"{keyPrefix}[{index++}]={item}&");
                    }

                    break;
                }

                case CollectionOptions.Concatenated:
                {
                    destination.Append(string.Join(",", collection));
                    break;
                }
            }
        }
    }
}
