// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Common.Data.Parameters
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class FormParameter : Attribute, IProperty
    {
        public FormParameter(string name)
            : this(name, CollectionConversionMode.Unordered)
        {
        }

        public FormParameter(string name, CollectionConversionMode collectionHandling)
        {
            Name = name;
            CollectionHandling = collectionHandling;
        }

        public string Name { get; }
        public CollectionConversionMode CollectionHandling { get; }
    }
}
