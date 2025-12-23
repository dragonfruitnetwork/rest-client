// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Requests
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FormBodyTypeAttribute(FormBodyType bodyType) : Attribute
    {
        public FormBodyType BodyType { get; } = bodyType;
    }
}
