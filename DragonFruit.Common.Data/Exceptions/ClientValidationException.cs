// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Common.Data.Exceptions
{
    public class ClientValidationException : Exception
    {
        public ClientValidationException(string message)
            : base(message)
        {
        }
    }
}
