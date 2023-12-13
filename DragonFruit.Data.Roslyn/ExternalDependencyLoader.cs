// DragonFruit.Data Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DragonFruit.Data.Roslyn
{
    public static class ExternalDependencyLoader
    {
        private static bool _loaded;

        public static void RegisterDependencyLoader()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
        }

        // derived from https://stackoverflow.com/a/67074009
        private static Assembly HandleAssemblyResolve(object _, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == name.FullName);

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            using var resourceStream = typeof(ExternalDependencyLoader).Assembly.GetManifestResourceStream($"{typeof(ExternalDependencyLoader).Namespace}.{name.Name}.dll");

            if (resourceStream == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();
            resourceStream.CopyTo(memoryStream);

            return Assembly.Load(memoryStream.ToArray());
        }
    }
}
