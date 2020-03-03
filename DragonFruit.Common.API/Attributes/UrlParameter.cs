// DragonFruit.Common Copyright 2020 DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.Common.API.Attributes
{
    public class UrlParameter : Attribute
    {
        public UrlParameter(uint position = 0)
        {
            Position = position;
        }

        public uint Position { get; }
    }
}