using System;
using System.Security.Cryptography;
using System.Text;
using DragonFruit.Data.Requests;

namespace DragonFruit.Data.Roslyn.Tests.TestData;

/// <summary>
/// Dummy request, method with parameters (DA0007)
/// </summary>
public class DA0007 : ApiRequest
{
    [RequestParameter]
    public string UserId(string originalId) => Convert.ToHexString(MD5.HashData(Encoding.ASCII.GetBytes(originalId)));
}