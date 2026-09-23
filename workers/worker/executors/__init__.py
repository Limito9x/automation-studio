from .base import BaseExecutor, ExecutionResult, StepResult
from .blender_executor import BlenderExecutor
from .python_executor import PythonExecutor
from .unreal_executor import UnrealEngineExecutor

EXECUTOR_REGISTRY: dict[str, BaseExecutor] = {
    "blender": BlenderExecutor(),
    "python": PythonExecutor(),
    "unreal": UnrealEngineExecutor(),
}


def get_executor(name: str) -> BaseExecutor:
    """Get executor by name. Raises KeyError if not found."""
    executor = EXECUTOR_REGISTRY.get(name)
    if executor is None:
        raise KeyError(f"Unknown executor: '{name}'. Available: {list(EXECUTOR_REGISTRY.keys())}")
    return executor
