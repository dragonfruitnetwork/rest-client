// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Entities
{
    internal class RequestSymbolMetadata
    {
        public IReadOnlyDictionary<ParameterType, IList<SymbolMetadata>> Properties { get; set; }

        public SymbolMetadata BodyProperty { get; set; }

        public FormBodyType? FormBodyType { get; set; }
    }
}
