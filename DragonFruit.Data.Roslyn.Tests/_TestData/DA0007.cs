using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData;

/// <summary>
/// Dummy request, method with parameters (DA0007)
/// </summary>
public partial class DA0007 : ApiRequest
{
    [RequestParameter]
    public string UserId(string originalId) => originalId.ToLowerInvariant();
}