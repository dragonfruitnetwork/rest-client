// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn
{
    internal class RequestSymbolMetadata
    {
        public virtual int Type => (int)RequestSymbolType.Standard;

        public int Depth { get; set; }

        public bool IsString { get; set; }
        public bool Nullable { get; set; }

        public ISymbol Symbol { get; set; }
        public string ParameterName { get; set; }

        public string Accessor => Symbol is IPropertySymbol ps ? $"this.{ps.Name}" : $"this.{Symbol.Name}()";
    }

    internal class EnumRequestSymbolMetadata : RequestSymbolMetadata
    {
        public override int Type => (int)RequestSymbolType.Enum;

        public string EnumOption { get; set; }
    }

    internal class EnumerableRequestSymbolMetadata : RequestSymbolMetadata
    {
        public override int Type => (int)RequestSymbolType.Enumerable;

        public string Separator { get; set; }
        public string EnumerableOption { get; set; }
    }
}
