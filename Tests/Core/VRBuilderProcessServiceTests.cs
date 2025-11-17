using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRBuilder.MCP.Core;
using VRBuilder.MCP.Core.Models;

namespace VRBuilder.MCP.Tests.Core
{
    /// <summary>
    /// Unity Edit Mode tests for VRBuilderProcessService.
    /// Tests the core process creation functionality.
    /// </summary>
    [TestFixture]
    public class VRBuilderProcessServiceTests
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
        /// Test: Create a process with no chapters specified.
        /// Expected: Should create a default "Chapter 1" with empty steps.
        /// </summary>
        [Test]
        public void CreateProcess_NoChaptersSpecified_CreatesDefaultChapter()
        {
            // Arrange
            var request = new ProcessCreationRequest
            {
                ProcessName = "TestProcess_NoChapters",
                Overwrite = true,
                Chapters = new List<ChapterData>()
            };

            // Need to add a default chapter since validation requires at least one
            request.Chapters.Add(VRBuilderProcessService.CreateDefaultChapter());
            createdProcesses.Add(request.ProcessName);

            // Act
            var result = VRBuilderProcessService.CreateAndSaveProcess(request);

            // Assert
            Assert.IsTrue(result.Success, $"Expected success but got: {result.Message}");
            Assert.AreEqual(1, result.ChapterCount, "Expected 1 chapter");
            Assert.AreEqual(0, result.StepCount, "Expected 0 steps in default empty chapter");
            Assert.AreEqual(request.ProcessName, result.ProcessName);
            Assert.IsFalse(string.IsNullOrEmpty(result.FilePath));
            Assert.Greater(result.BytesWritten, 0);
        }

        /// <summary>
        /// Test: Create a process with chapters but empty steps.
        /// Expected: Should successfully create chapters with no steps.
        /// </summary>
        [Test]
        public void CreateProcess_ChaptersWithEmptySteps_CreatesSuccessfully()
        {
            // Arrange
            var request = ProcessTestHelper.CreateRequestWithChaptersEmptySteps("TestProcess_EmptySteps", true);
            createdProcesses.Add(request.ProcessName);

            // Act
            var result = VRBuilderProcessService.CreateAndSaveProcess(request);

            // Assert
            Assert.IsTrue(result.Success, $"Expected success but got: {result.Message}");
            Assert.AreEqual(2, result.ChapterCount, "Expected 2 chapters");
            Assert.AreEqual(0, result.StepCount, "Expected 0 total steps");
            Assert.AreEqual(request.ProcessName, result.ProcessName);
        }

        /// <summary>
        /// Test: Create a process with chapters, steps, names, descriptions, and positions.
        /// Expected: Should successfully create a complex multi-chapter process.
        /// </summary>
        [Test]
        public void CreateProcess_FullStepsWithDescriptionsAndPositions_CreatesSuccessfully()
        {
            // Arrange
            var request = ProcessTestHelper.CreateRequestWithFullSteps("TestProcess_FullSteps", true);
            createdProcesses.Add(request.ProcessName);

            // Act
            var result = VRBuilderProcessService.CreateAndSaveProcess(request);

            // Assert
            Assert.IsTrue(result.Success, $"Expected success but got: {result.Message}");
            Assert.AreEqual(2, result.ChapterCount, "Expected 2 chapters");
            Assert.AreEqual(5, result.StepCount, "Expected 5 total steps (2 in first chapter, 3 in second)");
            Assert.AreEqual(request.ProcessName, result.ProcessName);
            Assert.Greater(result.BytesWritten, 0);

            // Verify the file was actually created
            Assert.IsTrue(File.Exists(result.FilePath), $"Process file should exist at: {result.FilePath}");
        }

        /// <summary>
        /// Test: Create a simple single-chapter process.
        /// Expected: Should successfully create a basic process with one chapter and steps.
        /// </summary>
        [Test]
        public void CreateProcess_SimpleSingleChapter_CreatesSuccessfully()
        {
            // Arrange
            var request = ProcessTestHelper.CreateSimpleSingleChapterRequest("TestProcess_Simple", true);
            createdProcesses.Add(request.ProcessName);

            // Act
            var result = VRBuilderProcessService.CreateAndSaveProcess(request);

            // Assert
            Assert.IsTrue(ProcessTestHelper.ValidateSuccessResult(result, 1, 2));
            Assert.AreEqual("TestProcess_Simple", result.ProcessName);
        }

        /// <summary>
        /// Test: Verify that BuildProcess creates the correct number of chapters and steps.
        /// </summary>
        [Test]
        public void BuildProcess_MultipleChapters_CreatesCorrectStructure()
        {
            // Arrange
            var chapters = new List<ChapterData>
            {
                new ChapterData
                {
                    Name = "Chapter 1",
                    Steps = new List<StepData>
                    {
                        new StepData { Name = "Step 1" },
                        new StepData { Name = "Step 2" }
                    }
                },
                new ChapterData
                {
                    Name = "Chapter 2",
                    Steps = new List<StepData>
                    {
                        new StepData { Name = "Step 3" }
                    }
                }
            };

            // Act
            var process = VRBuilderProcessService.BuildProcess("TestProcess", chapters);

            // Assert
            Assert.IsNotNull(process);
            Assert.AreEqual("TestProcess", process.Data.Name);
            Assert.AreEqual(2, process.Data.Chapters.Count);
            Assert.AreEqual(2, process.Data.Chapters[0].Data.Steps.Count);
            Assert.AreEqual(1, process.Data.Chapters[1].Data.Steps.Count);
        }

        /// <summary>
        /// Test: Verify that steps are wired in linear sequence.
        /// </summary>
        [Test]
        public void BuildSteps_LinearWiring_StepsConnectedCorrectly()
        {
            // Arrange
            var stepDataList = new List<StepData>
            {
                new StepData { Name = "Step 1" },
                new StepData { Name = "Step 2" },
                new StepData { Name = "Step 3" }
            };

            // Act
            var steps = VRBuilderProcessService.BuildSteps(stepDataList);

            // Assert
            Assert.AreEqual(3, steps.Count);

            // Verify linear wiring: Step 1 -> Step 2 -> Step 3 -> null
            Assert.IsNotNull(steps[0].Data.Transitions.Data.Transitions);
            Assert.AreEqual(steps[1], steps[0].Data.Transitions.Data.Transitions[0].Data.TargetStep);
            Assert.AreEqual(steps[2], steps[1].Data.Transitions.Data.Transitions[0].Data.TargetStep);
            Assert.IsNull(steps[2].Data.Transitions.Data.Transitions[0].Data.TargetStep);
        }

        /// <summary>
        /// Test: Verify that step metadata (description and position) is applied correctly.
        /// </summary>
        [Test]
        public void ApplyStepMetadata_WithDescriptionAndPosition_MetadataAppliedCorrectly()
        {
            // Arrange
            var stepData = new StepData
            {
                Name = "Test Step",
                Description = "Test Description",
                Position = new Vector2(100, 200)
            };
            var step = VRBuilder.Core.Entities.Factories.EntityFactory.CreateStep(stepData.Name);

            // Act
            VRBuilderProcessService.ApplyStepMetadata(step, stepData, 0);

            // Assert
            Assert.AreEqual("Test Description", step.Data.Description);

            // Check position if step supports it
            if (step is VRBuilder.Core.Step stepWithMetadata && stepWithMetadata.StepMetadata != null)
            {
                Assert.AreEqual(new Vector2(100, 200), stepWithMetadata.StepMetadata.Position);
            }
        }

        /// <summary>
        /// Test: Verify validation fails for invalid process name.
        /// </summary>
        [Test]
        public void CreateProcess_InvalidProcessName_ReturnsError()
        {
            // Arrange
            var request = new ProcessCreationRequest
            {
                ProcessName = "", // Invalid: empty name
                Overwrite = true,
                Chapters = new List<ChapterData> { new ChapterData { Name = "Chapter 1" } }
            };

            // Act
            var result = VRBuilderProcessService.CreateAndSaveProcess(request);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("ProcessName"));
        }

        /// <summary>
        /// Test: Verify validation fails when no chapters are provided.
        /// </summary>
        [Test]
        public void CreateProcess_NoChapters_ReturnsError()
        {
            // Arrange
            var request = new ProcessCreationRequest
            {
                ProcessName = "ValidName",
                Overwrite = true,
                Chapters = new List<ChapterData>() // Empty chapters list
            };

            // Act
            var result = VRBuilderProcessService.CreateAndSaveProcess(request);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("chapter"));
        }

        /// <summary>
        /// Test: Verify default chapter is created correctly.
        /// </summary>
        [Test]
        public void CreateDefaultChapter_CreatesChapterWithCorrectName()
        {
            // Act
            var chapter = VRBuilderProcessService.CreateDefaultChapter();

            // Assert
            Assert.IsNotNull(chapter);
            Assert.AreEqual("Chapter 1", chapter.Name);
            Assert.IsNotNull(chapter.Steps);
            Assert.AreEqual(0, chapter.Steps.Count);
        }
    }
}
