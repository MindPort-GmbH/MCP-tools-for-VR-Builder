using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VRBuilder.MCP.Core.Models
{
    /// <summary>
    /// Request model for creating a VR Builder process.
    /// </summary>
    public class ProcessCreationRequest
    {
        /// <summary>
        /// The name of the process to create.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// The chapters in the process. If null or empty, Steps will be used for a single chapter.
        /// </summary>
        public List<ChapterData> Chapters { get; set; }

        /// <summary>
        /// Whether to overwrite an existing process file.
        /// </summary>
        public bool Overwrite { get; set; }

        public ProcessCreationRequest()
        {
            Chapters = new List<ChapterData>();
        }

        /// <summary>
        /// Validates the request and returns an error message if invalid, or null if valid.
        /// </summary>
        public string Validate()
        {
            if (string.IsNullOrWhiteSpace(ProcessName))
            {
                return "ProcessName is required";
            }

            // Check for invalid file name characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (ProcessName.IndexOfAny(invalidChars) >= 0)
            {
                return $"ProcessName contains invalid characters: {string.Join(", ", invalidChars.Where(c => ProcessName.Contains(c)))}";
            }

            if (Chapters == null || Chapters.Count == 0)
            {
                return "At least one chapter is required";
            }

            // Validate chapters
            for (int i = 0; i < Chapters.Count; i++)
            {
                var chapter = Chapters[i];
                if (string.IsNullOrWhiteSpace(chapter.Name))
                {
                    return $"Chapter {i} is missing a name";
                }

                // Validate steps if provided empty steps list is allowed
                if (chapter.Steps != null && chapter.Steps.Count > 0)
                {
                    for (int j = 0; j < chapter.Steps.Count; j++)
                    {
                        var step = chapter.Steps[j];
                        if (string.IsNullOrWhiteSpace(step.Name))
                        {
                            return $"Step {j} in chapter '{chapter.Name}' is missing a name";
                        }
                    }
                }
            }

            return null; // Valid
        }
    }
}
