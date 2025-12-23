// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Tests.Requests
{
    public partial class NullableEnumRequest : ApiRequest
    {
        public override string RequestPath => "https://example.com";

        // Query parameters with nullable enums
        [RequestParameter(ParameterType.Query, "status")]
        public NullableTestEnum? QueryEnum { get; set; }

        [RequestParameter(ParameterType.Query, "status_numeric")]
        [EnumOptions(EnumOption.Numeric)]
        public NullableTestEnum? QueryEnumNumeric { get; set; }

        [RequestParameter(ParameterType.Query, "status_lower")]
        [EnumOptions(EnumOption.StringLower)]
        public NullableTestEnum? QueryEnumLower { get; set; }

        // Header with nullable enum
        [RequestParameter(ParameterType.Header, "X-Status")]
        public NullableTestEnum? HeaderEnum { get; set; }

        [RequestParameter(ParameterType.Header, "X-Status-Numeric")]
        [EnumOptions(EnumOption.Numeric)]
        public NullableTestEnum? HeaderEnumNumeric { get; set; }
    }

    public enum NullableTestEnum
    {
        Active = 1,
        Inactive = 2,
        Pending = 3
    }
}
