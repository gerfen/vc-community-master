using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Packaging;
using VirtoCommerce.Platform.Data.Packaging;

namespace VirtoCommerce.Platform.Tests
{
    [TestClass]
    public class ZipPackageServiceTests
    {
        private static string _tempDir;

        [TestMethod]
        public void ValidatePackage()
        {
            var service = GetPackageService();

            // Load module descriptor from package
            var module = service.OpenPackage(@"source\Sample.TestModule1_1.0.0.zip");
            WriteModuleLine(module);

            // Check if all dependencies are installed
            var dependencyErrors = service.GetDependencyErrors(module);
        }

        [TestMethod]
        public void InstallUpdateUninstall()
        {
            const string package1 = "TestModule1";
            const string package2 = "TestModule2";

            var service = GetPackageService();
            var progress = new Progress<ProgressMessage>(WriteProgressMessage);

            ListModules(service);

            service.Install(package2, "1.0.0", progress);
            ListModules(service);

            service.Install(package1, "1.0.0", progress);
            ListModules(service);

            service.Install(package2, "1.0.0", progress);
            ListModules(service);

            service.Update(package2, "1.1.0", progress);
            ListModules(service);

            service.Update(package1, "1.1.0", progress);
            ListModules(service);

            service.Update(package2, "1.1.0", progress);
            ListModules(service);

            service.Uninstall(package1, progress);
            ListModules(service);

            service.Uninstall(package2, progress);
            ListModules(service);

            service.Uninstall(package1, progress);
            ListModules(service);
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Directory.Delete(_tempDir, true);
        }

        private static IPackageService GetPackageService()
        {
            var sourcePath = Path.GetFullPath("source");
            var modulesPath = Path.Combine(_tempDir, @"modules");
            var packagesPath = Path.Combine(_tempDir, @"packages");

            var manifestProvider = new ModuleManifestProvider(modulesPath);

            var service = new ZipPackageService(null, manifestProvider, packagesPath, sourcePath);
            return service;
        }

        static void ListModules(IPackageService service)
        {
            var modules = service.GetModules();
            Debug.WriteLine("Modules count: {0}", modules.Length);

            foreach (var module in modules)
            {
                WriteModuleLine(module);
            }
        }

        static void WriteModuleLine(ModuleDescriptor module)
        {
            Debug.WriteLine("{0} {1}", module.Id, module.Version);
        }

        static void WriteProgressMessage(ProgressMessage message)
        {
            Debug.WriteLine("{0}: {1}", message.Level, message.Message);
        }
    }
}
