// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using System;
using System.Net.Http;

#endregion

namespace DragonFruit.Data.Handlers
{
    [Obsolete(nameof(InsecureSslVerificationHandler) + "is insecure and should only be used for testing/development purposes")]
    public class InsecureSslVerificationHandler : HttpClientHandler
    {
        public InsecureSslVerificationHandler()
        {
            ServerCertificateCustomValidationCallback = delegate { return true; };
        }
    }
}
