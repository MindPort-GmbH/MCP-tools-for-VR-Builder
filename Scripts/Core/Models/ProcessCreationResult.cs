using System;

namespace VRBuilder.MCP.Core.Models
{
    /// <summary>
    /// Result of a process creation operation.
    /// </summary>
    public class ProcessCreationResult
    {
        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// A message describing the result.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The name of the process.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// The file path where the process was saved.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The number of chapters created.
        /// </summary>
        public int ChapterCount { get; set; }

        /// <summary>
        /// The total number of steps created.
        /// </summary>
        public int StepCount { get; set; }

        /// <summary>
        /// The number of bytes written to the file.
        /// </summary>
        public int BytesWritten { get; set; }

        /// <summary>
        /// The error that occurred, if any.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// The process request that was used to create the process.
        /// </summary>
        public ProcessCreationRequest CreatedProcess { get; set; }

        public static ProcessCreationResult CreateSuccess(string processName, string filePath, int chapterCount, int stepCount, int bytesWritten)
        {
            return new ProcessCreationResult
            {
                Success = true,
                Message = $"Successfully created process '{processName}' with {chapterCount} chapter(s) and {stepCount} step(s)",
                ProcessName = processName,
                FilePath = filePath,
                ChapterCount = chapterCount,
                StepCount = stepCount,
                BytesWritten = bytesWritten
            };
        }

        public static ProcessCreationResult CreateError(string message, Exception error = null)
        {
            return new ProcessCreationResult
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }
}
