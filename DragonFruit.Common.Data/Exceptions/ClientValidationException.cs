using System;
using System.Collections.Generic;
using System.Text;

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
