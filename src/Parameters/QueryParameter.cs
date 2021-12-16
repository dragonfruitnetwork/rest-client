// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;

#endregion

#nullable enable

namespace DragonFruit.Data.Parameters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class QueryParameter : Attribute, IProperty
    {
        public QueryParameter()
        {
        }

        public QueryParameter(string name)
        {
            Name = name;
        }

        public QueryParameter(string name, EnumHandlingMode enumHandling)
            : this(name)
        {
            EnumHandling = enumHandling;
        }

        public QueryParameter(string name, CollectionConversionMode collectionConversionMode)
            : this(name)
        {
            CollectionHandling = collectionConversionMode;
        }

        public string? Name { get; set; }
        public CollectionConversionMode? CollectionHandling { get; set; }
        public EnumHandlingMode? EnumHandling { get; set; }

        public string? CollectionSeparator { get; set; }
    }
}
