// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;
using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn.Generators
{
    internal class RequestSymbolMetadata
    {
        public int Depth { get; set; }

        public bool IsString { get; set; }
        public bool Nullable { get; set; }

        public ISymbol Symbol { get; set; }
        public string ParameterName { get; set; }

        public string Accessor => Symbol is IPropertySymbol ps ? $"this.{ps.Name}" : $"this.{Symbol.Name}()";
    }

    internal class EnumRequestSymbolMetadata : RequestSymbolMetadata
    {
        public EnumOption? Options { get; set; }
    }

    internal class EnumerableRequestSymbolMetadata : RequestSymbolMetadata
    {
        public string Separator { get; set; }
        public string EnumerableType { get; set; }
        public EnumerableOption? Options { get; set; }
    }
}
