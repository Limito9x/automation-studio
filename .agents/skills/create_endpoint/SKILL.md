---
name: create_endpoint
description: Hướng dẫn cách tạo một API Endpoint bằng FastEndpoints theo chuẩn của dự án.
---

# Hướng Dẫn Tạo Endpoint

Dự án sử dụng **FastEndpoints** thay cho Controller truyền thống. Mỗi endpoint thường nằm trong thư mục tính năng (Slice) của nó.

### Cấu trúc cơ bản
Một Endpoint kế thừa từ `Endpoint<TRequest, TResponse>` (nếu có trả về và nhận tham số) hoặc `Endpoint<TRequest>` (không có response type) hoặc `EndpointWithoutRequest`.

```csharp
public class GetUserByIdEndpoint : Endpoint<GetUserByIdQuery, Result<UserDto>>
{
    public override void Configure()
    {
        Get("/users/{id:guid}");
        Group<UsersGroup>(); // QUAN TRỌNG: Gắn Endpoint này vào một Group cụ thể của Module
        AllowAnonymous(); // Hoặc Policies("..."), Roles("...")
        Description(b => b
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(GetUserByIdQuery req, CancellationToken ct)
    {
        // Gửi Query/Command vào Message Bus (thường là Wolverine)
        var result = await bus.InvokeAsync<Result<UserDto>>(req, ct);
        
        // Trả về kết quả HTTP chuẩn
        await SendResultAsync(result.ToEndpointResult());
    }
}
```

### Lưu ý quan trọng
1. **Endpoint Group**: LUÔN thiết lập `Group<TGroup>()` trong hàm `Configure()` để nhóm các API cùng tính năng (vd: `Group<UsersGroup>()`). File Group này sẽ cấu hình chung `Prefix` hoặc `WithTags` cho Swagger.
2. **Result Pattern**: Sử dụng `FluentResults.Result` làm kiểu trả về, và dùng extension `ToEndpointResult()` để chuyển đổi Result thành HTTP Response thích hợp (vd: trả về lỗi 404, 400 hoặc 200 OK).
3. **Phân quyền**: Cấu hình `Permissions(...)` nếu endpoint yêu cầu quyền cụ thể, hoặc `AllowAnonymous()` nếu là API công khai.
4. **Khởi tạo DI**: Sử dụng primary constructor cho đơn giản và tinh gọn thay vì khởi tạo readonly và qua hàm constructor
