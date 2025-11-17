from typing import Annotated, Any
from fastmcp import Context
from registry import mcp_for_unity_tool
from unity_connection import send_command_with_retry


@mcp_for_unity_tool(
description="""Create a VR Builder training process with chapters and steps.
Each chapter has a `name` and `steps`. Each step has `name`, optional `description`, and optional `position` (x, y).
Each chapter has an automatic Start step at (0,0). First steps should be positioned at (300, 0) to avoid overlap."""
)
async def create_vr_builder_process(
    ctx: Context,
    ProcessName: Annotated[str, "Name of the process (becomes folder and file name)"],
    Chapters: Annotated[
        list[dict[str, Any]] | None,
        """List of chapters with 'name' (str) and 'steps' (list of dicts with 'name', optional 'description', and optional 'position')."""
    ] = None,
    Steps: Annotated[
        list[str | dict[str, str]] | None,
        """For single-chapter processes (ignored if Chapters provided). Strings or dicts with name/description/position."""
    ] = None,
    Overwrite: Annotated[bool, "Set true to overwrite existing file"] = False,
) -> dict[str, Any]:
    """
    Creates a VR Builder process with structured chapters and steps.

    STRUCTURE:
    process → chapters (with name) → steps (with name + description + position)

    PRIORITY: chapters > steps > default (one chapter, one step)

    FORMAT EXAMPLE:
    {
      "processName": "myTrainingProcess",
      "chapters": [
        {
          "name": "Introduction",
          "steps": [
            {
              "name": "Welcome",
              "description": "Introduction to the training module",
              "position": {"x": 300, "y": 0}
            }
          ]
        },
        {
          "name": "Main Tasks",
          "steps": [
            {
              "name": "Task 1",
              "description": "Perform the first task",
              "position": {"x": 300, "y": 0}
            },
            {
              "name": "Task 2",
              "description": "Perform the second task",
              "position": {"x": 600, "y": 0}
            },
            {
              "name": "Task 3",
              "description": "Perform the third task",
              "position": {"x": 900, "y": 0}
            }
          ]
        }
      ],
      "overwrite": true
    }

    POSITIONING:
    - Position format: {"x": float, "y": float} or [x, y] array
    - If Position omitted, defaults to horizontal layout: ((index + 1) * 300, 0)
    - This offset prevents overlap with the Start step at (0,0)

    Returns: {"success": bool, "message": str, "data": {"name", "path", "chapters", "steps", "bytes"}}
    """
    await ctx.info(f"create_vr_builder_process: ProcessName={ProcessName}")

    params: dict[str, Any] = {
        "ProcessName": ProcessName,
        "Chapters": Chapters,
        "Steps": Steps,
        "Overwrite": Overwrite,
    }
    # Strip None values for cleaner payload
    params = {k: v for k, v in params.items() if v is not None}

    response = send_command_with_retry("create_vr_builder_process", params)
    return response if isinstance(response, dict) else {"success": False, "message": str(response)}
