// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Requests
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FormBodyTypeAttribute : Attribute
    {
        public FormBodyTypeAttribute(FormBodyType bodyType)
        {
            BodyType = bodyType;
        }

        public FormBodyType BodyType { get; }
    }
}
