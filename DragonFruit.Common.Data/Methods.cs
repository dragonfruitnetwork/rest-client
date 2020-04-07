// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.Common.Data
{
    public enum Methods
    {
        Get,
        Head,
        Post,
        Put,
        Patch,
        Delete
    }

    public enum DataTypes
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
        /// Finds the <see cref="RequestBody"/> marked property and serializes it
        /// </summary>
        SerializedProperty,

        /// <summary>
        /// Tells the client to use the <see cref="ApiRequest.GetContent"/> property override to get the content
        /// </summary>
        Custom
    }
}