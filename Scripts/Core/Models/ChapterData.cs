using System.Collections.Generic;

namespace VRBuilder.MCP.Core.Models
{
    /// <summary>
    /// Represents a chapter in a VR Builder training process.
    /// </summary>
    public class ChapterData
    {
        /// <summary>
        /// The name of the chapter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The steps within this chapter.
        /// </summary>
        public List<StepData> Steps { get; set; }

        public ChapterData()
        {
            Steps = new List<StepData>();
        }

        public ChapterData(string name, List<StepData> steps = null)
        {
            Name = name;
            Steps = steps ?? new List<StepData>();
        }
    }
}
