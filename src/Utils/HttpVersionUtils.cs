// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Data.Utils
{
    internal static class HttpVersionUtils
    {
        /// <summary>
        /// Gets the default HTTP <see cref="Version"/> suitable for the host system
        /// </summary>
        public static Version DefaultHttpVersion
        {
            get
            {
#if NETSTANDARD
                return System.Net.HttpVersion.Version11;
#else
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) && Environment.OSVersion.Version.Major < 10)
            {
                // because the WinHttpHandler exists, versions prior to Windows 10 might fallover and cause unexpected breakage.
                // if this isn't the case, a developer can simply override the version on initialisation, bypassing this check.
                return System.Net.HttpVersion.Version11;
            }

            return System.Net.HttpVersion.Version20;
#endif
            }
        }
    }
}
