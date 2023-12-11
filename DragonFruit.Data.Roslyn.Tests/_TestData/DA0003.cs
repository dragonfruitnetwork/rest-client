using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData
{
    /// <summary>
    /// Dummy request, property with no getter (DA0003)
    /// </summary>
    public class DA0003 : ApiRequest
    {
        [RequestParameter]
        public string Param1 { get; set; }

        [RequestParameter]
        public int Param2
        {
            set => Param1 = value.ToString();
        }
    }
}