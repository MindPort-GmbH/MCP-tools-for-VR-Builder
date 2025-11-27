from typing import Annotated, Any
from fastmcp import Context
from registry import mcp_for_unity_tool
from unity_connection import send_command_with_retry


@mcp_for_unity_tool(
description="""Creates a VR Builder process and saves it to StreamingAssets/Processes/{processName}/{processName}.json
Each chapter includes an automatic Start step at [0, 0]. First steps should be positioned at [300, 0] to avoid overlap.
If no chapters provided, creates a default empty chapter. Empty chapters are allowed.
Steps auto-generate names as Step 1, Step 2, etc. if not provided."""
)
async def create_vr_builder_process(
    ctx: Context,
    ProcessName: Annotated[str, "Name of the process."],
    Chapters: Annotated[
        list[dict[str, Any]] | None,
        "Chapters data. Each chapter includes an automatic Start step at [0, 0]. If null/empty, creates default Chapter 1."
    ] = None,
    Steps: Annotated[
        list[str | dict[str, str]] | None,
        "Steps inside the chapter. Empty chapters are allowed."
    ] = None,
    Overwrite: Annotated[bool, "Overwrite the target file if it already exists."] = False,
) -> dict[str, Any]:
    """
    Creates a VR Builder process with structured chapters and steps.

    Chapter properties:
    - name: Chapter name.
    - steps: Steps inside the chapter. Empty chapters are allowed.

    Step properties:
    - name: Step name. Auto-generates as Step 1, Step 2, etc. if not provided
    - description: Step description.
    - position: Position for visual layout as [x, y] array. Suggested default: [300, 0]
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
