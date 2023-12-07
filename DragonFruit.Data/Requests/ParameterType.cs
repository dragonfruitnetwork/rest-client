// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.Data.Requests
{
    /// <summary>
    /// Describes the destination location of the parameter being decorated
    /// </summary>
    public enum ParameterType
    {
        Query = 1,
        Form = 2,
        Header = 3
    }
}
