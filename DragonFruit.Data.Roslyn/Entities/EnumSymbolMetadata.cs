// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;
using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn.Entities
{
    internal class EnumSymbolMetadata : PropertySymbolMetadata
    {
        public override RequestSymbolType Type => RequestSymbolType.Enum;

        public EnumSymbolMetadata(ISymbol symbol, ITypeSymbol returnType, string parameterName)
            : base(symbol, returnType, parameterName)
        {
        }

        public EnumOption EnumOption { get; set; }
    }
}
