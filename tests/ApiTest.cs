// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

#region

using DragonFruit.Data.Serializers.Newtonsoft;

#endregion

namespace DragonFruit.Data.Tests
{
    public abstract class ApiTest
    {
        protected static readonly ApiClient Client = new ApiClient<ApiJsonSerializer>();
    }
}
