using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DragonFruit.Data.Roslyn.Fixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DragonFruit.Data.Roslyn.Tests
{
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

        public static readonly TheoryData<string, DiagnosticResult[]> AnalyzerDetectionData = new()
        {
            {
                // DA0002: nested class
                "DA0002.cs", new[] { DiagnosticResult.CompilerError(ApiRequestAnalyzer.NestedClassNotAllowedRule.Id).WithSpan(10, 30, 10, 40).WithArguments("DA0002_Req") }
            },
            {
                // DA0003: property has no getter
                "DA0003.cs", new[] { DiagnosticResult.CompilerWarning(ApiRequestAnalyzer.PropertyNoGetterRule.Id).WithSpan(14, 20, 14, 26).WithArguments("Param2") }
            },
            {
                // DA0004: property or method not in apirequest
                "DA0004.cs", new[]
                {
                    // method not in apirequest
                    DiagnosticResult.CompilerWarning(ApiRequestAnalyzer.PropertyOrMethodNotInApiRequestRule.Id).WithSpan(11, 23, 11, 32).WithArguments("NotAParam"),

                    // property not in apirequest
                    DiagnosticResult.CompilerWarning(ApiRequestAnalyzer.PropertyOrMethodNotInApiRequestRule.Id).WithSpan(14, 20, 14, 28).WithArguments("TestData"),
                }
            },
            {
                // DA0005: property or method is inaccessible
                "DA0005.cs", new[]
                {
                    // private getter with public setter
                    DiagnosticResult.CompilerWarning(ApiRequestAnalyzer.PropertyOrMethodInaccessibleRule.Id).WithSpan(11, 19, 11, 28).WithArguments("Parameter"),

                    // private method
                    DiagnosticResult.CompilerWarning(ApiRequestAnalyzer.PropertyOrMethodInaccessibleRule.Id).WithSpan(14, 18, 14, 32).WithArguments("IsParameterSet"),
                }
            },
            {
                // DA0006: method returns void
                "DA0006.cs", new[]
                {
                    DiagnosticResult.CompilerError(ApiRequestAnalyzer.MethodReturnsVoidRule.Id).WithSpan(11, 17, 11, 28).WithArguments("GetUserData")
                }
            },
            {
                // DA0007: method has parameters
                "DA0007.cs", new[]
                {
                    DiagnosticResult.CompilerError(ApiRequestAnalyzer.MethodHasParametersRule.Id).WithSpan(11, 19, 11, 25).WithArguments("UserId")
                }
            }
        };

        [Theory]
        [MemberData(nameof(AnalyzerDetectionData))]
        public async Task TestRequestAnalysisDetections(string fileName, DiagnosticResult[] diagnosticResults)
        {
            var test = new CSharpAnalyzerTest<ApiRequestAnalyzer, DefaultVerifier>
            {
                TestCode = await File.ReadAllTextAsync(Path.Combine(_testDataPath, fileName))
            };

            test.ExpectedDiagnostics.AddRange(diagnosticResults);

            await PerformTest(test);
        }

        private async Task PerformTest(AnalyzerTest<DefaultVerifier> test)
        {
            var content = ("Common.cs", await File.ReadAllTextAsync(Path.Combine(_testDataPath, "Common.cs")));

            // add common.cs to test sources
            test.TestState.Sources.Add(content);

            if (test is CodeFixTest<DefaultVerifier> verifier)
            {
                verifier.FixedState.Sources.Add(content);
            }

            await test.RunAsync();
        }

        private static string GetSolutionRoot()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            while (Directory.EnumerateFiles(currentDirectory).All(x => Path.GetFileName(x) != "DragonFruit.Data.sln"))
            {
                currentDirectory = Path.Combine(currentDirectory, "..");
            }

            return Path.GetFullPath(currentDirectory);
        }
    }
}
