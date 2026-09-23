import os
import logging
from typing import List

from .base import BaseSubprocessExecutor
from worker.contracts import StageTaskMessage

logger = logging.getLogger(__name__)

STAGE_RUNNER_PATH = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "scripts", "stage_runner.py"
)


class BlenderExecutor(BaseSubprocessExecutor):
    """
    Executor that runs a batch of scripts inside a single Blender headless process.
    Sends the full StageTaskMessage as JSON via stdin to stage_runner.py,
    which orchestrates step execution in Blender's RAM.
    """

    def __init__(self, blender_path: str = "blender"):
        self.blender_path = blender_path

    def build_command(self, task: StageTaskMessage) -> List[str]:
        return [self.blender_path, "--background", "--python", STAGE_RUNNER_PATH]
