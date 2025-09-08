using System;

namespace Waldhari.Core.Persistence
{
    /// <summary>
    /// Provides a contract for persistence operations, allowing data to be saved to and loaded from storage.
    /// Implementations should handle serialization and deserialization of objects to a persistent medium.
    /// </summary>
    public interface IPersistenceService
    {
        /// <summary>
        /// Loads an object of type T from storage using the specified file name.
        /// </summary>
        /// <typeparam name="T">The type of object to load. Must be a reference type.</typeparam>
        /// <param name="fileName">The name of the file to load from, without extension.</param>
        /// <returns>The deserialized object of type T, or null if the file doesn't exist.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs during loading or deserialization.</exception>
        T Load<T>(string fileName) where T : class;

        /// <summary>
        /// Saves an object of type T to storage using the specified file name.
        /// If the file already exists, it will be overwritten.
        /// </summary>
        /// <typeparam name="T">The type of object to save. Must be a reference type.</typeparam>
        /// <param name="fileName">The name of the file to save to, without extension.</param>
        /// <param name="data">The object to serialize and save.</param>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs during saving or serialization.</exception>
        void Save<T>(string fileName, T data) where T : class;

        /// <summary>
        /// Checks whether a file with the specified name exists in storage.
        /// </summary>
        /// <param name="fileName">The name of the file to check for, without extension.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        bool Exists(string fileName);

        /// <summary>
        /// Deletes a file from storage if it exists.
        /// Does nothing if the file doesn't exist.
        /// </summary>
        /// <param name="fileName">The name of the file to delete, without extension.</param>
        void Delete(string fileName);
    }
}