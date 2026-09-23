from core.grpc_client import get_channel
from core.config import get_config
from core import agent_pb2, agent_pb2_grpc

def run():
    config = get_config()
    agent_id = config.get("agentId")
    if not agent_id:
        print("[ERROR] Error: agentId not found in agent_config.json.")
        return

    print(f"[INFO] Sending HealthCheck for Agent ID: {agent_id}...")
    
    try:
        channel = get_channel()
        stub = agent_pb2_grpc.AgentServiceStub(channel)
        
        def single_message():
            yield agent_pb2.AgentMessage(agent_id=agent_id)

        stream = stub.Connect(single_message())
        print(f"[OK] Agent ID {agent_id} connected successfully to gRPC server!")
    except Exception as e:
        print(f"[ERROR] Could not connect to gRPC server: {e}")
