﻿using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData
{
    /// <summary>
    /// Dummy request nested in another class (DA0002)
    /// </summary>
    public partial class DA0002
    {
        public partial class DA0002_Req : ApiRequest
        {
            [RequestParameter]
            public int Id { get; set; }
        }
    }
}