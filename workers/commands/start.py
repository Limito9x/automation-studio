import threading
import time
from commands import connect

try:
    from worker.pipeline_consumer import PipelineConsumer
except Exception as e:
    PipelineConsumer = None

def run_pipeline_worker():
    if PipelineConsumer is None:
        return
    while True:
        try:
            consumer = PipelineConsumer()
            consumer.start()
        except KeyboardInterrupt:
            break
        except Exception as e:
            print(f"[WARNING] [RabbitMQ] Pipeline Worker disconnected: {e}. Reconnecting in 3s...")
            time.sleep(3)

def run():
    print("[INFO] Starting Automation Agent...")
    
    # 1. Start unified RabbitMQ Pipeline Worker in background thread
    if PipelineConsumer is not None:
        t1 = threading.Thread(target=run_pipeline_worker, daemon=True)
        t1.start()
        print("[OK] Background Pipeline Task Worker thread initialized.")
    
    # 2. Start gRPC Connect stream to maintain heartbeat & listen for commands from Backend
    print("[OK] Starting gRPC Stream & Command Listener...")
    try:
        connect.run()
    except KeyboardInterrupt:
        print("\n[STOP] Agent stopped.")

