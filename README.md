# VR Builder MCP Tools

Custom MCP tools for VR Builder, enabling AI-assisted creation and modification of VR training processes.

## Overview

This package provides MCP tools that allow AI assistants to create and manage VR Builder training processes programmatically. It includes adapters for multiple MCP frameworks.

## Features

- **Process Creation**: Create linear VR Builder training processes with multiple chapters and steps
  - **Flexible Step Configuration**: Support for custom step positions, descriptions, and names
  - **Automatic Step Positioning**: Intelligent auto-spacing of steps in the process graph

## Quickstart Guide

1. Check Dependencies below.
2. Install VR Builder MCP Tools via the Unity package manger or by putting it into any subfolder in /Assets in your Project.
3. From your MCP-enabled assistant that is connected to Unity via a supported Unity MCP, use a prompt like:
   > Create a VR Builder training process in my current Unity project named that describes how to open a bottle with an bottel opener.
4. In Unity, open or create a VR Builder scene. On the PROCESS_CONFIGURATION GameObject, switch the scene to use the created VR Builder process and open the Process Editor.

## Dependencies

### Required
- **Unity 6000.0.x** or higher. (Did not test with earlier versions.)
- **VR Builder Core 5.5.0** or higher (`co.mindport.vrbuilder.core`). You can get it from folowing sources:
  - VR Builder Pro: https://assetstore.unity.com/packages/tools/game-toolkits/vr-builder-pro-toolkit-for-vr-creation-301706 
  - VR Builder Core GitHub: https://github.com/MindPort-GmbH/VR-Builder
  - VR Builder Core OpenUPM: https://openupm.com/packages/co.mindport.vrbuilder.core/

### At least one Unity MCP
- **CoplayDev MCP for Unity** (`com.coplaydev.unity-mcp`)
  - GitHub: https://github.com/CoplayDev/unity-mcp

- **IvanMurzak Unity MCP** (`com.ivanmurzak.unity.mcp`)
  - GitHub: https://github.com/IvanMurzak/Unity-MCP

**Note**: Both frameworks can be installed simultaneously, and the package will automatically enable the corresponding adapters based on what's available.

## Usage

### Creating a VR Builder Process

The package provides a tool called `create_vr_builder_process` which will be invoked through your MCP-enabled AI assistant.
`create_vr_builder_process` expects as imput a JSON.

> **JSON foramt:** `com.ivanmurzak.unity.mcp` only works with camelCase field names (`processName`, `chapters`, `steps`, `name`, `description`, `position`, `overwrite`).
> The CoplayDev adapter is PascalCase compatible, but all samples use camelCase for consistency.

#### Example: Minimal Process (Default Empty Chapter)

Promt: `Create a VR Builder training process in my current Unity project named "MyNewProcess".`

The out put of your AI should be:
```json
{
  "processName": "MyNewProcess"
}
```
Note: If no chapters are specified, there will automatically be a default empty chapter named "Chapter 1"

#### Example: Multi-Chapter Process

Promt:
```text
Create a VR Builder training process in my current Unity project on how to make coffee with a French press. 
Each Step should have a description. In each chapter, start with x: 300 y: 0 and increase the x by 300 with each step. 
After three steps, move down and start with x: 300 y: 200, and so on.

Chapter 1: Preparation
Step: Heat Water
Bring water to the target temperature.
Step: Measure Coffee Beans
Allocate the correct mass of whole beans.
Step: Grind Beans
Produce a coarse, uniform grind.
Step: Warm French Press
Rinse the vessel with hot water to stabilize temperature.
Step: Discard Rinse Water
Empty the press completely before loading grounds.
Step: Add Coffee Grounds
Place the grounds into the empty press.

Chapter 2: Brewing
Step: Pour Initial Water
Add a small volume of hot water to saturate the grounds.
Step: Stir Bloom
Agitate the mixture to release trapped gases.
Step: Add Remaining Water
Fill the press to the intended brew level.
Step: Place Lid Assembly
Set the lid on top without plunging.
Step: Steep Brew
Allow the mixture to extract for the full duration.
Step: Plunge Filter
Press the plunger downward with steady force.

Chapter 3: Serving and Cleanup
Step: Pour Coffee
Decant the brew immediately to prevent over-extraction.
Step: Empty Grounds
Remove the spent grounds from the press.
Step: Rinse Components
Flush the vessel, plunger, and filter with hot water.
Step: Dry Equipment
Air-dry all parts to prevent residue buildup.
```

The out put of your AI should be:
```json
{
  "processName": "French Press Coffee",
  "chapters": [
    {
      "name": "Preparation",
      "steps": [
        { "name": "Heat Water", "description": "Bring water to the target temperature.", "position": [300, 0] },
        { "name": "Measure Coffee Beans", "description": "Allocate the correct mass of whole beans.", "position": [600, 0] },
        { "name": "Grind Beans", "description": "Produce a coarse, uniform grind.", "position": [900, 0] },
        { "name": "Warm French Press", "description": "Rinse the vessel with hot water to stabilize temperature.", "position": [300, 200] },
        { "name": "Discard Rinse Water", "description": "Empty the press completely before loading grounds.", "position": [600, 200] },
        { "name": "Add Coffee Grounds", "description": "Place the grounds into the empty press.", "position": [900, 200] }
      ]
    },
    {
      "name": "Brewing",
      "steps": [
        { "name": "Pour Initial Water", "description": "Add a small volume of hot water to saturate the grounds.", "position": [300, 0] },
        { "name": "Stir Bloom", "description": "Agitate the mixture to release trapped gases.", "position": [600, 0] },
        { "name": "Add Remaining Water", "description": "Fill the press to the intended brew level.", "position": [900, 0] },
        { "name": "Place Lid Assembly", "description": "Set the lid on top without plunging.", "position": [300, 200] },
        { "name": "Steep Brew", "description": "Allow the mixture to extract for the full duration.", "position": [600, 200] },
        { "name": "Plunge Filter", "description": "Press the plunger downward with steady force.", "position": [900, 200] }
      ]
    },
    {
      "name": "Serving and Cleanup",
      "steps": [
        { "name": "Pour Coffee", "description": "Decant the brew immediately to prevent over-extraction.", "position": [300, 0] },
        { "name": "Empty Grounds", "description": "Remove the spent grounds from the press.", "position": [600, 0] },
        { "name": "Rinse Components", "description": "Flush the vessel, plunger, and filter with hot water.", "position": [900, 0] },
        { "name": "Dry Equipment", "description": "Air-dry all parts to prevent residue buildup.", "position": [300, 200] }
      ]
    }
  ],
  "overwrite": false
}

```
### Response Format

Both adapters return a consistent JSON response structure containing the created process and the project-relative path to the created VR Builder process JSON.

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
