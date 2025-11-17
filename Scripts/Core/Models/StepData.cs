using UnityEngine;

namespace VRBuilder.MCP.Core.Models
{
    /// <summary>
    /// Represents a step in a VR Builder training process.
    /// </summary>
    public class StepData
    {
        /// <summary>
        /// The name of the step.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description of the step.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional position for visual layout in the VR Builder editor.
        /// </summary>
        public Vector2? Position { get; set; }

        public StepData()
        {
        }

        public StepData(string name, string description = null, Vector2? position = null)
        {
            Name = name;
            Description = description;
            Position = position;
        }
    }
}
