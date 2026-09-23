import os
import json

IGNORED_NAMES = {
    ".git", "venv", "__pycache__", "node_modules", ".idea", ".vscode",
    "$recycle.bin", "system volume information", "config.msi", "recovery",
    "msocache", "perflogs"
}

def is_hidden_or_system(entry):
    name_lower = entry.name.lower()
    if name_lower in IGNORED_NAMES or entry.name.startswith((".", "$")):
        return True
    
    if os.name == 'nt':
        try:
            attrs = entry.stat(follow_symlinks=False).st_file_attributes
            if attrs & (0x02 | 0x04):
                return True
        except Exception:
            pass
    return False

def run(directory="."):
    target_dir = directory.strip()
    if len(target_dir) == 2 and target_dir[1] == ':' and target_dir[0].isalpha():
        target_dir = f"{target_dir[0].upper()}:/"

    abs_directory = os.path.abspath(target_dir)
    print(f"Browsing directory: {abs_directory}")

    items = []
    try:
        with os.scandir(abs_directory) as entries:
            for entry in entries:
                if is_hidden_or_system(entry):
                    continue

                is_dir = entry.is_dir()
                size = 0 if is_dir else entry.stat().st_size
                
                items.append({
                    "name": entry.name,
                    "path": os.path.relpath(entry.path, abs_directory).replace("\\", "/"),
                    "is_dir": is_dir,
                    "size_bytes": size
                })
    except Exception as e:
        print(f"[ERROR] Could not browse directory {abs_directory}: {e}")
        return

    # Sort directories first, then files
    items.sort(key=lambda x: (not x["is_dir"], x["name"].lower()))

    print(f"\nFound {len(items)} items in {abs_directory}:")
    print("-" * 60)
    print(f"{'TYPE':<10} {'SIZE (BYTES)':<15} {'NAME'}")
    print("-" * 60)
    for item in items:
        item_type = "<DIR>" if item["is_dir"] else "<FILE>"
        size_str = "-" if item["is_dir"] else str(item["size_bytes"])
        print(f"{item_type:<10} {size_str:<15} {item['name']}")
    print("-" * 60)
