// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

#nullable enable

namespace DragonFruit.Common.Data.Parameters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class FormParameter : Attribute, IProperty
    {
        public FormParameter()
        {
        }

        public FormParameter(string name)
            : this(name, CollectionConversionMode.Unordered)
        {
        }

        public FormParameter(string name, CollectionConversionMode collectionHandling)
        {
            Name = name;
            CollectionHandling = collectionHandling;
        }

        public string? Name { get; set; }
        public CollectionConversionMode CollectionHandling { get; set; }

        public string? CollectionSeparator { get; set; }
    }
}
