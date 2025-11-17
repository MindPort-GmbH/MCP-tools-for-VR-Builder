using System.Collections.Generic;
using UnityEngine;
using VRBuilder.MCP.Core.Models;

namespace VRBuilder.MCP.Tests
{
    /// <summary>
    /// Helper class for creating test data for VR Builder MCP process creation tests.
    /// Reduces code duplication across test classes.
    /// </summary>
    public static class ProcessTestHelper
    {
        /// <summary>
        /// Creates a minimal ProcessCreationRequest with no chapters specified.
        /// This should trigger the creation of a default "Chapter 1" with no steps.
        /// </summary>
        public static ProcessCreationRequest CreateRequestNoChaptersSpecified(string processName = "TestProcess", bool overwrite = true)
        {
            return new ProcessCreationRequest
            {
                ProcessName = processName,
                Overwrite = overwrite,
                Chapters = new List<ChapterData>() // Empty list - no chapters specified
            };
        }

        /// <summary>
        /// Creates a ProcessCreationRequest with chapters but empty steps.
        /// </summary>
        public static ProcessCreationRequest CreateRequestWithChaptersEmptySteps(string processName = "TestProcessChapters", bool overwrite = true)
        {
            return new ProcessCreationRequest
            {
                ProcessName = processName,
                Overwrite = overwrite,
                Chapters = new List<ChapterData>
                {
                    new ChapterData { Name = "Chapter 1", Steps = new List<StepData>() },
                    new ChapterData { Name = "Chapter 2", Steps = new List<StepData>() }
                }
            };
        }

        /// <summary>
        /// Creates a ProcessCreationRequest with chapters, steps, names, descriptions, and positions.
        /// </summary>
        public static ProcessCreationRequest CreateRequestWithFullSteps(string processName = "TestProcessFull", bool overwrite = true)
        {
            return new ProcessCreationRequest
            {
                ProcessName = processName,
                Overwrite = overwrite,
                Chapters = new List<ChapterData>
                {
                    new ChapterData
                    {
                        Name = "Introduction",
                        Steps = new List<StepData>
                        {
                            new StepData
                            {
                                Name = "Welcome",
                                Description = "Welcome to the training",
                                Position = new Vector2(300, 0)
                            },
                            new StepData
                            {
                                Name = "Safety Brief",
                                Description = "Review safety procedures",
                                Position = new Vector2(600, 0)
                            }
                        }
                    },
                    new ChapterData
                    {
                        Name = "Practical Exercise",
                        Steps = new List<StepData>
                        {
                            new StepData
                            {
                                Name = "Step 1",
                                Description = "First practical step",
                                Position = new Vector2(300, 100)
                            },
                            new StepData
                            {
                                Name = "Step 2",
                                Description = "Second practical step",
                                Position = new Vector2(600, 100)
                            },
                            new StepData
                            {
                                Name = "Step 3",
                                Description = "Final step",
                                Position = new Vector2(900, 100)
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates a simple single-chapter request for basic testing.
        /// </summary>
        public static ProcessCreationRequest CreateSimpleSingleChapterRequest(string processName = "SimpleProcess", bool overwrite = true)
        {
            return new ProcessCreationRequest
            {
                ProcessName = processName,
                Overwrite = overwrite,
                Chapters = new List<ChapterData>
                {
                    new ChapterData
                    {
                        Name = "Main Chapter",
                        Steps = new List<StepData>
                        {
                            new StepData { Name = "Step 1" },
                            new StepData { Name = "Step 2" }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Validates common success criteria for a ProcessCreationResult.
        /// </summary>
        public static bool ValidateSuccessResult(ProcessCreationResult result, int expectedChapters, int expectedSteps)
        {
            if (result == null) return false;
            if (!result.Success) return false;
            if (result.ChapterCount != expectedChapters) return false;
            if (result.StepCount != expectedSteps) return false;
            if (string.IsNullOrEmpty(result.ProcessName)) return false;
            if (string.IsNullOrEmpty(result.FilePath)) return false;
            if (result.BytesWritten <= 0) return false;

            return true;
        }

        /// <summary>
        /// Creates a request with invalid data for testing validation.
        /// </summary>
        public static ProcessCreationRequest CreateInvalidRequest()
        {
            return new ProcessCreationRequest
            {
                ProcessName = "", // Invalid: empty name
                Overwrite = false,
                Chapters = new List<ChapterData>()
            };
        }
    }
}
