// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;
using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn.Entities
{
    internal class EnumerableSymbolMetadata : ParameterSymbolMetadata
    {
        public override RequestSymbolType Type => RequestSymbolType.Enumerable;

        public EnumerableSymbolMetadata(ISymbol symbol, ITypeSymbol returnType, string parameterName)
            : base(symbol, returnType, parameterName)
        {
        }

        public string Separator { get; set; }
        public EnumerableOption EnumerableOption { get; set; }

        public bool IsByteArray => ReturnType is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte };
    }
}
