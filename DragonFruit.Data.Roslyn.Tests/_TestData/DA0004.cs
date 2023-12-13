using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData
{
    /// <summary>
    /// Parameters not in an ApiRequest (DA0004)
    /// </summary>
    public partial class DA0004
    {
        [RequestParameter]
        public string NotAParam { get; set; }

        [RequestParameter]
        public int TestData() => 10;
    }
}