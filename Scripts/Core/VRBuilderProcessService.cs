using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Entities.Factories;
using VRBuilder.Core.Serialization;
using VRBuilder.MCP.Core.Models;

namespace VRBuilder.MCP.Core
{
    /// <summary>
    /// Core service for creating and managing VR Builder processes.
    /// </summary>
    public static class VRBuilderProcessService
    {
        /// <summary>
        /// Default horizontal spacing between steps in the VR Builder editor.
        /// </summary>
        private const float DEFAULT_STEP_SPACING = 300f;

        /// <summary>
        /// Creates and saves a VR Builder process from a request.
        /// </summary>
        /// <param name="request">The process creation request</param>
        /// <returns>Result of the operation</returns>
        public static ProcessCreationResult CreateAndSaveProcess(ProcessCreationRequest request)
        {
            try
            {
                // Validate request
                string validationError = request.Validate();
                if (validationError != null)
                {
                    return ProcessCreationResult.CreateError(validationError);
                }

                // Build the process
                IProcess process = BuildProcess(request.ProcessName, request.Chapters);

                // Serialize the process
                byte[] data = SerializeProcess(process);

                // Save to file
                ProcessFileManager.SaveProcess(request.ProcessName, data, request.Overwrite);

                // Get statistics
                int chapterCount = process.Data.Chapters.Count;
                int stepCount = process.Data.Chapters.Sum(c => c.Data.Steps.Count);
                string filePath = ProcessFileManager.GetProcessFilePath(request.ProcessName);

                var result = ProcessCreationResult.CreateSuccess(
                    request.ProcessName,
                    ProcessFileManager.NormalizePath(filePath),
                    chapterCount,
                    stepCount,
                    data.Length
                );

                // Include the created process structure in the result
                result.CreatedProcess = request;

                return result;
            }
            catch (Exception ex)
            {
                return ProcessCreationResult.CreateError($"Failed to create process: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Builds a VR Builder process from chapter data.
        /// </summary>
        /// <param name="processName">Name of the process</param>
        /// <param name="chapters">List of chapter data</param>
        /// <returns>IProcess instance</returns>
        public static IProcess BuildProcess(string processName, List<ChapterData> chapters)
        {
            if (chapters == null || chapters.Count == 0)
            {
                throw new ArgumentException("At least one chapter is required");
            }

            var processChapters = new List<IChapter>();
            foreach (var chapterData in chapters)
            {
                var chapter = BuildChapter(chapterData);
                processChapters.Add(chapter);
            }

            return new Process(processName, processChapters);
        }

        /// <summary>
        /// Builds a VR Builder chapter from chapter data.
        /// </summary>
        /// <param name="chapterData">The chapter data.</param>
        /// <returns>IChapter instance</returns>
        public static IChapter BuildChapter(ChapterData chapterData)
        {
            if (chapterData == null)
            {
                throw new ArgumentNullException(nameof(chapterData));
            }

            if (string.IsNullOrWhiteSpace(chapterData.Name))
            {
                throw new ArgumentException("Chapter name is required");
            }

            var steps = BuildSteps(chapterData.Steps);

            var firstStep = steps.Count > 0 ? steps[0] : null;
            var chapter = new Chapter(chapterData.Name, firstStep);
            chapter.Data.Steps = new List<IStep>(steps);
            chapter.Data.FirstStep = firstStep;

            return chapter;
        }

        /// <summary>
        /// Builds a list of VR Builder steps from step data and wires them in linear sequence.
        /// </summary>
        /// <param name="stepDataList">List of step data</param>
        /// <returns>List of IStep instances wired in linear sequence, or empty list if no steps provided</returns>
        public static List<IStep> BuildSteps(List<StepData> stepDataList)
        {
            var steps = new List<IStep>();

            if (stepDataList == null || stepDataList.Count == 0)
            {
                return steps;
            }

            for (int i = 0; i < stepDataList.Count; i++)
            {
                var stepData = stepDataList[i];
                var step = CreateStep(stepData, i);
                steps.Add(step);
            }

            WireStepsLinear(steps);
            return steps;
        }

        /// <summary>
        /// Creates a single VR Builder step from step data.
        /// </summary>
        /// <param name="stepData">The step data</param>
        /// <param name="index">Zero-based step index for auto-positioning</param>
        /// <returns>IStep instance</returns>
        private static IStep CreateStep(StepData stepData, int index)
        {
            if (stepData == null || string.IsNullOrWhiteSpace(stepData.Name))
            {
                throw new ArgumentException($"Step name is required at index {index}");
            }

            Step step = EntityFactory.CreateStep(stepData.Name) as Step;

            // Apply metadata
            ApplyStepMetadata(step, stepData, index);

            return step;
        }

        /// <summary>
        /// Applies metadata (description and position) to a step.
        /// </summary>
        /// <param name="step">The step to apply metadata to</param>
        /// <param name="stepData">The step data containing metadata</param>
        /// <param name="index">Zero-based step index for auto-positioning</param>
        public static void ApplyStepMetadata(IStep step, StepData stepData, int index = 0)
        {
            if (step == null)
            {
                return;
            }

            // Apply description if provided
            if (!string.IsNullOrWhiteSpace(stepData.Description))
            {
                step.Data.Description = stepData.Description;
            }

            // Apply position (use provided or calculate default)
            if (step is Step stepWithMetadata && stepWithMetadata.StepMetadata != null)
            {
                // If position is provided, use it; otherwise auto-space horizontally
                // The +1 offset prevents overlap with the automatic Start step at (0,0)
                stepWithMetadata.StepMetadata.Position = stepData.Position ?? new Vector2((index + 1) * DEFAULT_STEP_SPACING, 0);
            }
        }

        /// <summary>
        /// Wires steps together in a linear sequence (step[0] -> step[1] -> ... -> step[n] -> null).
        /// </summary>
        /// <param name="steps">List of steps to wire together</param>
        public static void WireStepsLinear(List<IStep> steps)
        {
            if (steps == null || steps.Count == 0)
            {
                return;
            }

            for (int i = 0; i < steps.Count; i++)
            {
                var transitions = steps[i].Data.Transitions.Data.Transitions;
                if (transitions == null)
                {
                    transitions = new List<ITransition>();
                    steps[i].Data.Transitions.Data.Transitions = transitions;
                }
                if (transitions.Count == 0)
                {
                    transitions.Add(EntityFactory.CreateTransition());
                }
                // Set target: next step or null (end of process)
                transitions[0].Data.TargetStep = (i + 1 < steps.Count) ? steps[i + 1] : null;
            }
        }

        /// <summary>
        /// Serializes a VR Builder process to byte array.
        /// </summary>
        /// <param name="process">The process to serialize</param>
        /// <returns>Serialized process data</returns>
        public static byte[] SerializeProcess(IProcess process)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            var serializer = new NewtonsoftJsonProcessSerializerV3();
            return serializer.ProcessToByteArray(process);
        }

        /// <summary>
        /// Creates a default empty chapter named "Chapter 1" with no steps.
        /// </summary>
        /// <returns>Default ChapterData with empty steps list</returns>
        public static ChapterData CreateDefaultChapter()
        {
            return new ChapterData
            {
                Name = "Chapter 1",
                Steps = new List<StepData>()
            };
        }
    }
}
