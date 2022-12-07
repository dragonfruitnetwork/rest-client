// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Parameters;

namespace DragonFruit.Data
{
    public enum Methods
    {
        Get,
        Head,
        Post,
        Put,
        Patch,
        Delete,
        Trace
    }

    public enum BodyType
    {
        /// <summary>
        /// Finds all properties marked with <see cref="FormParameter"/> and creates a url-form encoded content from them 
        /// </summary>
        Encoded,

        /// <summary>
        /// Serializes the current <see cref="ApiRequest"/> and sends it as a string using the serialization method specified
        /// </summary>
        Serialized,

        /// <summary>
        /// Finds the single <see cref="RequestBody"/>-marked property and serializes it
        /// </summary>
        SerializedProperty,

        /// <summary>
        /// Tells the client to use the <see cref="ApiRequest.BodyContent"/> property override to get the content
        /// </summary>
        Custom
    }

    public enum CollectionConversionMode
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
        Ordered,

        /// <summary>
        /// The query is concatenated with a string and merged with one key (a=1,2,3)
        /// </summary>
        Concatenated
    }

    public enum EnumHandlingMode
    {
        /// <summary>
        /// Convert to the integer representation
        /// </summary>
        Numeric,

        /// <summary>
        /// Convert to string form
        /// </summary>
        String,

        /// <summary>
        /// Convert to lowercase string form
        /// </summary>
        StringLower,

        /// <summary>
        /// Convert to uppercase string form
        /// </summary>
        StringUpper
    }
}
