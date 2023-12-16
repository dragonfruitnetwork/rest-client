// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Net.Http;

namespace DragonFruit.Data.Requests
{
    /// <summary>
    /// Marks the property or method as being the body of the request.
    /// If the return type does not derive from <see cref="HttpContent"/>, the body will be serialized using the client
    /// </summary>
    /// <remarks>
    /// There must not be any more than one property/method that is decorated with this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class RequestBodyAttribute : Attribute
    {
    }
}
