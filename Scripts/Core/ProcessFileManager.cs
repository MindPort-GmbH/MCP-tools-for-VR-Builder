using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VRBuilder.MCP.Core
{
    /// <summary>
    /// Manages file operations for VR Builder processes.
    /// </summary>
    public static class ProcessFileManager
    {
        /// <summary>
        /// Gets the directory path for a process.
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <returns>The full directory path (e.g., StreamingAssets/Processes/ProcessName/)</returns>
        public static string GetProcessDirectory(string processName)
        {
            string streamingRoot = Application.streamingAssetsPath;
            if (string.IsNullOrEmpty(streamingRoot))
            {
                throw new InvalidOperationException("StreamingAssets path is not available");
            }

            return Path.Combine(streamingRoot, "Processes", processName);
        }

        /// <summary>
        /// Gets the file path for a process JSON file.
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <returns>The full file path (e.g., StreamingAssets/Processes/ProcessName/ProcessName.json)</returns>
        public static string GetProcessFilePath(string processName)
        {
            string processDir = GetProcessDirectory(processName);
            return Path.Combine(processDir, processName + ".json");
        }

        /// <summary>
        /// Validates a process name for invalid file system characters.
        /// </summary>
        /// <param name="processName">The process name to validate</param>
        /// <returns>An error message if invalid, or null if valid</returns>
        public static string ValidateProcessName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return "ProcessName is required";
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (processName.IndexOfAny(invalidChars) >= 0)
            {
                return $"ProcessName contains invalid characters: {string.Join(", ", invalidChars.Where(c => processName.Contains(c)))}";
            }

            return null; // Valid
        }

        /// <summary>
        /// Saves process data to disk.
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <param name="data">The serialized process data</param>
        /// <param name="overwrite">Whether to overwrite an existing file</param>
        /// <exception cref="InvalidOperationException">Thrown if file exists and overwrite is false</exception>
        public static void SaveProcess(string processName, byte[] data, bool overwrite)
        {
            string streamingRoot = Application.streamingAssetsPath;
            if (string.IsNullOrEmpty(streamingRoot))
            {
                throw new InvalidOperationException("StreamingAssets path is not available");
            }

            // Ensure StreamingAssets directory exists
            Directory.CreateDirectory(streamingRoot);

            // Create process directory
            string processDir = GetProcessDirectory(processName);
            Directory.CreateDirectory(processDir);

            // Get file path
            string jsonPath = GetProcessFilePath(processName);

            // Check if file exists
            if (File.Exists(jsonPath) && !overwrite)
            {
                throw new InvalidOperationException($"Process file already exists at {NormalizePath(jsonPath)}");
            }

            // Write file
            File.WriteAllBytes(jsonPath, data);
        }

        /// <summary>
        /// Normalizes file paths by converting backslashes to forward slashes for cross-platform compatibility.
        /// </summary>
        /// <param name="path">The file path to normalize</param>
        /// <returns>Path with forward slashes</returns>
        public static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/');
        }
    }
}
