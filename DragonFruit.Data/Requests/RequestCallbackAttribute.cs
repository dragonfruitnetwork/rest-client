// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Requests
{
    /// <summary>
    /// Marks the annotated method as a callback before returning the final request object
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestCallbackAttribute : Attribute
    {
        /// <summary>
        /// The order to invoke the callback.
        /// Lower value results in earlier execution.
        /// </summary>
        public int Order { get; } = 0;
    }
}
