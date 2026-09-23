# Monorepo General Rules

Những quy tắc này áp dụng cho toàn bộ workspace Monorepo `Automation/`.

---

## 1. Ưu Tiên Sử Dụng CodeGraph MCP (MANDATORY)

- **Primary Tool:** Bắt buộc ưu tiên sử dụng MCP tool `codegraph_explore` (hoặc lệnh shell `codegraph explore`) là công cụ ĐẦU TIÊN để phân tích kiến trúc, dò tìm symbol, xem references/callers/callees, và đọc code chính xác.
- **projectPath Parameter:** Khi gọi `codegraph_explore`, luôn truyền trực tiếp tham số `projectPath` trỏ tới root `D:/FullStack/Automation` (hoặc thư mục subproject).
- **Hạn chế Grep/View mò mẫm:** Không dùng `grep_search`, `list_dir`, hoặc `view_file` để tìm kiếm mù mờ khi chưa dùng CodeGraph để nắm bức tranh tổng thể.

---

## 2. Quản Lý Terminal & Tiến Trình Nền (Process Management)

- **MANDATORY TERMINAL CONTROL:** Bắt buộc ưu tiên thực thi các lệnh chạy hữu hạn trong terminal mà User có thể thấy và điều khiển.
- **CLEAN UP BACKGROUND TASKS:** Tuyệt đối KHÔNG để server/tiến trình (như `dotnet run`, `pnpm dev`, `python cli.py start`) chạy ngầm vĩnh viễn trong background.
  - Nếu phải chạy background task để kiểm tra ngắn hạn, BẮT BUỘC phải dùng tool `manage_task` với action `kill` để tắt tiến trình ngay lập tức sau khi kiểm tra xong.
  - Tránh triệt để việc giam giữ cổng mạng (port collision) hoặc khóa file `.dll` / `.exe` trên Windows.
- **User Control:** Luôn trả lại quyền kiểm soát terminal cho User sau khi xác nhận code hoạt động. User sẽ tự chủ động khởi chạy server khi cần làm việc.

---

## 3. Ngôn Ngữ & Quy Ước Hiển Thị (Language Conventions)

- **UI & Code:** Toàn bộ văn bản hiển thị cho người dùng (UI text, nhãn, thông báo lỗi, placeholder), tài liệu commit, và tên biến/hàm/lớp PHẢI viết bằng **tiếng Anh**.
- **Agent Chat & Plans:** Khi User viết tiếng Việt, Agent phản hồi bằng **tiếng Việt**. Tất cả tài liệu kế hoạch (`implementation_plan.md`, `walkthrough.md`) viết bằng **tiếng Việt**.
- **Comments & Logs:** Có thể viết bằng **tiếng Việt** hoặc **tiếng Anh**.

---

## 4. Điều Hướng Theo Phạm Vi (Scope Routing)

Khi thực hiện tác vụ, hãy xác định khu vực mã nguồn đang thao tác để đọc và tuân thủ các quy tắc riêng biệt:
- **`api/` (Backend)**: Tuân thủ nghiêm ngặt [rules/backend.md](file:///d:/FullStack/Automation/.agents/rules/backend.md).
- **`web/` (Frontend)**: Tuân thủ nghiêm ngặt [rules/frontend.md](file:///d:/FullStack/Automation/.agents/rules/frontend.md).
- **`workers/` (Workers)**: Tuân thủ nghiêm ngặt [rules/workers.md](file:///d:/FullStack/Automation/.agents/rules/workers.md).
