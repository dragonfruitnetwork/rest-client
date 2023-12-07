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

        public string Accessor => Symbol is IPropertySymbol ps ? $"this.{ps.Name}" : $"this.{Symbol.Name}()";
        public bool Nullable => ReturnType.IsReferenceType || (ReturnType.IsValueType && ReturnType.NullableAnnotation == NullableAnnotation.Annotated);
    }
}
