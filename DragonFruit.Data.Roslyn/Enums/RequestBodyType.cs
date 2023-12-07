// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Data.Roslyn.Enums
{
    internal enum RequestBodyType
    {
        None = 0,

        FormMultipart = 1,
        FormUriEncoded = 2,
        CustomBodyDirect = 3,
        CustomBodySerialized = 4
    }
}
