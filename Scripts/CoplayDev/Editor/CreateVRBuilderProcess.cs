#if COPLAYDEV_MCP_INSTALLED
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using MCPForUnity.Editor.Tools;
using MCPForUnity.Editor.Helpers;
using VRBuilder.MCP.Core;
using VRBuilder.MCP.Core.Models;

namespace VRBuilder.MCP.CoplayDev.CustomTools.Editor
{
    /// <summary>
    /// CoplayDev MCP adapter for creating VR Builder processes.
    /// This is a thin adapter that converts JSON parameters to ProcessCreationRequest
    /// and delegates to VRBuilderProcessService for core business logic.
    /// </summary>
    [McpForUnityTool("create_vr_builder_process")]
    public static class CreateVRBuilderProcess
    {
        /// <summary>
        /// Creates a VR Builder process and saves it to StreamingAssets/Processes/{processName}/{processName}.json
        /// Each chapter includes an automatic Start step at [0, 0]. First steps should be positioned at [300, 0] to avoid overlap.
        /// If no chapters provided, creates a default empty chapter. Empty chapters are allowed.
        /// Steps auto-generate names as Step 1, Step 2, etc. if not provided.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            try
            {
                // Parse parameters into request model
                ProcessCreationRequest request = ParseRequest(@params);

                // Call core service
                ProcessCreationResult result = VRBuilderProcessService.CreateAndSaveProcess(request);

                // Convert result to response
                if (result.Success)
                {
                    var responseData = ProcessCreationResponse.FromResult(result);
                    return new SuccessResponse(
                        result.Message,
                        new
                        {
                            process = responseData.Process,
                            path = responseData.Path
                        }
                    );
                }
                else
                {
                    return new ErrorResponse(result.Message);
                }
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Error creating VR Builder process: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses JObject parameters into a ProcessCreationRequest.
        /// </summary>
        /// <remarks>
        /// Supports two modes:
        /// 1) chapters provided
        /// 2) default empty chapter
        /// </remarks>
        private static ProcessCreationRequest ParseRequest(JObject @params)
        {
            var request = new ProcessCreationRequest();

            // Parse process name (case-insensitive)
            request.ProcessName = GetString(@params, "ProcessName", "process_name", "processName");

            // Parse overwrite flag
            request.Overwrite = GetBool(@params, "Overwrite", "overwrite") ?? false;

            // Parse chapters
            JArray chaptersToken = GetArray(@params, "Chapters", "chapters");

            if (chaptersToken != null && chaptersToken.Count > 0)
            {
                // Use provided chapters
                request.Chapters = ParseChapters(chaptersToken);
            }
            else
            {
                // Default: single empty chapter named "Chapter 1"
                request.Chapters = new List<ChapterData> { VRBuilderProcessService.CreateDefaultChapter() };
            }

            return request;
        }

        /// <summary>
        /// Parses a JArray of chapters into ChapterData list.
        /// Chapter names are required and must be explicit.
        /// </summary>
        private static List<ChapterData> ParseChapters(JArray chaptersToken)
        {
            var chapters = new List<ChapterData>();

            for (int i = 0; i < chaptersToken.Count; i++)
            {
                var chapterToken = chaptersToken[i];
                var chapter = new ChapterData();

                // Parse chapter name
                if (chapterToken is JObject chapterObj)
                {
                    string chapterName = GetString(chapterObj, "Name", "name");
                    if (string.IsNullOrWhiteSpace(chapterName))
                    {
                        throw new ArgumentException($"Chapter {i} is missing a required 'Name' property");
                    }
                    chapter.Name = chapterName;

                    // Parse steps (empty list allowed)
                    JArray stepsToken = GetArray(chapterObj, "Steps", "steps");
                    chapter.Steps = (stepsToken != null && stepsToken.Count > 0)
                        ? ParseSteps(stepsToken)
                        : new List<StepData>();
                }
                else
                {
                    // Chapter is just a string name
                    string chapterName = chapterToken?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(chapterName))
                    {
                        throw new ArgumentException($"Chapter {i} is missing a name");
                    }
                    chapter.Name = chapterName;
                    chapter.Steps = new List<StepData>();
                }

                chapters.Add(chapter);
            }

            return chapters;
        }

        /// <summary>
        /// Parses a JArray of steps into StepData list.
        /// </summary>
        private static List<StepData> ParseSteps(JArray stepsToken)
        {
            var steps = new List<StepData>();

            for (int i = 0; i < stepsToken.Count; i++)
            {
                var stepToken = stepsToken[i];
                var step = new StepData();

                if (stepToken is JObject stepObj)
                {
                    // Step is an object with Name, Description, Position
                    step.Name = GetString(stepObj, "Name", "name");
                    step.Description = GetString(stepObj, "Description", "description");

                    // Parse position
                    if (stepObj.TryGetValue("Position", out JToken posTok) || stepObj.TryGetValue("position", out posTok))
                    {
                        step.Position = ParsePosition(posTok);
                    }
                }
                else
                {
                    // Step is just a string name
                    step.Name = stepToken?.ToString()?.Trim();
                }

                // Ensure step has a name
                if (string.IsNullOrWhiteSpace(step.Name))
                {
                    step.Name = $"Step {i + 1}";
                }

                steps.Add(step);
            }

            return steps;
        }

        /// <summary>
        /// Parses a JSON token into a Vector2 position.
        /// Supports: {"x": 100, "y": 200} or [100, 200]
        /// </summary>
        private static Vector2? ParsePosition(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            try
            {
                // Try parsing as object with x/y properties
                if (token is JObject obj)
                {
                    float x = 0f, y = 0f;
                    bool hasX = false, hasY = false;

                    if (obj.TryGetValue("x", StringComparison.OrdinalIgnoreCase, out JToken xToken))
                    {
                        x = xToken.ToObject<float>();
                        hasX = true;
                    }
                    if (obj.TryGetValue("y", StringComparison.OrdinalIgnoreCase, out JToken yToken))
                    {
                        y = yToken.ToObject<float>();
                        hasY = true;
                    }

                    if (hasX || hasY)
                    {
                        return new Vector2(x, y);
                    }
                }
                // Try parsing as array [x, y]
                else if (token is JArray arr && arr.Count >= 2)
                {
                    float x = arr[0].ToObject<float>();
                    float y = arr[1].ToObject<float>();
                    return new Vector2(x, y);
                }
            }
            catch
            {
                // Return null if parsing fails
            }

            return null;
        }

        #region Helper Methods for JObject Parsing

        /// <summary>
        /// Retrieves a string value from a JObject using multiple possible key names (case-insensitive).
        /// </summary>
        private static string GetString(JObject p, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (p.TryGetValue(k, StringComparison.OrdinalIgnoreCase, out JToken v))
                {
                    var s = v?.ToString();
                    if (!string.IsNullOrWhiteSpace(s))
                        return s;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves a JArray from a JObject using multiple possible key names (case-insensitive).
        /// </summary>
        private static JArray GetArray(JObject p, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (p.TryGetValue(k, StringComparison.OrdinalIgnoreCase, out JToken v) && v is JArray arr)
                {
                    return arr;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves a boolean value from a JObject using multiple possible key names (case-insensitive).
        /// </summary>
        private static bool? GetBool(JObject p, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (p.TryGetValue(k, StringComparison.OrdinalIgnoreCase, out JToken v))
                {
                    try { return v.ToObject<bool?>(); } catch { }
                }
            }
            return null;
        }

        #endregion
    }
}
#endif
