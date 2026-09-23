import os
import sys
import json
import socket
import urllib.request
import urllib.error
import uuid

# Cấu hình mặc định
DEFAULT_API_URL = "http://localhost:5189/api"
CONFIG_FILE = "agent_config.json"

def post_json(url, payload):
    data = json.dumps(payload).encode('utf-8')
    headers = {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    }
        
    req = urllib.request.Request(url, data=data, headers=headers, method='POST')
    try:
        with urllib.request.urlopen(req) as response:
            res_data = response.read().decode('utf-8')
            if res_data:
                return json.loads(res_data)
            return None
    except urllib.error.HTTPError as e:
        error_msg = e.read().decode('utf-8')
        print(f"\n[LỖI HTTP {e.code}]: {error_msg}")
        return None
    except Exception as e:
        print(f"\n[LỖI KẾT NỐI]: {str(e)}")
        return None

def get_machine_key():
    # Địa chỉ MAC + Hostname
    node = uuid.getnode()
    return f"{socket.gethostname()}-{node}"

def run():
    print("==================================================")
    print("    AUTOMATION AGENT REGISTRATION CLI (TOKEN)     ")
    print("==================================================")
    
    api_url = input(f"Nhập API Base URL [{DEFAULT_API_URL}]: ").strip()
    if not api_url:
        api_url = DEFAULT_API_URL
        
    print("\n--- Đăng ký Agent bằng Setup Token ---")
    setup_token = input("Nhập Setup Token (lấy từ Web Dashboard): ").strip()
    
    if not setup_token:
        print("Lỗi: Setup Token không được để trống!")
        sys.exit(1)
        
    hostname = socket.gethostname()
    machine_key = get_machine_key()
    
    print(f"\n[THÔNG TIN MÁY]")
    print(f" - Hostname    : {hostname}")
    print(f" - Machine Key : {machine_key}")
    print(f" - Setup Token : {setup_token}")
    
    print("\nĐang gửi yêu cầu kích hoạt Agent...")
    
    register_payload = {
        "setupToken": setup_token,
        "name": hostname,
        "machineKey": machine_key
    }
    
    agent_result = post_json(f"{api_url}/agents/register-token", register_payload)
    
    if not agent_result or 'registrationToken' not in agent_result:
        print("\n❌ Đăng ký Agent thất bại! Token không hợp lệ hoặc đã hết hạn.")
        sys.exit(1)
        
    print("\n==================================================")
    print(" 🎉 KÍCH HOẠT VÀ ĐĂNG KÝ AGENT THÀNH CÔNG!")
    print("==================================================")
    print(f" - Agent ID     : {agent_result.get('id')}")
    print(f" - Agent Name   : {agent_result.get('name')}")
    
    # Lưu cấu hình
    config = {
        "apiUrl": api_url,
        "agentId": agent_result.get("id"),
        "name": agent_result.get("name"),
        "machineKey": agent_result.get("machineKey"),
        "agentSecret": agent_result.get("registrationToken")
    }
    
    with open(CONFIG_FILE, 'w', encoding='utf-8') as f:
        json.dump(config, f, indent=4)
        
    print(f"\n[OK] Cấu hình Agent đã được lưu an toàn tại: {os.path.abspath(CONFIG_FILE)}")
    print("Agent sẵn sàng duy trì kết nối ngầm tới Backend!")
