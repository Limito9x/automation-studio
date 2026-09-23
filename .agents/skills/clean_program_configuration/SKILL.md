---
name: clean_program_configuration
description: Hướng dẫn cấu hình DI và các services trong Program.cs sao cho gọn gàng bằng cách sử dụng ServiceCollectionExtensions.
---

# Cấu hình Program.cs gọn gàng (Clean Configuration)

Khi có yêu cầu thêm cấu hình mới cho chương trình (ví dụ: cấu hình Caching, Authentication, kết nối Database thứ 3, hoặc đăng ký các services cơ sở hạ tầng), BẮT BUỘC phải tuân thủ nguyên tắc "Clean Program.cs".

Tư tưởng cốt lõi: **Không viết trực tiếp các đoạn code cấu hình (DI, đọc config) dài dòng vào file `Program.cs`. Thay vào đó, hãy viết các Extension Methods.**

### Các bước thực hiện khi thêm cấu hình:

1. **Xác định Feature/Nhóm cấu hình:**
   - Xác định xem cấu hình này thuộc nhóm nào (VD: `Caching`, `Auth`, `Storage`, `Logging`, ...).

2. **Tạo file Extension Method ở SharedKernel:**
   - Đi đến thư mục `src/SharedKernel/SharedKernel/Extensions/<Nhóm_Cấu_Hình>/`.
   - Tạo file mang tên `<Nhóm_Cấu_Hình>ServiceCollectionExtensions.cs` (VD: `CachingServiceCollectionExtensions.cs`).
   
3. **Viết Extension Method:**
   - Cài đặt một class `static` và một hàm `static` mở rộng cho `IServiceCollection`.
   - Ví dụ:
     ```csharp
     using Microsoft.Extensions.DependencyInjection;
     using Microsoft.Extensions.Configuration;

     namespace SharedKernel.Extensions.Caching;

     public static class CachingServiceCollectionExtensions
     {
         public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
         {
             // Đọc config và đăng ký services tại đây
             var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
             // ...
             return services;
         }
     }
     ```

4. **Sử dụng trong `Program.cs`:**
   - Mở file `Program.cs` của ứng dụng (`Automation.Api`).
   - `using SharedKernel.Extensions.<Nhóm_Cấu_Hình>;`
   - Gọi hàm extension một cách ngắn gọn:
     ```csharp
     builder.Services.AddRedisCache(builder.Configuration);
     ```

### Lưu ý quan trọng
- Luôn gom nhóm các cấu hình liên quan vào chung một file extension (VD: mọi cấu hình về Auth thì để trong `AuthServiceCollectionExtensions.cs`).
- Mục đích cuối cùng là giữ cho file `Program.cs` chỉ chứa danh sách các lời gọi hàm `.Add...()` và `.Use...()` mạch lạc, dễ đọc.


