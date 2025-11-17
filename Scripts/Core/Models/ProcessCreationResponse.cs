namespace VRBuilder.MCP.Core.Models
{
    /// <summary>
    /// Standardized MCP response for process creation operations.
    /// Returns the created process structure and file path.
    /// </summary>
    public class ProcessCreationResponse
    {
        /// <summary>
        /// The process structure that was created (chapters, steps, etc.).
        /// Vector2 fields are flattened to {x,y} via Serializable DTOs for safe JSON serialization.
        /// </summary>
        public SerializableProcessData Process { get; set; }

        /// <summary>
        /// The file path where the process was saved.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Creates a ProcessCreationResponse from a ProcessCreationResult.
        /// </summary>
        /// <param name="result">The result from VRBuilderProcessService</param>
        /// <returns>A standardized response object</returns>
        public static ProcessCreationResponse FromResult(ProcessCreationResult result)
        {
            return new ProcessCreationResponse
            {
                Process = SerializableProcessData.FromProcessCreationRequest(result.CreatedProcess),
                Path = result.FilePath
            };
        }
    }
}
