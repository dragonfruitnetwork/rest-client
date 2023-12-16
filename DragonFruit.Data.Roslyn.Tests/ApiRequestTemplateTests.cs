// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO;
using System.Threading.Tasks;
using Scriban;
using Xunit;

namespace DragonFruit.Data.Roslyn.Tests
{
    public class ApiRequestTemplateTests
    {
        [Fact]
        public async Task TestTemplateParse()
        {
            var assembly = typeof(ApiRequestSourceGenerator).Assembly;
            using var template = assembly.GetManifestResourceStream(ApiRequestSourceGenerator.TemplateName);

            Assert.NotNull(template);

            using var templateReader = new StreamReader(template);
            var templateText = await templateReader.ReadToEndAsync();

            Assert.True(templateText.Length > 0);

            var templateAst = Template.ParseLiquid(templateText);

            Assert.False(templateAst.HasErrors);
        }
    }
}
