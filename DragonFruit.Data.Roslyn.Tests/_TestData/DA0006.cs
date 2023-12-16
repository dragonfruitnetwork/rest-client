using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData;

/// <summary>
/// Dummy request with a void return type (DA0006)
/// </summary>
public partial class DA0006 : ApiRequest
{
    [RequestParameter]
    public void GetUserData()
    {
        
    }
}