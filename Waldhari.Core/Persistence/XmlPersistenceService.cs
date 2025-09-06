using System;
using System.IO;
using System.Xml.Serialization;

namespace Waldhari.Core.Persistence
{
    public class XmlPersistenceService : IPersistenceService
    {
        private readonly string _basePath;
        private readonly string _extension;

        public XmlPersistenceService(string extension = "xml")
        {
            // StartupPath = GTAV directory
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari");
            _extension = extension;

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }
        
        private string GetFullPath(string fileName) => Path.Combine(_basePath, $"{fileName}.{_extension}");

        public T Load<T>(string fileName) where T : class
        {
            try
            {
                var fullPath = GetFullPath(fileName);

                if (!File.Exists(fullPath))
                    return null;
                
                using (var fs = new FileStream(fullPath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(fs) as T;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading file {fileName}", ex);
            }
        }

        public void Save<T>(string fileName, T data) where T : class
        {
            try
            {
                var fullPath = GetFullPath(fileName);
                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(fs, data);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving file {fileName}", ex);
            }
        }

        public bool Exists(string fileName)
        {
            var fullPath = GetFullPath(fileName);
            return File.Exists(fullPath);
        }

        public void Delete(string fileName)
        {
            var fullPath = GetFullPath(fileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
