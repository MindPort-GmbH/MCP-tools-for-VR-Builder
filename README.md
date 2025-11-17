# VR Builder MCP Tools

Custom MCP (Model Context Protocol) tools for VR Builder, enabling AI-assisted creation of VR training processes.

## Overview

This package provides MCP tools that allow AI assistants to create and manage VR Builder training processes programmatically. It includes adapters for multiple MCP frameworks.

## Features

- **Process Creation**: Create linear VR Builder training processes with multiple chapters and steps
  - **Flexible Step Configuration**: Support for custom step positions, descriptions, and names
  - **Automatic Step Positioning**: Intelligent auto-spacing of steps in the process graph

## Installation

...

## Dependencies

### Required
- **Unity 6000.0.0** (Did not test with earlier versions.)
- **VR Builder Core 5.5.0+** (`co.mindport.vrbuilder.core`)

### At least one Unity MCP
- **CoplayDev MCP for Unity** (`com.coplaydev.unity-mcp`)
  - GitHub: https://github.com/CoplayDev/unity-mcp

- **IvanMurzak Unity MCP** (`com.ivanmurzak.unity.mcp`)
  - GitHub: https://github.com/IvanMurzak/Unity-MCP

**Note**: Both frameworks can be installed simultaneously, and the package will automatically enable the corresponding adapters based on what's available.

## Usage

### Creating a VR Builder Process

The package provides a tool called `create_vr_builder_process` that can be invoked through your MCP-enabled AI assistant.

#### Example: Minimal Process (Default Empty Chapter)

The simplest way to create a process - no chapters specified, automatically creates a default empty chapter named "Chapter 1":

```json
{
  "processName": "MyNewProcess"
}
```

#### Example: Multi-Chapter Process with Step Positioning

Create a process with multiple chapters, custom step descriptions, and specific positions for visual layout in the VR Builder editor:

```json
{
  "processName": "Advanced Training",
  "chapters": [
    {
      "name": "Introduction",
      "steps": [
        { "name": "Welcome", "description": "Introduction to the training" },
        { "name": "Safety Brief", "description": "Review safety procedures" }
      ]
    },
    {
      "name": "Practical Exercise",
      "steps": [
        { "name": "Step 1", "description": "First practical step", "position": { "x": 300, "y": 0 } },
        { "name": "Step 2", "description": "Second practical step", "position": { "x": 600, "y": 0 } }
      ]
    }
  ]
}
```

> **JSON casing:** `com.ivanmurzak.unity.mcp` only works with camelCase field names (`processName`, `chapters`, `steps`, `name`, `description`, `position`, `overwrite`).
> The CoplayDev adapter is PascalCase compatible, but all samples use camelCase for consistency.

### Response Format

Both adapters return a consistent JSON response structure containing the created process and the project-relative path to the created VR Builder process JSON, or an error response.

## Architecture

### Core Components

- **VRBuilderProcessService**: Main service for creating and configuring VR Builder processes
- **ProcessFileManager**: Handles file I/O and validation
- **Models**: Data transfer objects for process creation requests and results

### MCP Adapters

The package uses **conditional compilation** to support multiple MCP frameworks. Each adapter is only compiled when its corresponding framework is installed.

#### CoplayDev Adapter
Located in `Scripts/CoplayDev/Editor/`
- **Compilation Define**: `COPLAYDEV_MCP_INSTALLED`
- Automatically enabled when `com.coplaydev.unity-mcp` package is detected

#### IvanMurzak Adapter
Located in `Scripts/IvanMurzak/Editor/`
- **Compilation Define**: `IVANMURZAK_MCP_INSTALLED`
- Automatically enabled when `com.ivanmurzak.unity.mcp` package is detected

## Troubleshooting

1. Ensure VR Builder Core is installed and properly configured
2. Verify at least one MCP framework is installed (CoplayDev or IvanMurzak)
3. Check Unity Console for compilation errors
4. Ask an LLM
5. Check Support

## Support

Use GitHub or Mind Port Discord: https://discord.gg/jmje7qpp

## License

...
