using System;
using System.IO;
using System.Xml.Serialization;

namespace Waldhari.Core.Persistence
{
    /// <summary>
    /// XML-based implementation of IPersistenceService that stores data as XML files.
    /// Files are stored in a "Waldhari" subdirectory within the GTA V scripts directory.
    /// </summary>
    public class XmlPersistenceService : IPersistenceService
    {
        private readonly string _basePath;
        private readonly string _extension;

        /// <summary>
        /// Initializes a new instance of the XmlPersistenceService.
        /// Creates the storage directory if it doesn't exist.
        /// </summary>
        /// <param name="extension">The file extension to use for saved files (default: "xml").</param>
        public XmlPersistenceService(string extension = "xml")
        {
            // BaseDirectory = GTAV scripts directory
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Waldhari");
            _extension = extension;

            // Ensure the storage directory exists
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                Core.Logger.Info($"Created persistence directory: {_basePath}");
            }

            Core.Logger.Info($"XmlPersistenceService initialized with extension '{_extension}' in directory: {_basePath}");
        }
        
        /// <summary>
        /// Constructs the full file path by combining base path, filename, and extension.
        /// </summary>
        /// <param name="fileName">The filename without extension.</param>
        /// <returns>The complete file path.</returns>
        private string GetFullPath(string fileName) => Path.Combine(_basePath, $"{fileName}.{_extension}");

        /// <summary>
        /// Loads an object of type T from an XML file.
        /// </summary>
        /// <typeparam name="T">The type of object to load.</typeparam>
        /// <param name="fileName">The name of the file to load from, without extension.</param>
        /// <returns>The deserialized object, or null if the file doesn't exist.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public T Load<T>(string fileName) where T : class
        {
            try
            {
                var fullPath = GetFullPath(fileName);

                if (!File.Exists(fullPath))
                {
                    Core.Logger.Debug($"File not found for loading: {fileName}.{_extension}");
                    return null;
                }

                Core.Logger.Debug($"Loading {typeof(T).Name} from file: {fileName}.{_extension}");
                
                using (var fs = new FileStream(fullPath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    var result = serializer.Deserialize(fs) as T;
                    
                    Core.Logger.Info($"Successfully loaded {typeof(T).Name} from file: {fileName}.{_extension}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Failed to load {typeof(T).Name} from file {fileName}.{_extension}: {ex.Message}");
                throw new InvalidOperationException($"Error loading file {fileName}", ex);
            }
        }

        /// <summary>
        /// Saves an object of type T to an XML file.
        /// Overwrites the file if it already exists.
        /// </summary>
        /// <typeparam name="T">The type of object to save.</typeparam>
        /// <param name="fileName">The name of the file to save to, without extension.</param>
        /// <param name="data">The object to serialize and save.</param>
        /// <exception cref="InvalidOperationException">Thrown when serialization fails.</exception>
        public void Save<T>(string fileName, T data) where T : class
        {
            try
            {
                Core.Logger.Debug($"Saving {typeof(T).Name} to file: {fileName}.{_extension}");
                
                var fullPath = GetFullPath(fileName);
                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(fs, data);
                }
                
                Core.Logger.Info($"Successfully saved {typeof(T).Name} to file: {fileName}.{_extension}");
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Failed to save {typeof(T).Name} to file {fileName}.{_extension}: {ex.Message}");
                throw new InvalidOperationException($"Error saving file {fileName}", ex);
            }
        }

        /// <summary>
        /// Checks if a file exists in the storage directory.
        /// </summary>
        /// <param name="fileName">The name of the file to check for, without extension.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public bool Exists(string fileName)
        {
            var fullPath = GetFullPath(fileName);
            var exists = File.Exists(fullPath);
            
            Core.Logger.Debug($"File existence check for {fileName}.{_extension}: {exists}");
            return exists;
        }

        /// <summary>
        /// Deletes a file from the storage directory if it exists.
        /// Does nothing if the file doesn't exist.
        /// </summary>
        /// <param name="fileName">The name of the file to delete, without extension.</param>
        public void Delete(string fileName)
        {
            var fullPath = GetFullPath(fileName);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Core.Logger.Info($"Deleted file: {fileName}.{_extension}");
            }
            else
            {
                Core.Logger.Debug($"Cannot delete file {fileName}.{_extension}: file does not exist");
            }
        }
    }
}