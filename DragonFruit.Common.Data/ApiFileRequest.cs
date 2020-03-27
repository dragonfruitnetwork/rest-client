using System;
using System.Collections.Generic;
using System.Text;

namespace DragonFruit.Common.Data
{
    public abstract class ApiFileRequest : ApiRequest
    {
        public override Methods Method => Methods.Get;

        public abstract string Destination { get; }
    }
}
