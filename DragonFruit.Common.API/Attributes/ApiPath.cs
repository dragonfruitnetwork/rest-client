// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Common.API.Attributes
{
    public class ApiPath : Attribute
    {
        public ApiPath(string path, Methods method = Methods.Get, bool requireAuth = false, string acceptedContent = null)
        {
            Path = path;
            Method = method;
            RequireAuth = requireAuth;
            AcceptedContent = acceptedContent;
        }

        public string Path { get; }
        public Methods Method { get; }
        public bool RequireAuth { get; }
        public string AcceptedContent { get; }
    }
}