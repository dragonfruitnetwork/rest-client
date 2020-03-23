using System;

namespace DragonFruit.Common.Data.Exceptions
{
    public class NullRequestException : Exception
    {
        public NullRequestException()
            : base("The Request provided was null or has no path")
        {
        }
    }
}
