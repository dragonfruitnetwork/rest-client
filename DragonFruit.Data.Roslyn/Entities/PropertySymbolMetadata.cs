// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Roslyn.Enums;
using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn.Entities
{
    internal class PropertySymbolMetadata : SymbolMetadata
    {
        private readonly RequestSymbolType _type;

        public virtual RequestSymbolType Type => _type;

        public PropertySymbolMetadata(ISymbol symbol, ITypeSymbol returnType, string parameterName, RequestSymbolType type = RequestSymbolType.Standard)
            : base(symbol, returnType)
        {
            _type = type;
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
