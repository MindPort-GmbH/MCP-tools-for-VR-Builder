#if IVANMURZAK_MCP_INSTALLED
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRBuilder.MCP.Core;
using VRBuilder.MCP.IvanMurzak.CustomTools.Editor;

namespace VRBuilder.MCP.Tests.IvanMurzak
{
    /// <summary>
    /// Unity Edit Mode tests for IvanMurzak MCP adapter.
    /// Tests the CreateVRBuilderProcess tool with IvanMurzak MCP framework.
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
        /// Test: Create a process with no chapters specified (null chapters array).
        /// Expected: Should create a default "Chapter 1" with empty steps.
        /// </summary>
        [Test]
        public void CreateProcess_NoChaptersSpecified_CreatesDefaultChapter()
        {
            // Arrange
            string processName = "IvanTest_NoChapters";
            createdProcesses.Add(processName);

            // Act
            var result = CreateVRBuilderProcess.CreateProcess(
                processName: processName,
                chapters: null, // No chapters specified
                overwrite: true
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.process, "Response should have 'process' field");
            Assert.IsNotNull(result.path, "Response should have 'path' field");

            var process = result.process;
            Assert.AreEqual(1, process.Chapters.Count, "Expected 1 chapter");
            Assert.AreEqual(0, process.Chapters.Sum(c => c.Steps.Count), "Expected 0 steps in default empty chapter");

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
            string processName = "IvanTest_EmptySteps";
            createdProcesses.Add(processName);

            var chapters = new[]
            {
                new ChapterArg { Name = "Chapter 1", Steps = null },
                new ChapterArg { Name = "Chapter 2", Steps = new StepArg[] { } }
            };

            // Act
            var result = CreateVRBuilderProcess.CreateProcess(
                processName: processName,
                chapters: chapters,
                overwrite: true
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.process, "Response should have 'process' field");
            Assert.IsNotNull(result.path, "Response should have 'path' field");

            var process = result.process;
            Assert.AreEqual(2, process.Chapters.Count, "Expected 2 chapters");
            Assert.AreEqual(0, process.Chapters.Sum(c => c.Steps.Count), "Expected 0 total steps");
        }

        /// <summary>
        /// Test: Create a process with chapters, steps, names, descriptions, and positions.
        /// Expected: Should successfully create a complex multi-chapter process.
        /// </summary>
        [Test]
        public void CreateProcess_FullStepsWithDescriptionsAndPositions_CreatesSuccessfully()
        {
            // Arrange
            string processName = "IvanTest_FullSteps";
            createdProcesses.Add(processName);

            var chapters = new[]
            {
                new ChapterArg
                {
                    Name = "Introduction",
                    Steps = new[]
                    {
                        new StepArg
                        {
                            Name = "Welcome",
                            Description = "Welcome to the training",
                            Position = new float[] { 300f, 0f }
                        },
                        new StepArg
                        {
                            Name = "Safety Brief",
                            Description = "Review safety procedures",
                            Position = new float[] { 600f, 0f }
                        }
                    }
                },
                new ChapterArg
                {
                    Name = "Practical Exercise",
                    Steps = new[]
                    {
                        new StepArg
                        {
                            Name = "Step 1",
                            Description = "First practical step",
                            Position = new float[] { 300f, 100f }
                        },
                        new StepArg
                        {
                            Name = "Step 2",
                            Description = "Second practical step",
                            Position = new float[] { 600f, 100f }
                        },
                        new StepArg
                        {
                            Name = "Step 3",
                            Description = "Final step",
                            Position = new float[] { 900f, 100f }
                        }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.CreateProcess(
                processName: processName,
                chapters: chapters,
                overwrite: true
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.process, "Response should have 'process' field");
            Assert.IsNotNull(result.path, "Response should have 'path' field");

            var process = result.process;
            Assert.AreEqual(2, process.Chapters.Count, "Expected 2 chapters");
            Assert.AreEqual(5, process.Chapters.Sum(c => c.Steps.Count), "Expected 5 total steps");

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
            string processName = "IvanTest_Simple";
            createdProcesses.Add(processName);

            var chapters = new[]
            {
                new ChapterArg
                {
                    Name = "Main Chapter",
                    Steps = new[]
                    {
                        new StepArg { Name = "Step 1" },
                        new StepArg { Name = "Step 2" }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.CreateProcess(
                processName: processName,
                chapters: chapters,
                overwrite: true
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.process, "Response should have 'process' field");
            Assert.IsNotNull(result.path, "Response should have 'path' field");

            var process = result.process;
            Assert.AreEqual(1, process.Chapters.Count, "Expected 1 chapter");
            Assert.AreEqual(2, process.Chapters.Sum(c => c.Steps.Count), "Expected 2 steps");
        }

        /// <summary>
        /// Test: Verify that missing chapter name returns an error.
        /// </summary>
        [Test]
        public void CreateProcess_MissingChapterName_ReturnsError()
        {
            // Arrange
            string processName = "IvanTest_InvalidChapter";

            var chapters = new[]
            {
                new ChapterArg { Name = null, Steps = new StepArg[] { } } // Missing name
            };

            // Act
            var result = CreateVRBuilderProcess.CreateProcess(
                processName: processName,
                chapters: chapters,
                overwrite: true
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.error, "Response should have 'error' field for failed requests");

            string errorMessage = result.error.ToString();
            Assert.IsTrue(errorMessage.Contains("Chapter") || errorMessage.Contains("Name"), "Error should mention chapter or name");
        }

        /// <summary>
        /// Test: Verify that empty process name returns an error.
        /// </summary>
        [Test]
        public void CreateProcess_EmptyProcessName_ReturnsError()
        {
            // Arrange
            string processName = ""; // Invalid: empty name

            var chapters = new[]
            {
                new ChapterArg { Name = "Valid Chapter", Steps = new StepArg[] { } }
            };

            // Act
            var result = CreateVRBuilderProcess.CreateProcess(
                processName: processName,
                chapters: chapters,
                overwrite: true
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.error, "Response should have 'error' field for failed requests");
        }

        /// <summary>
        /// Test: Verify that steps without positions get auto-positioned.
        /// </summary>
        [Test]
        public void CreateProcess_StepsWithoutPositions_AutoPositionsSteps()
        {
            // Arrange
            string processName = "IvanTest_AutoPosition";
            createdProcesses.Add(processName);

            var chapters = new[]
            {
                new ChapterArg
                {
                    Name = "Test Chapter",
                    Steps = new[]
                    {
                        new StepArg { Name = "Step 1", Position = null },
                        new StepArg { Name = "Step 2", Position = null },
                        new StepArg { Name = "Step 3", Position = null }
                    }
                }
            };

            // Act
            var result = CreateVRBuilderProcess.CreateProcess(
                processName: processName,
                chapters: chapters,
                overwrite: true
            );

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.process, "Response should have 'process' field");
            Assert.IsNotNull(result.path, "Response should have 'path' field");

            var process = result.process;
            Assert.AreEqual(3, process.Chapters.Sum(c => c.Steps.Count), "Expected 3 steps");

            // Verify file was created (which means steps were positioned successfully)
            string filePath = ProcessFileManager.GetProcessFilePath(processName);
            Assert.IsTrue(File.Exists(filePath), $"Process file should exist at: {filePath}");
        }
    }
}
#endif
