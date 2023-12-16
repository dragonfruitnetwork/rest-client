// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using Microsoft.CodeAnalysis;

namespace DragonFruit.Data.Roslyn.Entities
{
    internal class SymbolMetadata
    {
        public SymbolMetadata(ISymbol symbol, ITypeSymbol returnType)
        {
            ReturnType = returnType;
            Symbol = symbol;
        }

        public int Depth { get; set; }

        public ISymbol Symbol { get; }
        public ITypeSymbol ReturnType { get; }

        public string Accessor => Symbol switch
        {
            IPropertySymbol propertySymbol when Symbol.IsStatic => $"{propertySymbol.ContainingType.Name}.{propertySymbol.Name}",
            IPropertySymbol propertySymbol => $"this.{propertySymbol.Name}",

            _ => Symbol.IsStatic ? $"{Symbol.ContainingType.Name}.{Symbol.Name}" : $"this.{Symbol.Name}()"
        };

        public bool Nullable => ReturnType.IsReferenceType || (ReturnType.IsValueType && ReturnType.NullableAnnotation == NullableAnnotation.Annotated);
    }
}
