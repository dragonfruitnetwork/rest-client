// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Runtime.InteropServices;
using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.Common.Data.Tests.Handlers.AuthPreservingHandler.Objects
{
    public class AuthRequest : ApiRequest
    {
        public override string Path => "https://osu.ppy.sh/oauth/token";
        protected override Methods Method => Methods.Post;
        protected override DataTypes DataType => DataTypes.Encoded;

        [FormParameter("grant_type")]
        public string Grant => "client_credentials";

        [FormParameter("client_id")]
        public string ClientId => GetEnvironmentVar("orbit_client_id");

        [FormParameter("client_secret")]
        public string ClientSecret => GetEnvironmentVar("orbit_client_secret");

        private static string GetEnvironmentVar(string var)
        {
            var envVar = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable(var, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(var, EnvironmentVariableTarget.Machine) ?? Environment.GetEnvironmentVariable(var)
                : Environment.GetEnvironmentVariable(var);

            return envVar;
        }
    }
}
