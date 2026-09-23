---
name: register_and_link_asset
description: Hướng dẫn cách tạo, đăng ký một Asset Slot mới và cách liên kết file sau khi upload bằng IAssetApi
---

# Hướng dẫn Đăng ký và Sử dụng File (Asset) trong các Module

Module `Files` đóng vai trò quản lý tập trung toàn bộ file upload trong hệ thống. Khi một Module khác (như `Identity`, `Catalog`, v.v.) cần lưu trữ file (ví dụ: Avatar, Product Image), nó phải tự đăng ký các **Asset Slot** của riêng mình và gọi API để liên kết file.

Khi có yêu cầu thêm chức năng đính kèm file/hình ảnh vào một Entity, hãy thực hiện theo 3 bước chuẩn sau:

## 1. Tạo File Constants lưu Slot Key
- Tạo file `[TênModule]AssetSlots.cs` trong thư mục `Constants/` của module.
- Khai báo các hằng số `public const string` để tránh hardcode.

```csharp
namespace Automation.Identity.Constants;

public static class IdentityAssetSlots
{
    public const string Avatar = "Avatar";
}
```

## 2. Tạo Extension Method để đăng ký Slot
- Tạo file `[TênModule]AssetExtensions.cs` trong thư mục `Extensions/`.
- Sử dụng hàm `AddAssetSlot` từ `Automation.Files.Contracts` để đăng ký cấu hình dung lượng, loại file cho phép.

```csharp
using Automation.Files.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Automation.Identity.Constants;

namespace Automation.Identity.Extensions;

public static class IdentityAssetExtensions
{
    public static IServiceCollection AddIdentityAssetSlots(this IServiceCollection services)
    {
        services.AddAssetSlot(
            entityType: "User", // Tên Entity sở hữu
            slotKey: IdentityAssetSlots.Avatar,
            options: new AssetCategoryOptions
            {
                AllowMultiple = false,
                MaxCount = 1,
                MaxSizeBytes = 5 * 1024 * 1024, // 5MB
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"]
            }
        );
        return services;
    }
}
```

- Sau đó, nhớ gọi `services.Add[TênModule]AssetSlots();` bên trong hàm `ConfigureServices` của file `[TênModule]Module.cs`.

## 3. Gọi IAssetApi.VerifyAndLinkAsync trong Handler
- Frontend sẽ upload file thông qua endpoint của Module `Files` và nhận về `AssetId`.
- Trong tính năng (Feature Handler) của Module sở hữu (ví dụ: UpdateUserHandler), nhận `AssetId` này và tiến hành xác nhận (Verify & Link).
- Phải inject `IAssetApi` (từ `Automation.Files.Contracts`) vào Handler.

```csharp
// Trong Handler:
if (!string.IsNullOrEmpty(command.AvatarAssetId) && Guid.TryParse(command.AvatarAssetId, out var avatarAssetId))
{
    var linkResult = await assetApi.VerifyAndLinkAsync(
        assetId: avatarAssetId,
        ownerEntityType: "User", // Phải khớp với lúc đăng ký
        slotKey: IdentityAssetSlots.Avatar,
        ownerEntityId: user.Id.ToString(),
        ct: ct);
        
    if (linkResult.IsFailed)
        return Result.Fail($"Failed to link avatar: {linkResult.Errors.FirstOrDefault()?.Message}");
}
```

**LƯU Ý:**
- KHÔNG hardcode chuỗi định dạng (như `"image/jpeg"` hoặc Slot Key) rải rác.
- Module gọi liên kết (`VerifyAndLinkAsync`) không cần phải quan tâm file đó được lưu trữ thực sự ở S3 hay đĩa cứng. Mọi thứ do module `Files` lo liệu.


