import os
import sys
import logging
from typing import List

from .base import BaseSubprocessExecutor
from worker.contracts import StageTaskMessage

logger = logging.getLogger(__name__)

STAGE_RUNNER_PATH = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "scripts", "stage_runner.py"
)


class PythonExecutor(BaseSubprocessExecutor):
    """
    Executor that runs a batch of Python scripts using the agent's Python environment.
    Sends the full StageTaskMessage as JSON via stdin to stage_runner.py,
    which dynamically resolves JIT gRPC inputs, executes main() of each script,
    and streams structured PIPELINE_EVENT outputs.
    """

    def __init__(self, python_path: str | None = None):
        self.python_path = python_path or sys.executable

    def build_command(self, task: StageTaskMessage) -> List[str]:
        return [self.python_path, STAGE_RUNNER_PATH]
