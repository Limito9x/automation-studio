import argparse
import sys
import os

# Thêm thư mục core vào sys.path để gRPC generated files (agent_pb2_grpc) import được agent_pb2
sys.path.append(os.path.abspath("core"))

from commands import register, scan, health, start, browse, connect

def main():
    parser = argparse.ArgumentParser(description="Automation Agent CLI")
    subparsers = parser.add_subparsers(dest="command", help="Available commands")

    # Register command
    subparsers.add_parser("register", help="Register the agent with the server")
    
    # Start command
    subparsers.add_parser("start", help="Start the background workers (Pipeline + Inspector) and gRPC stream")

    # Connect command
    subparsers.add_parser("connect", help="Connect to gRPC bidirectional stream")

    # Browse command
    browse_parser = subparsers.add_parser("browse", help="Browse directory (1 level, lazy-loading)")
    browse_parser.add_argument("--dir", type=str, default=".", help="Directory to browse")

    # Scan command
    scan_parser = subparsers.add_parser("scan", help="Scan local directory and sync resources")
    scan_parser.add_argument("--dir", type=str, default=".", help="Directory to scan")
    scan_parser.add_argument("-r", "--recursive", action="store_true", help="Scan subdirectories recursively")

    # Health command
    subparsers.add_parser("health", help="Send a one-off health check")

    # Detect Executors command
    detect_parser = subparsers.add_parser("detect-executors", help="Scan local software installations (Blender, Python)")
    detect_parser.add_argument("--key", type=str, default="", help="Specific executor key to scan (blender/python)")

    # Inspect Worker command
    inspect_worker_parser = subparsers.add_parser("inspect-worker", help="Start only the RabbitMQ inspector worker")
    inspect_worker_parser.add_argument("--host", type=str, default=None, help="RabbitMQ host")

    # Purge queues command
    subparsers.add_parser("purge", help="Purge all pending messages in RabbitMQ queues")

    # Compile Protos command
    subparsers.add_parser("compile-protos", help="Compile gRPC protos from packages/proto into workers/core")

    args = parser.parse_args()

    if args.command == "register":
        register.run()
    elif args.command == "start":
        start.run()
    elif args.command == "connect":
        connect.run()
    elif args.command == "browse":
        browse.run(args.dir)
    elif args.command == "scan":
        scan.run(args.dir, recursive=args.recursive)
    elif args.command == "health":
        health.run()
    elif args.command == "purge":
        from commands import purge
        purge.run()
    elif args.command == "compile-protos":
        from commands import compile_protos
        compile_protos.run()
    elif args.command == "detect-executors":
        from commands.detect_environment import scan_all_executors
        print(f"--- Scanning Executors (Filter: '{args.key or 'ALL'}') ---")
        candidates = scan_all_executors(args.key if args.key else None)
        for c in candidates:
            print(f"  [{c['executor_key']}] {c['version']} -> {c['executable_path']}")
        print(f"Total found: {len(candidates)}")
    elif args.command in ("worker", "pipeline-worker", "inspect-worker"):
        from worker.pipeline_consumer import PipelineConsumer
        consumer = PipelineConsumer(host=args.host)
        consumer.start()
    else:
        parser.print_help()
        sys.exit(1)

if __name__ == "__main__":
    main()
