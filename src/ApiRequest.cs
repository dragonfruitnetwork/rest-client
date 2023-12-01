// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;

namespace DragonFruit.Data
{
    public abstract class ApiRequest
    {
        public abstract string RequestPath { get; }

        public virtual HttpMethod RequestMethod => HttpMethod.Get;
    }
}
