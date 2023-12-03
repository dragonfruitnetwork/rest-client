// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Data.Requests
{
    public enum EnumerableOption
    {
        /// <summary>
        /// The query name is repeated and a new element created for each (a=1&a=2&a=3)
        /// </summary>
        Recursive,

        /// <summary>
        /// The query name has indexer symbols appended with no order (a[]=1&a[]=2&a[]=3)
        /// </summary>
        Unordered,

        /// <summary>
        /// The query name has indexer symbols appended explicit order inserted (a[0]=1&a[1]=2&a[2]=3)
        /// </summary>
        Indexed,

        /// <summary>
        /// The query is concatenated with a string and merged with one key (a=1,2,3)
        /// </summary>
        Concatenated
    }
}
