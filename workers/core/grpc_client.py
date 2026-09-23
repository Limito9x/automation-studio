import grpc
import urllib.parse
from core.config import get_config
from core import agent_pb2
from core import agent_pb2_grpc

def get_channel():
    config = get_config()
    
    # 1. Ưu tiên grpcUrl nếu được chỉ định riêng trong agent_config.json
    grpc_url = config.get("grpcUrl")
    
    if not grpc_url:
        # 2. Nếu không có, suy ra từ apiUrl
        api_url = config.get("apiUrl", "http://localhost:5189")
        parsed_api = urllib.parse.urlparse(api_url)
        host = parsed_api.hostname or "localhost"
        
        # Với HTTP không mã hóa (h2c), Kestrel mở port gRPC riêng (mặc định 50051)
        if parsed_api.scheme == "https":
            port = parsed_api.port or 443
            grpc_url = f"https://{host}:{port}"
        else:
            grpc_url = f"http://{host}:50051"

    parsed = urllib.parse.urlparse(grpc_url)
    host = parsed.hostname or "localhost"
    port = parsed.port or (443 if parsed.scheme == "https" else 50051)
    target = f"{host}:{port}"

    if parsed.scheme == "https":
        credentials = grpc.ssl_channel_credentials()
        return grpc.secure_channel(target, credentials)
    else:
        return grpc.insecure_channel(target)

def get_agent_stub():
    channel = get_channel()
    return agent_pb2_grpc.AgentServiceStub(channel)
