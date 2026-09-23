import os
import shutil
import zipfile
import urllib.request
import logging

logger = logging.getLogger(__name__)

CACHE_BASE_DIR = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "worker", "cache")


def resolve_script_path(
    script_path: str = "",
    script_url: str = None,
    script_hash: str = None,
    entry_point: str = None
) -> str:
    """
    Phân giải đường dẫn script thực thi:
    1. Nếu có script_hash & script_url (hoặc entry_point): Tải và giải nén vào worker/cache/{script_hash}/ giống Inspector.
    2. Fallback tìm kiếm trong worker/scripts/pipeline/ hoặc worker/scripts/ cho các built-in scripts.
    """
    # 1. Cached remote script (Asset Slot download)
    if script_hash:
        entry = entry_point or (os.path.basename(script_path) if script_path else "main.py")
        if not entry.endswith(".py"):
            entry += ".py"

        cached_folder = os.path.join(CACHE_BASE_DIR, script_hash)
        entry_file_path = os.path.join(cached_folder, entry)

        if os.path.exists(entry_file_path):
            logger.debug(f"Sử dụng pipeline script đã cache tại: {entry_file_path}")
            return entry_file_path

        os.makedirs(cached_folder, exist_ok=True)

        if script_url:
            temp_download_path = os.path.join(cached_folder, "downloaded_script")
            logger.info(f"Đang tải pipeline script từ {script_url}...")
            try:
                req = urllib.request.Request(
                    script_url,
                    headers={
                        "User-Agent": "Mozilla/5.0 (Automation Agent)"
                    }
                )
                with urllib.request.urlopen(req, timeout=30) as response, open(temp_download_path, 'wb') as out_file:
                    shutil.copyfileobj(response, out_file)
            except Exception as dl_err:
                logger.warning(f"Không thể tải script trực tiếp từ URL ({dl_err}).")

            if os.path.exists(temp_download_path):
                if zipfile.is_zipfile(temp_download_path):
                    with zipfile.ZipFile(temp_download_path, 'r') as zip_ref:
                        zip_ref.extractall(cached_folder)
                    try:
                        os.remove(temp_download_path)
                    except Exception:
                        pass
                else:
                    target_script = os.path.join(cached_folder, entry)
                    if os.path.exists(target_script):
                        os.remove(target_script)
                    shutil.move(temp_download_path, target_script)

        if os.path.exists(entry_file_path):
            return entry_file_path

    # 2. Local built-in pipeline / inspector scripts
    if not script_path:
        return ""

    if os.path.isabs(script_path):
        candidates = [script_path, script_path + ".py"]
    else:
        scripts_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "worker", "scripts")
        clean_name = script_path if not script_path.endswith(".py") else script_path[:-3]
        
        candidates = [
            os.path.join(scripts_dir, script_path),
            os.path.join(scripts_dir, f"{clean_name}.py"),
            os.path.join(scripts_dir, "pipeline", script_path),
            os.path.join(scripts_dir, "pipeline", f"{clean_name}.py"),
            os.path.join(scripts_dir, "pipeline", "blender", script_path),
            os.path.join(scripts_dir, "pipeline", "blender", f"{clean_name}.py"),
            os.path.join(scripts_dir, "pipeline", "daz", script_path),
            os.path.join(scripts_dir, "pipeline", "daz", f"{clean_name}.py"),
            os.path.join(scripts_dir, "inspectors", script_path),
            os.path.join(scripts_dir, "inspectors", f"{clean_name}.py"),
            os.path.join(scripts_dir, "inspectors", "blender", script_path),
            os.path.join(scripts_dir, "inspectors", "blender", f"{clean_name}.py"),
            os.path.join(scripts_dir, "inspectors", "daz", script_path),
            os.path.join(scripts_dir, "inspectors", "daz", f"{clean_name}.py"),
        ]

    for cand in candidates:
        if os.path.exists(cand):
            return os.path.abspath(cand)

    # Recursive fallback search by filename inside worker/scripts/
    scripts_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "worker", "scripts")
    target_filename = os.path.basename(script_path)
    if not target_filename.endswith(".py"):
        target_filename += ".py"
    for root, _, files in os.walk(scripts_dir):
        if target_filename in files:
            return os.path.abspath(os.path.join(root, target_filename))

    return ""


def resolve_file_asset(url: str, file_hash: str = None, file_name: str = None) -> str:
    """
    Download and cache a file asset from a remote URL to worker/cache/assets/{hash}/{filename}.
    Returns the absolute local file path.
    """
    if not url:
        return ""

    folder_key = str(file_hash or "generic").strip()
    cached_folder = os.path.join(CACHE_BASE_DIR, "assets", folder_key)
    os.makedirs(cached_folder, exist_ok=True)

    fname = file_name or os.path.basename(url.split("?")[0]) or "asset_file"
    target_path = os.path.join(cached_folder, fname)

    if os.path.exists(target_path) and os.path.getsize(target_path) > 0:
        logger.debug(f"Using cached asset file: {target_path}")
        return os.path.abspath(target_path)

    logger.info(f"Downloading file asset from {url} to {target_path}...")
    try:
        req = urllib.request.Request(
            url,
            headers={"User-Agent": "Mozilla/5.0 (Automation Agent)"}
        )
        with urllib.request.urlopen(req, timeout=60) as response, open(target_path, 'wb') as out_file:
            shutil.copyfileobj(response, out_file)
        return os.path.abspath(target_path)
    except Exception as e:
        logger.error(f"Failed to download file asset from {url}: {e}")
        return ""

