import os
import sys
import glob
import shutil
import subprocess
import re
import json
import logging

logger = logging.getLogger(__name__)

def _get_clean_version_from_output(output: str, pattern: str) -> str:
    match = re.search(pattern, output, re.IGNORECASE)
    if match:
        return match.group(1).strip()
    return ""

def _get_executable_version(exe_path: str, version_flag: str, pattern: str) -> str:
    try:
        # Bỏ qua Windows Store app execution aliases nếu file không thực sự chạy được
        if "WindowsApps" in exe_path and os.path.getsize(exe_path) == 0:
            return ""

        result = subprocess.run(
            [exe_path, version_flag],
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            timeout=5,
            creationflags=subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0
        )
        return _get_clean_version_from_output(result.stdout, pattern)
    except Exception as e:
        logger.debug(f"Failed to get version for {exe_path}: {e}")
        return ""

def scan_blender_executables() -> list[dict]:
    """
    Dò tìm các bản cài đặt Blender trên hệ thống (Windows, Linux, macOS)
    Trả về: [{"executor_key": "blender", "executable_path": path, "version": ver}]
    """
    found_paths = set()

    # 1. PATH lookup
    path_blender = shutil.which("blender")
    if path_blender and os.path.exists(path_blender):
        found_paths.add(os.path.abspath(path_blender))

    # 2. Windows-specific scanning
    if os.name == 'nt':
        # 2a. Registry scanning
        try:
            import winreg
            for root_key in (winreg.HKEY_LOCAL_MACHINE, winreg.HKEY_CURRENT_USER):
                try:
                    with winreg.OpenKey(root_key, r"SOFTWARE\Blender Foundation\Blender") as key:
                        num_subkeys, _, _ = winreg.QueryInfoKey(key)
                        for i in range(num_subkeys):
                            subkey_name = winreg.EnumKey(key, i)
                            try:
                                with winreg.OpenKey(key, rf"{subkey_name}\InstallDir") as install_key:
                                    install_dir, _ = winreg.QueryValueEx(install_key, "")
                                    exe = os.path.join(install_dir, "blender.exe")
                                    if os.path.exists(exe):
                                        found_paths.add(os.path.abspath(exe))
                            except Exception:
                                pass
                except Exception:
                    pass
        except Exception:
            pass

        # 2b. Standard directories pattern across all drives
        patterns = [
            r"C:\Program Files\Blender Foundation\Blender *\blender.exe",
            r"C:\Program Files (x86)\Blender Foundation\Blender *\blender.exe",
            r"D:\Program Files\Blender Foundation\Blender *\blender.exe",
            r"C:\Blender*\blender.exe",
            r"D:\Blender*\blender.exe",
            r"E:\Blender*\blender.exe",
            r"D:\Games\Blender*\blender.exe",
            r"C:\Games\Blender*\blender.exe",
            os.path.expandvars(r"%LOCALAPPDATA%\Programs\Blender Foundation\Blender *\blender.exe"),
            os.path.expandvars(r"%APPDATA%\Blender Foundation\Blender *\blender.exe"),
            # Steam installations
            r"C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe",
            r"D:\SteamLibrary\steamapps\common\Blender\blender.exe",
            r"E:\SteamLibrary\steamapps\common\Blender\blender.exe"
        ]
        for pattern in patterns:
            for match in glob.glob(pattern):
                if os.path.isfile(match):
                    found_paths.add(os.path.abspath(match))

    # 3. macOS
    elif sys.platform == 'darwin':
        mac_apps = [
            "/Applications/Blender.app/Contents/MacOS/Blender",
            os.path.expanduser("~/Applications/Blender.app/Contents/MacOS/Blender")
        ]
        for p in mac_apps:
            if os.path.isfile(p):
                found_paths.add(os.path.abspath(p))

    # 4. Linux
    else:
        linux_paths = [
            "/usr/bin/blender",
            "/usr/local/bin/blender",
            "/opt/blender/blender",
            "/snap/bin/blender",
            os.path.expanduser("~/.local/bin/blender")
        ]
        for p in linux_paths:
            if os.path.isfile(p):
                found_paths.add(os.path.abspath(p))

    # Verify each found executable, extract version, deduplicate normalized paths
    seen_paths = set()
    results = []
    for exe in sorted(found_paths):
        norm_path = os.path.abspath(exe).replace("\\", "/")
        norm_path_lower = norm_path.lower()
        if norm_path_lower in seen_paths:
            continue

        version = _get_executable_version(exe, "--version", r"Blender\s+([\d\.]+)")
        if version:
            seen_paths.add(norm_path_lower)
            results.append({
                "executor_key": "blender",
                "executable_path": norm_path,
                "version": version
            })

    return results

def scan_python_executables() -> list[dict]:
    """
    Dò tìm các bản cài đặt Python trên hệ thống
    Trả về: [{"executor_key": "python", "executable_path": path, "version": ver}]
    """
    found_paths = set()

    # 1. Current running python interpreter
    if sys.executable and os.path.exists(sys.executable):
        found_paths.add(os.path.abspath(sys.executable))

    # 2. PATH lookups
    for name in ["python", "python3", "python3.13", "python3.12", "python3.11", "python3.10", "python3.9"]:
        p = shutil.which(name)
        if p and os.path.exists(p):
            found_paths.add(os.path.abspath(p))

    # 3. Windows-specific scanning
    if os.name == 'nt':
        # 3a. 'py -0p' Windows launcher
        py_launcher = shutil.which("py")
        if py_launcher:
            try:
                out = subprocess.run(
                    ["py", "-0p"],
                    stdout=subprocess.PIPE,
                    stderr=subprocess.STDOUT,
                    text=True,
                    timeout=5,
                    creationflags=subprocess.CREATE_NO_WINDOW
                ).stdout
                # Format: " -V:3.11 *        C:\Python311\python.exe"
                for line in out.splitlines():
                    match = re.search(r"(\w:[\\/][^*]+python\.exe)", line, re.IGNORECASE)
                    if match:
                        exe = match.group(1).strip()
                        if os.path.isfile(exe):
                            found_paths.add(os.path.abspath(exe))
            except Exception:
                pass

        # 3b. Registry scanning
        try:
            import winreg
            for root_key in (winreg.HKEY_LOCAL_MACHINE, winreg.HKEY_CURRENT_USER):
                try:
                    with winreg.OpenKey(root_key, r"SOFTWARE\Python\PythonCore") as key:
                        num_subkeys, _, _ = winreg.QueryInfoKey(key)
                        for i in range(num_subkeys):
                            ver_key_name = winreg.EnumKey(key, i)
                            try:
                                with winreg.OpenKey(key, rf"{ver_key_name}\InstallPath") as ip_key:
                                    install_dir, _ = winreg.QueryValueEx(ip_key, "")
                                    exe = os.path.join(install_dir, "python.exe")
                                    if os.path.isfile(exe):
                                        found_paths.add(os.path.abspath(exe))
                            except Exception:
                                pass
                except Exception:
                    pass
        except Exception:
            pass

        # 3c. Standard directories
        patterns = [
            r"C:\Python*\python.exe",
            r"D:\Python*\python.exe",
            r"C:\Program Files\Python*\python.exe",
            r"C:\Program Files (x86)\Python*\python.exe",
            os.path.expandvars(r"%LOCALAPPDATA%\Programs\Python\Python*\python.exe"),
            os.path.expandvars(r"%APPDATA%\Local\Programs\Python\Python*\python.exe"),
            r"C:\Users\*\AppData\Local\Programs\Python\Python*\python.exe"
        ]
        for pattern in patterns:
            for match in glob.glob(pattern):
                if os.path.isfile(match):
                    found_paths.add(os.path.abspath(match))

    # 4. Unix lookups
    else:
        unix_patterns = [
            "/usr/bin/python3*",
            "/usr/local/bin/python3*",
            os.path.expanduser("~/.pyenv/versions/*/bin/python"),
            os.path.expanduser("~/.local/bin/python3*")
        ]
        for pattern in unix_patterns:
            for match in glob.glob(pattern):
                if os.path.isfile(match) and not match.endswith("-config"):
                    found_paths.add(os.path.abspath(match))

    # Verify and extract versions
    seen_paths = set()
    results = []
    for exe in sorted(found_paths):
        norm_path = os.path.abspath(exe).replace("\\", "/")
        norm_path_lower = norm_path.lower()
        if norm_path_lower in seen_paths:
            continue

        version = _get_executable_version(exe, "--version", r"Python\s+([\d\.]+)")
        if version:
            seen_paths.add(norm_path_lower)
            results.append({
                "executor_key": "python",
                "executable_path": norm_path,
                "version": version
            })

    return results

def _get_unreal_version_from_path(exe_path: str) -> str:
    """
    Extract Unreal Engine version from Build.version file or directory name.
    """
    try:
        # 1. Look for Engine/Build/Build.version
        # exe is in Engine/Binaries/Win64/UnrealEditor-Cmd.exe -> 3 levels up is Engine
        binaries_dir = os.path.dirname(exe_path)
        engine_dir = os.path.dirname(os.path.dirname(binaries_dir))
        build_version_file = os.path.join(engine_dir, "Build", "Build.version")
        if os.path.isfile(build_version_file):
            with open(build_version_file, "r", encoding="utf-8") as f:
                data = json.load(f)
                major = data.get("MajorVersion")
                minor = data.get("MinorVersion")
                patch = data.get("PatchVersion", 0)
                if major is not None and minor is not None:
                    return f"{major}.{minor}.{patch}"

        # 2. Fallback regex on executable path (e.g. UE_5.8 or Unreal Engine 5.7)
        match = re.search(r"UE_([\d\.]+)", exe_path, re.IGNORECASE) or re.search(r"Unreal\s*Engine[\\/]+([\d\.]+)", exe_path, re.IGNORECASE)
        if match:
            return match.group(1).strip()
    except Exception as e:
        logger.debug(f"Failed to read Unreal version for {exe_path}: {e}")

    return "5.0.0"


def scan_unreal_executables() -> list[dict]:
    """
    Dò tìm các bản cài đặt Unreal Engine (UnrealEditor-Cmd.exe) trên hệ thống (Windows, macOS, Linux).
    Trả về: [{"executor_key": "unreal", "executable_path": path, "version": ver}]
    """
    found_paths = set()

    # 1. Environment variables
    for env_var in ["UNREAL_CMD_PATH", "UE_CMD_PATH", "UNREAL_ENGINE_PATH"]:
        val = os.environ.get(env_var, "").strip()
        if val and os.path.isfile(val):
            found_paths.add(os.path.abspath(val))

    # 2. PATH lookup
    for cmd_name in ["UnrealEditor-Cmd", "UnrealEditor-Cmd.exe"]:
        p = shutil.which(cmd_name)
        if p and os.path.isfile(p):
            found_paths.add(os.path.abspath(p))

    # 3. Windows-specific scanning
    if os.name == 'nt':
        # 3a. Registry scanning (HKLM \ SOFTWARE \ EpicGames \ Unreal Engine)
        try:
            import winreg
            for root_key in (winreg.HKEY_LOCAL_MACHINE, winreg.HKEY_CURRENT_USER):
                try:
                    with winreg.OpenKey(root_key, r"SOFTWARE\EpicGames\Unreal Engine") as key:
                        num_subkeys, _, _ = winreg.QueryInfoKey(key)
                        for i in range(num_subkeys):
                            ver_name = winreg.EnumKey(key, i)
                            try:
                                with winreg.OpenKey(key, ver_name) as ver_key:
                                    install_dir, _ = winreg.QueryValueEx(ver_key, "InstalledDirectory")
                                    exe = os.path.join(install_dir, "Engine", "Binaries", "Win64", "UnrealEditor-Cmd.exe")
                                    if os.path.isfile(exe):
                                        found_paths.add(os.path.abspath(exe))
                            except Exception:
                                pass
                except Exception:
                    pass

                # 3b. Registry scanning (HKCU \ SOFTWARE \ Epic Games \ Unreal Engine \ Builds)
                try:
                    with winreg.OpenKey(root_key, r"SOFTWARE\Epic Games\Unreal Engine\Builds") as key:
                        num_values, _, _ = winreg.QueryInfoKey(key)
                        for i in range(num_values):
                            _, install_dir, _ = winreg.EnumValue(key, i)
                            if install_dir:
                                exe = os.path.join(install_dir, "Engine", "Binaries", "Win64", "UnrealEditor-Cmd.exe")
                                if os.path.isfile(exe):
                                    found_paths.add(os.path.abspath(exe))
                except Exception:
                    pass
        except Exception:
            pass

        # 3c. Common installation directories patterns across all drives
        patterns = [
            r"C:\Program Files\Epic Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"D:\Program Files\Epic Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"E:\Program Files\Epic Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"C:\Epic Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"D:\Epic Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"E:\Epic Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"D:\Games\UnrealEngine\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"C:\Games\UnrealEngine\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"D:\Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"C:\Games\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"D:\UnrealEngine\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
            r"C:\UnrealEngine\UE_*\Engine\Binaries\Win64\UnrealEditor-Cmd.exe",
        ]
        for pattern in patterns:
            for match in glob.glob(pattern):
                if os.path.isfile(match):
                    found_paths.add(os.path.abspath(match))

    # 4. macOS
    elif sys.platform == 'darwin':
        mac_patterns = [
            "/Users/Shared/Epic Games/UE_*/Engine/Binaries/Mac/UnrealEditor-Cmd",
            "/Applications/Epic Games/UE_*/Engine/Binaries/Mac/UnrealEditor-Cmd",
        ]
        for pattern in mac_patterns:
            for match in glob.glob(pattern):
                if os.path.isfile(match):
                    found_paths.add(os.path.abspath(match))

    # 5. Linux
    else:
        linux_patterns = [
            "/opt/EpicGames/UE_*/Engine/Binaries/Linux/UnrealEditor-Cmd",
            "/opt/unreal-engine/Engine/Binaries/Linux/UnrealEditor-Cmd",
            os.path.expanduser("~/EpicGames/UE_*/Engine/Binaries/Linux/UnrealEditor-Cmd"),
        ]
        for pattern in linux_patterns:
            for match in glob.glob(pattern):
                if os.path.isfile(match):
                    found_paths.add(os.path.abspath(match))

    # Deduplicate and extract versions
    seen_paths = set()
    results = []
    for exe in sorted(found_paths, reverse=True):
        norm_path = os.path.abspath(exe).replace("\\", "/")
        norm_path_lower = norm_path.lower()
        if norm_path_lower in seen_paths:
            continue

        version = _get_unreal_version_from_path(exe)
        seen_paths.add(norm_path_lower)
        results.append({
            "executor_key": "unreal",
            "executable_path": norm_path,
            "version": version
        })

    return results


def scan_all_executors(target_key: str | None = None) -> list[dict]:
    """
    Quét danh sách các candidate executor trên máy.
    target_key: "blender", "python", "unreal" hoặc None/rỗng để quét tất cả.
    """
    results = []
    key = (target_key or "").strip().lower()

    if not key or key == "blender":
        results.extend(scan_blender_executables())

    if not key or key == "python":
        results.extend(scan_python_executables())

    if not key or key in ("unreal", "unrealengine", "ue"):
        results.extend(scan_unreal_executables())

    return results


if __name__ == "__main__":
    print("--- Scanning Executors ---")
    candidates = scan_all_executors()
    for c in candidates:
        print(f"[{c['executor_key']}] {c['version']} -> {c['executable_path']}")
