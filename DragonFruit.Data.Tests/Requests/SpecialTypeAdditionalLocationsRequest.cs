// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class SpecialTypeAdditionalLocationsRequest : ApiRequest
    {
        private readonly string[] _ids =
        [
            "5c2a5585-2682-4c60-9bc7-b11874582af6",
            "d85cf845-a35a-48e9-b629-700f77ab1c5d",
            "885f37a8-6bf6-4d52-8092-71276ab706fd"
        ];

        public override string RequestPath => "https://example.com";

        [RequestParameter(ParameterType.Form, "ids")]
        [EnumerableOptions(EnumerableOption.Concatenated)]
        public IEnumerable<string> Ids => _ids;

        [RequestParameter(ParameterType.Header, "X-User-Id-Value")]
        [EnumerableOptions(EnumerableOption.Recursive)]
        public IEnumerable<string> HeaderIds => _ids;

        [RequestParameter(ParameterType.Form, "id_opt")]
        public SourceGenTestEnum Enums => SourceGenTestEnum.One;

        [RequestParameter(ParameterType.Header, "X-User-Option")]
        [EnumOptions(EnumOption.Numeric)]
        public SourceGenTestEnum HeaderEnums => SourceGenTestEnum.Two;
    }

    public enum SourceGenTestEnum
    {
        One = 1,
        Two = 2
    }
}
