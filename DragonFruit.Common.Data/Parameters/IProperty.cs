// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#nullable enable

namespace DragonFruit.Common.Data.Parameters
{
    public interface IProperty
    {
        string? Name { get; set; }

        CollectionConversionMode CollectionHandling { get; set; }

        string? CollectionSeparator { get; set; }
    }
}
