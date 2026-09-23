import os
import hashlib
from core import agent_pb2

def calculate_hash(filepath):
    sha256 = hashlib.sha256()
    try:
        with open(filepath, 'rb') as f:
            while chunk := f.read(8192):
                sha256.update(chunk)
        return sha256.hexdigest()
    except Exception as e:
        print(f"Warning: Could not hash {filepath} - {e}")
        return ""

def scan_directory(base_dir, target_dir, recursive=True, extensions=None, ignored_dirs=None):
    if ignored_dirs is None:
        ignored_dirs = {"venv", ".git", "__pycache__", "node_modules", ".idea", ".vscode"}

    ext_set = set(e.lower() if e.startswith('.') else f".{e.lower()}" for e in extensions) if extensions else None

    files_to_sync = []
    try:
        if not os.path.exists(target_dir):
            print(f"Warning: Target directory does not exist: {target_dir}")
            return files_to_sync

        with os.scandir(target_dir) as entries:
            for entry in entries:
                if entry.name in ignored_dirs:
                    continue

                if entry.is_file():
                    if ext_set:
                        _, ext = os.path.splitext(entry.name)
                        if ext.lower() not in ext_set:
                            continue

                    rel_path = os.path.relpath(entry.path, base_dir).replace("\\", "/")
                    file_size = entry.stat().st_size
                    file_hash = calculate_hash(entry.path)
                    
                    files_to_sync.append(agent_pb2.ResourceItemMessage(
                        relative_path=rel_path,
                        hash=file_hash,
                        size_bytes=file_size
                    ))
                elif entry.is_dir() and recursive:
                    files_to_sync.extend(scan_directory(base_dir, entry.path, recursive=recursive, extensions=extensions, ignored_dirs=ignored_dirs))
    except Exception as e:
        print(f"Warning: Could not scan directory {target_dir} - {e}")

    return files_to_sync

def run(directory=".", recursive=True, extensions=None):
    abs_directory = os.path.abspath(directory)
    print(f"Scanning directory locally: {abs_directory} (Recursive={recursive})")
    files = scan_directory(abs_directory, abs_directory, recursive=recursive, extensions=extensions)
    print(f"Found {len(files)} files:")
    for item in files:
        print(f" - {item.relative_path} ({item.size_bytes} bytes, hash: {item.hash[:8]}...)")
