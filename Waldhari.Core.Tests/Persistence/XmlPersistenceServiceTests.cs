using System;
using NUnit.Framework;
using System.IO;
using System.Windows.Forms;
using Waldhari.Core.Persistence;

namespace Waldhari.Core.Tests.Persistence
{
    public class XmlPersistenceServiceTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public class TestData
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
        
        [TearDown]
        public void Cleanup()
        {
            string scriptsPath = Path.Combine(Application.StartupPath, "scripts");

            if (Directory.Exists(scriptsPath))
            {
                Directory.Delete(scriptsPath, recursive: true);
            }
        }

        

        [Test]
        public void Save_And_Load_Works()
        {
            var data = new TestData { Name = "Test", Value = 42 };
            var service = new XmlPersistenceService();
            service.Save("test", data);

            var loaded = service.Load<TestData>("test");

            Assert.NotNull(loaded);
            Assert.AreEqual("Test", loaded.Name);
            Assert.AreEqual(42, loaded.Value);
        }

        [Test]
        public void Load_NonExistingFile_ReturnsNull()
        {
            var service = new XmlPersistenceService();
            var result = service.Load<TestData>("notfound");
            Assert.IsNull(result);
        }

        [Test]
        public void Exists_ReturnsTrue_WhenFileExists()
        {
            var data = new TestData { Name = "ExistsCheck", Value = 10 };
            var service = new XmlPersistenceService();
            service.Save("exists", data);

            Assert.IsTrue(service.Exists("exists"));
        }

        [Test]
        public void Delete_RemovesFile()
        {
            var data = new TestData { Name = "DeleteCheck", Value = 99 };
            
            var service = new XmlPersistenceService();
            service.Save("delete", data);

            service.Delete("delete");

            Assert.IsFalse(service.Exists("delete"));
        }
        
        [Test]
        public void Constructor_CreatesDirectory_WhenNotExists()
        {
            string tempPath = Path.Combine(Application.StartupPath + "/scripts/Waldhari");
            Assert.False(Directory.Exists(tempPath));

            var service = new XmlPersistenceService();

            Assert.True(Directory.Exists(tempPath));
        }
        
        [Test]
        public void Save_Throws_InvalidOperationException_OnInvalidPath()
        {
            var service = new XmlPersistenceService();
            var data = new TestData { Name = "Test", Value = 1 };

            string invalidPath = @"<>:\invalid";

            var ex = Assert.Throws<InvalidOperationException>(() => service.Save(invalidPath, data));
            Assert.IsNotNull(ex.InnerException);
        }
        
        [Test]
        public void Load_Throws_InvalidOperationException_OnCorruptedFile()
        {
            string tempDir = Path.Combine(Application.StartupPath + "/scripts/Waldhari");
            Directory.CreateDirectory(tempDir);
            string filePath = Path.Combine(tempDir, "corrupt.xml");

            File.WriteAllText(filePath, "notxml");

            var service = new XmlPersistenceService();
            var ex = Assert.Throws<InvalidOperationException>(() => service.Load<TestData>("corrupt"));
            Assert.IsNotNull(ex.InnerException);
        }


        

    }
}