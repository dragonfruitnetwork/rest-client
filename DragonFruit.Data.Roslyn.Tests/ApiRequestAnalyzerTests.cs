using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DragonFruit.Data.Roslyn.Fixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DragonFruit.Data.Roslyn.Tests;

public class ApiRequestAnalyzerTests
{
    private readonly string _testDataPath;

    public ApiRequestAnalyzerTests()
    {
        _testDataPath = Path.Combine(GetSolutionRoot(), "DragonFruit.Data.Roslyn.Tests", "_TestData");
    }

    [Fact]
    public async Task TestNonPartialClassDetectionAndFix()
    {
        var test = new CSharpCodeFixTest<ApiRequestAnalyzer, ApiRequestClassFixProvider, DefaultVerifier>
        {
            TestCode = await File.ReadAllTextAsync(Path.Combine(_testDataPath, "DA0001.cs")),
            FixedCode = await File.ReadAllTextAsync(Path.Combine(_testDataPath, "DA0001.Fix.cs")),
            ExpectedDiagnostics = { DiagnosticResult.CompilerError(ApiRequestAnalyzer.PartialClassRule.Id).WithSpan(8, 18, 8, 24).WithArguments("DA0001") }
        };

        await PerformTest(test);
    }

    private async Task PerformTest(AnalyzerTest<DefaultVerifier> test)
    {
        var content = ("Common.cs", await File.ReadAllTextAsync(Path.Combine(_testDataPath, "Common.cs")));
        test.TestState.Sources.Add(content);

        if (test is CodeFixTest<DefaultVerifier> verifier)
        {
            verifier.FixedState.Sources.Add(content);
        }

        await test.RunAsync();
    }

    private string GetSolutionRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        while (Directory.EnumerateFiles(currentDirectory).All(x => Path.GetFileName(x) != "DragonFruit.Data.sln"))
        {
            currentDirectory = Path.Combine(currentDirectory, "..");
        }

        return Path.GetFullPath(currentDirectory);
    }
}
