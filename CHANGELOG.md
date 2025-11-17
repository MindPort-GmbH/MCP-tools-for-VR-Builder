# Changelog

All notable changes to the VR Builder MCP Tools package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-11-15

### Added
- Initial release of VR Builder MCP Tools package
- Core business logic for creating VR Builder processes programmatically
- `VRBuilderProcessService` for process creation and configuration
- `ProcessFileManager` for file operations and validation
- Data models: `ProcessCreationRequest`, `ProcessCreationResult`, `ChapterData`, `StepData`
- CoplayDev MCP framework adapter
- IvanMurzak MCP framework adapter
- Support for single-chapter and multi-chapter processes
- Automatic step positioning with 300px horizontal spacing
- Custom step positioning support via Position property
- Overwrite protection for existing process files
- Assembly definitions for clean dependency management
- Comprehensive README documentation
- Package metadata (package.json)

### Features
- Create VR Builder training processes through MCP tools
- Support for steps with names, descriptions, and optional positions
- Linear step wiring within chapters
- Serialization to VR Builder's JSON format
- StreamingAssets integration for process storage
- Case-insensitive parameter parsing (CoplayDev adapter)
- Thread-safe Unity API calls (IvanMurzak adapter)

### Dependencies
- Unity 6000+
- VR Builder Core 5.5.0+
- Compatible MCP framework (CoplayDev or IvanMurzak)

## [Unreleased]

### Planned
- Support for branching step logic
- Additional step types and behaviors
- Process templates
- Validation and preview tools
- Unit tests
