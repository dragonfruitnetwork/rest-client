// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;

#endregion

#nullable enable

namespace DragonFruit.Data.Parameters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class FormParameter : Attribute, IProperty
    {
        public FormParameter()
        {
        }

        public FormParameter(string name)
        {
            Name = name;
        }

        public FormParameter(string name, EnumHandlingMode enumHandling)
            : this(name)
        {
            EnumHandling = enumHandling;
        }

        public FormParameter(string name, CollectionConversionMode collectionHandling)
            : this(name)
        {
            CollectionHandling = collectionHandling;
        }

        public string? Name { get; set; }
        public CollectionConversionMode? CollectionHandling { get; set; }
        public EnumHandlingMode? EnumHandling { get; set; }

        public string? CollectionSeparator { get; set; }
    }
}
