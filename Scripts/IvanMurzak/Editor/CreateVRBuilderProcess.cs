#if IVANMURZAK_MCP_INSTALLED
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Utils;
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
        [McpPluginTool("vrb-create-process", Title = "Create VR Builder Process JSON")]
        [Description(@"Create a VR Builder process and save it to StreamingAssets/<processName>/<processName>.json. Chapter names are required. JSON field names must use camelCase.
                    FORMAT EXAMPLE:
                    {
                    ""processName"": ""MyTrainingProcess"",
                    ""chapters"": [
                        {
                        ""name"": ""Introduction"",
                        ""steps"": [
                            {
                            ""name"": ""Welcome"",
                            ""description"": ""Introduction to the training module"",
                            ""position"": { ""x"": 300, ""y"": 0}
                            }
                        ]
                        }
                    ],
                    ""overwrite"": true
                    }
                    ")]
        public static VRBuilderProcessResponse CreateProcess
        (
            [Description("Name of the process (and output folder/file).")]
            string processName,

            [Description("Optional: Chapters data. Each chapter MUST have an explicit Name. Steps are optional (empty chapters allowed). Steps may include Description and Position [x,y].")]
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

                    if (string.IsNullOrWhiteSpace(chArg.Name))
                    {
                        throw new ArgumentException($"Chapter {i} is missing a required 'Name' property");
                    }

                    var chapter = new ChapterData
                    {
                        Name = chArg.Name.Trim(),
                        Steps = ConvertSteps(chArg.Steps)
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
                string stepName = string.IsNullOrWhiteSpace(stepArg.Name) ? defaultName : stepArg.Name.Trim();

                var stepData = new StepData
                {
                    Name = stepName,
                    Description = stepArg.Description,
                    Position = ParsePosition(stepArg.Position)
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
        [Description("REQUIRED: Explicit chapter name.")]
        public string? Name { get; set; }

        [Description("Optional: Steps inside the chapter. Empty chapters are allowed.")]
        public StepArg[]? Steps { get; set; }
    }

    /// <summary>
    /// Step parameter for MCP tool.
    /// </summary>
    public class StepArg
    {
        [Description("Step name.")]
        public string? Name { get; set; }

        [Description("Optional step description.")]
        public string? Description { get; set; }

        [Description("Optional position for visual layout in VR Builder editor as [x, y] array.")]
        public float[]? Position { get; set; }
    }

    #endregion
}
#endif
