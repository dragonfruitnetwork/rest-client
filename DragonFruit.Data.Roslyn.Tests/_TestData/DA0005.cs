using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData;

/// <summary>
/// Dummy request with private getter/private method (DA0005)
/// </summary>
public partial class DA0005 : ApiRequest
{
    [RequestParameter]
    public string Parameter { private get; set; }
    
    [RequestParameter]
    private bool IsParameterSet() => Parameter != null;
}