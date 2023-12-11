using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<DragonFruit.Data.Roslyn.Analyzers.ApiRequestAnalyzer, DragonFruit.Data.Roslyn.Analyzers.ApiRequestClassFixProvider>;

namespace DragonFruit.Data.Roslyn.Tests;

public class ApiRequestAnalyzerTests
{
    [Fact]
    public async Task TestNonPartialClassDetectionAndFix()
    {
        const string text = @"
namespace DragonFruit.Data;

public class ApiRequest { }
public class TestRequest : ApiRequest
{
    public string RequestPath => ""https://google.com"";
}
";

        const string newText = @"
namespace DragonFruit.Data;

public class ApiRequest { }
public partial class TestRequest : ApiRequest
{
    public string RequestPath => ""https://google.com"";
}
";

        var expectedDiagnostic = Verifier.Diagnostic().WithSpan(5, 14, 5, 25).WithArguments("TestRequest");
        await Verifier.VerifyCodeFixAsync(text, expectedDiagnostic, newText).ConfigureAwait(false);
    }
}
