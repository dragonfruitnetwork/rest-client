// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Requests
{
    /// <summary>
    /// Marks a property or method to be included as a parameter in the HTTP request
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class RequestParameterAttribute : Attribute
    {
        public RequestParameterAttribute(ParameterType parameterType, string name)
        {
            ParameterType = parameterType;
            Name = name;
        }

        /// <summary>
        /// The type/location the value should be stored under
        /// </summary>
        public ParameterType ParameterType { get; }

        /// <summary>
        /// The name the value should be written under
        /// </summary>
        public string Name { get; }
    }
}
