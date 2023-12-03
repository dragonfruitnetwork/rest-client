// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Roslyn.References;
using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn.Generators
{
    public class RequestSymbolMetadata
    {
        public int Depth { get; set; }

        public string Name { get; set; }
        public string Accessor { get; set; }

        public ISymbol Symbol { get; set; }

        public EnumOptions? EnumOptions { get; set; }
        public CollectionOptions? CollectionOptions { get; set; }
    }
}
