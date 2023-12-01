// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Data.Requests
{
    public class QueryParameterAttribute : RequestParameterAttribute
    {
        public QueryParameterAttribute(string name)
            : base(ParameterType.Query, name)
        {
        }
    }

    public class FormParameterAttribute : RequestParameterAttribute
    {
        public FormParameterAttribute(string name)
            : base(ParameterType.Form, name)
        {
        }
    }
}
