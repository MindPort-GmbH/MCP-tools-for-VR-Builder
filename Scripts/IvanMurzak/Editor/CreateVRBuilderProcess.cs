#if IVANMURZAK_MCP_INSTALLED
using System;
using System.Collections.Generic;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using VRBuilder.MCP.Core;
using VRBuilder.MCP.Core.Models;

namespace VRBuilder.MCP.IvanMurzak.CustomTools.Editor
{
    /// <summary>
    /// IvanMurzak MCP adapter for creating VR Builder processes.
    /// This is a thin adapter that converts method parameters to ProcessCreationRequest
    /// and delegates to VRBuilderProcessService for core business logic.
    /// </summary>
    [McpPluginToolType]
    public static class CreateVRBuilderProcess
    {
        /// <summary>
        /// Creates a VR Builder process and saves it to StreamingAssets/Processes/{processName}/{processName}.json
        /// If no chapters provided, creates a default empty chapter.
        /// JSON field names must use camelCase.
        /// </summary>
        [McpPluginTool("create_vr_builder_process", Title = "Create VR Builder Process JSON")]
        [Description(@"Create a VR Builder process and save it to StreamingAssets/<processName>/<processName>.json. Chapter names are required. All JSON fields names must be camelCase.")]
        public static VRBuilderProcessResponse CreateProcess
        (
            [Description("Name of the process.")]
            string processName,

            [Description("Chapters data. Each chapter includes an automatic Start step at [0, 0]. If null/empty, creates default Chapter 1.")]
            ChapterArg[]? chapters = null,

            [Description("Overwrite the target file if it already exists.")]
            bool overwrite = false
        )
        {
            try
            {
                // Build request
                ProcessCreationRequest request = BuildRequest(processName, chapters, overwrite);

                // Run Unity/VR Builder API usage on main thread
                return MainThread.Instance.Run<VRBuilderProcessResponse>(() =>
                {
                    // Call core service
                    ProcessCreationResult result = VRBuilderProcessService.CreateAndSaveProcess(request);

                    // Convert result to structured response
                    if (result.Success)
                    {
                        var responseData = ProcessCreationResponse.FromResult(result);
                        return VRBuilderProcessResponse.Success(responseData.Process, responseData.Path);
                    }
                    else
                    {
                        return VRBuilderProcessResponse.Error(result.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                return VRBuilderProcessResponse.Error($"Failed to create VR Builder process: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a ProcessCreationRequest from method parameters.
        /// </summary>
        /// <remarks>
        /// Supports two modes:
        /// 1) chapters provided
        /// 2) default empty chapter
        /// </remarks>
        private static ProcessCreationRequest BuildRequest(string processName, ChapterArg[]? chaptersArg, bool overwrite)
        {
            var request = new ProcessCreationRequest
            {
                ProcessName = processName,
                Overwrite = overwrite,
                Chapters = new List<ChapterData>()
            };

            if (chaptersArg != null && chaptersArg.Length > 0)
            {
                // Process chapters - names must be explicit
                for (int i = 0; i < chaptersArg.Length; i++)
                {
                    var chArg = chaptersArg[i] ?? new ChapterArg();

                    if (string.IsNullOrWhiteSpace(chArg.name))
                    {
                        throw new ArgumentException($"Chapter {i} is missing a required 'Name' property");
                    }

                    var chapter = new ChapterData
                    {
                        Name = chArg.name.Trim(),
                        Steps = ConvertSteps(chArg.steps)
                    };

                    request.Chapters.Add(chapter);
                }
            }
            else
            {
                // Default: single empty chapter named "Chapter 1"
                request.Chapters.Add(VRBuilderProcessService.CreateDefaultChapter());
            }

            return request;
        }

        /// <summary>
        /// Converts StepArg array to StepData list.
        /// </summary>
        /// <returns>List of StepData and an empty list if stepArgs is null or empty</returns>
        private static List<StepData> ConvertSteps(StepArg[]? stepArgs)
        {
            if (stepArgs == null || stepArgs.Length == 0)
            {
                return new List<StepData>();
            }

            var steps = new List<StepData>();
            for (int i = 0; i < stepArgs.Length; i++)
            {
                var stepArg = stepArgs[i] ?? new StepArg();
                string defaultName = $"Step {i + 1}";
                string stepName = string.IsNullOrWhiteSpace(stepArg.name) ? defaultName : stepArg.name.Trim();

                var stepData = new StepData
                {
                    Name = stepName,
                    Description = stepArg.description,
                    Position = ParsePosition(stepArg.position)
                };

                steps.Add(stepData);
            }

            return steps;
        }

        /// <summary>
        /// Parses a float array [x, y] into a Vector2.
        /// </summary>
        private static Vector2? ParsePosition(float[]? position)
        {
            if (position != null && position.Length >= 2)
            {
                return new Vector2(position[0], position[1]);
            }
            return null;
        }
    }

    #region Parameter DTOs

    /// <summary>
    /// Chapter parameter for MCP tool.
    /// </summary>
    public class ChapterArg
    {
        [Description("Chapter name.")]
        public string name { get; set; }

        [Description("Steps inside the chapter. Empty chapters are allowed.")]
        public StepArg[]? steps { get; set; }
    }

    /// <summary>
    /// Step parameter for MCP tool.
    /// </summary>
    public class StepArg
    {
        [Description("Step name. Auto-generates as Step 1, Step 2, etc. if not provided")]
        public string? name { get; set; }

        [Description("Step description.")]
        public string? description { get; set; }

        [Description("Position for visual layout as [x, y] array. Suggested default: [300, 0]")]
        public float[]? position { get; set; }
    }

    #endregion
}
#endif
