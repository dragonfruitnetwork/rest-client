// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Net.Http;

namespace DragonFruit.Common.Data.Handlers
{
    [Obsolete(nameof(InsecureSslVerificationHandler) + "is insecure and should only be used for testing/development purposes")]
    public class InsecureSslVerificationHandler : HttpClientHandler
    {
        public InsecureSslVerificationHandler()
        {
            ServerCertificateCustomValidationCallback = delegate
            {
                return true;
            };
        }
    }
}
