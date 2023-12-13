// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn.Entities
{
    internal class PropertySymbolMetadata : SymbolMetadata
    {
        public virtual RequestSymbolType Type => RequestSymbolType.Standard;

        public PropertySymbolMetadata(ISymbol symbol, ITypeSymbol returnType, string parameterName)
            : base(symbol, returnType)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
        public SpecialRequestParameter? SpecialRequestParameter { get; set; }
    }
}
