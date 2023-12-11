using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData
{
    /// <summary>
    /// Dummy request with no partial class modifier (DA0001)
    /// </summary>
    public class DA0001 : ApiRequest
    {
        [RequestParameter]
        public string TestParam { get; set; }
    }
}