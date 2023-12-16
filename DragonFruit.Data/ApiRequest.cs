// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net.Http;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data
{
    /// <summary>
    /// The base class for all requests.
    /// Properties and methods decorated with <see cref="RequestParameterAttribute"/> are used to build the request.
    /// </summary>
    public abstract class ApiRequest
    {
        /// <summary>
        /// The base url of the request
        /// </summary>
        public abstract string RequestPath { get; }

        /// <summary>
        /// The <see cref="HttpMethod"/> to use when making the request
        /// </summary>
        public virtual HttpMethod RequestMethod => HttpMethod.Get;
    }
}
