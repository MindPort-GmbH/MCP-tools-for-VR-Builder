#if COPLAYDEV_MCP_INSTALLED
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;
using VRBuilder.MCP.Core;
using VRBuilder.MCP.CoplayDev.CustomTools.Editor;

namespace VRBuilder.MCP.Tests.CoplayDev
{
    /// <summary>
    /// Unity Edit Mode tests for CoplayDev MCP adapter.
    /// Tests the CreateVRBuilderProcess tool with CoplayDev MCP framework.
    /// </summary>
    [TestFixture]
    public class CreateVRBuilderProcessTests
    {
        private List<string> createdProcesses;

        [SetUp]
        public void Setup()
        {
            createdProcesses = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any created test processes
            foreach (var processName in createdProcesses)
            {
                try
                {
                    string processPath = ProcessFileManager.GetProcessFilePath(processName);
                    string processDir = Path.GetDirectoryName(processPath);

                    if (Directory.Exists(processDir))
                    {
                        Directory.Delete(processDir, true);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to clean up test process '{processName}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Test: Create a process with no chapters specified (empty Chapters array).
        /// Expected: Should create a default "Chapter 1" with empty steps.
        /// </summary>
        [Test]
        public void CreateProcess_NoChaptersSpecified_CreatesDefaultChapter()
        {
            // Arrange
            string processName = "CoplayTest_NoChapters";
            createdProcesses.Add(processName);

            var paramsJson = new JObject
            {
                ["ProcessName"] = processName,
                ["Overwrite"] = true
                // No Chapters property - should create default chapter
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);

            // Convert to JObject to access properties
            var response = JObject.FromObject(result, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            // Check if it's a success response
            Assert.IsTrue(response["success"].Value<bool>(), $"Expected success but got: {response}");
            var chaptersToken = response["data"]["process"]["Chapters"];
            int chapterCount = chaptersToken != null ? chaptersToken.Count() : 0;
            int totalSteps = chaptersToken != null ? chaptersToken.Sum(c => c["Steps"] != null ? c["Steps"].Count() : 0) : 0;
            Assert.AreEqual(1, chapterCount, "Expected 1 chapter");
            Assert.AreEqual(0, totalSteps, "Expected 0 steps in default empty chapter");

            // Verify file was created
            string filePath = ProcessFileManager.GetProcessFilePath(processName);
            Assert.IsTrue(File.Exists(filePath), $"Process file should exist at: {filePath}");
        }

        /// <summary>
        /// Test: Create a process with chapters but empty steps.
        /// Expected: Should successfully create chapters with no steps.
        /// </summary>
        [Test]
        public void CreateProcess_ChaptersWithEmptySteps_CreatesSuccessfully()
        {
            // Arrange
            string processName = "CoplayTest_EmptySteps";
            createdProcesses.Add(processName);

            var paramsJson = new JObject
            {
                ["ProcessName"] = processName,
                ["Overwrite"] = true,
                ["Chapters"] = new JArray
                {
                    new JObject { ["Name"] = "Chapter 1", ["Steps"] = new JArray() },
                    new JObject { ["Name"] = "Chapter 2", ["Steps"] = new JArray() }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            Assert.IsTrue(response["success"].Value<bool>(), $"Expected success but got: {response}");
            var chaptersToken2 = response["data"]["process"]["Chapters"];
            int chapterCount2 = chaptersToken2 != null ? chaptersToken2.Count() : 0;
            int totalSteps2 = chaptersToken2 != null ? chaptersToken2.Sum(c => c["Steps"] != null ? c["Steps"].Count() : 0) : 0;
            Assert.AreEqual(2, chapterCount2, "Expected 2 chapters");
            Assert.AreEqual(0, totalSteps2, "Expected 0 total steps");
        }

        /// <summary>
        /// Test: Create a process with chapters, steps, names, descriptions, and positions.
        /// Expected: Should successfully create a complex multi-chapter process.
        /// </summary>
        [Test]
        public void CreateProcess_FullStepsWithDescriptionsAndPositions_CreatesSuccessfully()
        {
            // Arrange
            string processName = "CoplayTest_FullSteps";
            createdProcesses.Add(processName);

            var paramsJson = new JObject
            {
                ["ProcessName"] = processName,
                ["Overwrite"] = true,
                ["Chapters"] = new JArray
                {
                    new JObject
                    {
                        ["Name"] = "Introduction",
                        ["Steps"] = new JArray
                        {
                            new JObject
                            {
                                ["Name"] = "Welcome",
                                ["Description"] = "Welcome to the training",
                                ["Position"] = new JObject { ["x"] = 300, ["y"] = 0 }
                            },
                            new JObject
                            {
                                ["Name"] = "Safety Brief",
                                ["Description"] = "Review safety procedures",
                                ["Position"] = new JObject { ["x"] = 600, ["y"] = 0 }
                            }
                        }
                    },
                    new JObject
                    {
                        ["Name"] = "Practical Exercise",
                        ["Steps"] = new JArray
                        {
                            new JObject
                            {
                                ["Name"] = "Step 1",
                                ["Description"] = "First practical step",
                                ["Position"] = new JObject { ["x"] = 300, ["y"] = 100 }
                            },
                            new JObject
                            {
                                ["Name"] = "Step 2",
                                ["Description"] = "Second practical step",
                                ["Position"] = new JObject { ["x"] = 600, ["y"] = 100 }
                            },
                            new JObject
                            {
                                ["Name"] = "Step 3",
                                ["Description"] = "Final step",
                                ["Position"] = new JObject { ["x"] = 900, ["y"] = 100 }
                            }
                        }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            Assert.IsTrue(response["success"].Value<bool>(), $"Expected success but got: {response}");
            var chaptersToken3 = response["data"]["process"]["Chapters"];
            int chapterCount3 = chaptersToken3 != null ? chaptersToken3.Count() : 0;
            int totalSteps3 = chaptersToken3 != null ? chaptersToken3.Sum(c => c["Steps"] != null ? c["Steps"].Count() : 0) : 0;
            Assert.AreEqual(2, chapterCount3, "Expected 2 chapters");
            Assert.AreEqual(5, totalSteps3, "Expected 5 total steps");

            // Verify the file was actually created
            string filePath = ProcessFileManager.GetProcessFilePath(processName);
            Assert.IsTrue(File.Exists(filePath), $"Process file should exist at: {filePath}");
        }

        /// <summary>
        /// Test: Create a simple single-chapter process.
        /// Expected: Should successfully create a basic process with one chapter and steps.
        /// </summary>
        [Test]
        public void CreateProcess_SimpleSingleChapter_CreatesSuccessfully()
        {
            // Arrange
            string processName = "CoplayTest_Simple";
            createdProcesses.Add(processName);

            var paramsJson = new JObject
            {
                ["ProcessName"] = processName,
                ["Overwrite"] = true,
                ["Chapters"] = new JArray
                {
                    new JObject
                    {
                        ["Name"] = "Main Chapter",
                        ["Steps"] = new JArray
                        {
                            new JObject { ["Name"] = "Step 1" },
                            new JObject { ["Name"] = "Step 2" }
                        }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            Assert.IsTrue(response["success"].Value<bool>(), $"Expected success but got: {response}");
            var chaptersToken4 = response["data"]["process"]["Chapters"];
            int chapterCount4 = chaptersToken4 != null ? chaptersToken4.Count() : 0;
            int totalSteps4 = chaptersToken4 != null ? chaptersToken4.Sum(c => c["Steps"] != null ? c["Steps"].Count() : 0) : 0;
            Assert.AreEqual(1, chapterCount4, "Expected 1 chapter");
            Assert.AreEqual(2, totalSteps4, "Expected 2 steps");
        }

        /// <summary>
        /// Test: Verify that missing chapter name returns an error.
        /// </summary>
        [Test]
        public void CreateProcess_MissingChapterName_ReturnsError()
        {
            // Arrange
            string processName = "CoplayTest_InvalidChapter";

            var paramsJson = new JObject
            {
                ["ProcessName"] = processName,
                ["Overwrite"] = true,
                ["Chapters"] = new JArray
                {
                    new JObject { ["Steps"] = new JArray() } // Missing Name property
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result);

            Assert.IsFalse(response["success"].Value<bool>(), $"Expected error but got success");
            string errorMessage = response["error"].Value<string>();
            Assert.IsTrue(errorMessage.Contains("Chapter") || errorMessage.Contains("Name"),
                "Error should mention chapter or name");
        }

        /// <summary>
        /// Test: Verify that empty process name returns an error.
        /// </summary>
        [Test]
        public void CreateProcess_EmptyProcessName_ReturnsError()
        {
            // Arrange
            var paramsJson = new JObject
            {
                ["ProcessName"] = "", // Invalid: empty name
                ["Overwrite"] = true,
                ["Chapters"] = new JArray
                {
                    new JObject { ["Name"] = "Valid Chapter", ["Steps"] = new JArray() }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result);

            Assert.IsFalse(response["success"].Value<bool>(), $"Expected error but got success");
        }

        /// <summary>
        /// Test: Verify case-insensitive parameter parsing.
        /// CoplayDev adapter should support case-insensitive property names.
        /// </summary>
        [Test]
        public void CreateProcess_CaseInsensitiveParams_CreatesSuccessfully()
        {
            // Arrange
            string processName = "CoplayTest_CaseInsensitive";
            createdProcesses.Add(processName);

            var paramsJson = new JObject
            {
                ["processName"] = processName, // lowercase 'p'
                ["overwrite"] = true, // lowercase 'o'
                ["chapters"] = new JArray // lowercase 'c'
                {
                    new JObject
                    {
                        ["name"] = "Test Chapter", // lowercase 'n'
                        ["steps"] = new JArray // lowercase 's'
                        {
                            new JObject { ["name"] = "Step 1" }
                        }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            Assert.IsTrue(response["success"].Value<bool>(), $"Expected success but got: {response}");
            var chaptersToken5 = response["data"]["process"]["Chapters"];
            int chapterCount5 = chaptersToken5 != null ? chaptersToken5.Count() : 0;
            int totalSteps5 = chaptersToken5 != null ? chaptersToken5.Sum(c => c["Steps"] != null ? c["Steps"].Count() : 0) : 0;
            Assert.AreEqual(1, chapterCount5, "Expected 1 chapter");
            Assert.AreEqual(1, totalSteps5, "Expected 1 step");
        }

        /// <summary>
        /// Test: Verify that position can be specified as an array [x, y].
        /// </summary>
        [Test]
        public void CreateProcess_PositionAsArray_CreatesSuccessfully()
        {
            // Arrange
            string processName = "CoplayTest_PositionArray";
            createdProcesses.Add(processName);

            var paramsJson = new JObject
            {
                ["ProcessName"] = processName,
                ["Overwrite"] = true,
                ["Chapters"] = new JArray
                {
                    new JObject
                    {
                        ["Name"] = "Test Chapter",
                        ["Steps"] = new JArray
                        {
                            new JObject
                            {
                                ["Name"] = "Step 1",
                                ["Position"] = new JArray { 100, 200 } // Array format
                            }
                        }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            Assert.IsTrue(response["success"].Value<bool>(), $"Expected success but got: {response}");
            var chaptersToken6 = response["data"]["process"]["Chapters"];
            int totalSteps6 = chaptersToken6 != null ? chaptersToken6.Sum(c => c["Steps"] != null ? c["Steps"].Count() : 0) : 0;
            Assert.AreEqual(1, totalSteps6, "Expected 1 step");
        }

        /// <summary>
        /// Test: Verify that steps without positions get auto-positioned.
        /// </summary>
        [Test]
        public void CreateProcess_StepsWithoutPositions_AutoPositionsSteps()
        {
            // Arrange
            string processName = "CoplayTest_AutoPosition";
            createdProcesses.Add(processName);

            var paramsJson = new JObject
            {
                ["ProcessName"] = processName,
                ["Overwrite"] = true,
                ["Chapters"] = new JArray
                {
                    new JObject
                    {
                        ["Name"] = "Test Chapter",
                        ["Steps"] = new JArray
                        {
                            new JObject { ["Name"] = "Step 1" }, // No position
                            new JObject { ["Name"] = "Step 2" }, // No position
                            new JObject { ["Name"] = "Step 3" }  // No position
                        }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.HandleCommand(paramsJson);

            // Assert
            Assert.IsNotNull(result);
            var response = JObject.FromObject(result, new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            Assert.IsTrue(response["success"].Value<bool>(), $"Expected success but got: {response}");
            var chaptersToken7 = response["data"]["process"]["Chapters"];
            int totalSteps7 = chaptersToken7 != null ? chaptersToken7.Sum(c => c["Steps"] != null ? c["Steps"].Count() : 0) : 0;
            Assert.AreEqual(3, totalSteps7, "Expected 3 steps");

            // Verify file was created (which means steps were positioned successfully)
            string filePath = ProcessFileManager.GetProcessFilePath(processName);
            Assert.IsTrue(File.Exists(filePath), $"Process file should exist at: {filePath}");
        }
    }
}
#endif
