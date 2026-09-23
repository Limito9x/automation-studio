---
name: create_pipeline_tool
description: Hướng dẫn tiêu chuẩn để tạo một Pipeline Tool C# trong Backend sử dụng BaseResolverTool và ToolPinAttribute theo triết lý Base-First.
---

# Hướng Dẫn Tạo Pipeline Tool (Base-First Pattern)

Dự án áp dụng triết lý **Base-First (Archetype / Template-First)** cho toàn bộ hệ thống công cụ Pipeline (.NET). Mọi Tool tính toán hoặc điều phối trong C# BẮT BUỘC kế thừa từ `BaseResolverTool<TInput, TOutput>` để đảm bảo Type-Safety, tự động hóa Model Binding và loại bỏ 100% code bóc tách Dictionary thủ công.

---

## 1. Cấu Trúc Cơ Bản của Một Tool

Một Tool hoàn chỉnh bao gồm 3 thành phần (thường đặt chung 1 file hoặc cùng thư mục feature):
1. **`*Inputs` class**: Định nghĩa các chân cắm đầu vào (Input Pins).
2. **`*Outputs` class**: Định nghĩa các chân cắm đầu ra (Output Pins).
3. **`*Tool` class**: Kế thừa `BaseResolverTool<TInput, TOutput>` và thực thi logic tại `ExecuteCoreAsync`.

```csharp
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Tools;
using Automation.Pipeline.Tools.Attributes;

namespace Automation.Pipeline.Tools.Utility;

public class CombinePathInputs
{
    [ToolPin(Id = "BasePath", Label = "Base Path", PrimitiveType = PinPrimitiveType.Path, IsRequired = true)]
    public string BasePath { get; set; } = string.Empty;

    [ToolPin(Id = "SubFolder", Label = "Sub Folder", PrimitiveType = PinPrimitiveType.String, IsRequired = false, DefaultValue = "")]
    public string SubFolder { get; set; } = string.Empty;

    [ToolPin(Id = "FileName", Label = "File Name", PrimitiveType = PinPrimitiveType.String, IsRequired = false, DefaultValue = "")]
    public string FileName { get; set; } = string.Empty;

    [ToolPin(Id = "Extension", Label = "Extension", PrimitiveType = PinPrimitiveType.String, IsRequired = false, DefaultValue = "")]
    public string Extension { get; set; } = string.Empty;
}

public class CombinePathOutputs
{
    [ToolPin(Id = "FullPath", Label = "Full Path", PrimitiveType = PinPrimitiveType.Path, IsRequired = true)]
    public string FullPath { get; set; } = string.Empty;
}

/// <summary>
/// Tool ghép các thành phần đường dẫn file thành một đường dẫn hoàn chỉnh.
/// </summary>
public class CombinePathTool : BaseResolverTool<CombinePathInputs, CombinePathOutputs>
{
    public override string Key => "CombinePath";
    public override string Label => "Combine Path";
    public override string? Category => "Utility";
    public override bool IsPure => true; // true nếu không tạo side-effect (Pure Operator)

    protected override Task<CombinePathOutputs> ExecuteCoreAsync(CombinePathInputs input, ToolExecutionContext context)
    {
        var fullFileName = input.FileName;
        if (!string.IsNullOrEmpty(input.Extension))
        {
            var ext = input.Extension.StartsWith(".") ? input.Extension : "." + input.Extension;
            if (!fullFileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                fullFileName += ext;
            }
        }

        var combined = input.BasePath;
        if (!string.IsNullOrEmpty(input.SubFolder))
        {
            combined = Path.Combine(combined, input.SubFolder);
        }
        if (!string.IsNullOrEmpty(fullFileName))
        {
            combined = Path.Combine(combined, fullFileName);
        }

        return Task.FromResult(new CombinePathOutputs
        {
            FullPath = combined.Replace('\\', '/')
        });
    }
}
```

---

## 2. Quy Chuẩn Khai Báo Chân Cắm (`ToolPinAttribute`)

Sử dụng `[ToolPin]` trên Property để cấu hình chi tiết chân cắm trên Canvas:
- `Id`: Tên định danh của chân (ví dụ: `"TargetMap"`, `"Key"`).
- `Label`: Nhãn hiển thị cho User trên Canvas (ví dụ: `"Target Map"`).
- `Kind`: `PinKind.Data` (mặc định) hoặc `PinKind.Exec`.
- `PrimitiveType`: Kiểu dữ liệu chân cắm (`PinPrimitiveType.String`, `Number`, `Boolean`, `Path`, `EntityRef`, `Asset`).
- `Cardinality`: Cấu trúc dữ liệu (`PinCardinality.Single`, `Array`, `Map`).
- `IsRequired`: `true` nếu chân cắm bắt buộc phải có giá trị.
- `DefaultValue`: Giá trị mặc định khi chưa cắm dây.
- `[ToolPinIgnore]`: Đặt lên property nội bộ để KHÔNG sinh chân cắm trên Canvas.

> **Tự Động Suy Luận (Inference):** Nếu không khai báo `PrimitiveType` hay `Cardinality`, `ToolModelBinder` sẽ tự động suy luận:
> - `string` $\rightarrow$ `String`, `Single`
> - `int`, `long`, `double` $\rightarrow$ `Number`, `Single`
> - `bool` $\rightarrow$ `Boolean`, `Single`
> - `Guid` $\rightarrow$ `EntityRef`, `Single`
> - `List<T>` / `T[]` $\rightarrow$ `Array`
> - `Dictionary<string, T>` $\rightarrow$ `Map`

---

## 3. Quy Tắc Bất Di Bất Dịch (Golden Rules)

1. **TUYỆT ĐỐI KHÔNG bóc tách thô `Dictionary<string, object>`:**
   - Không viết `inputs.GetValueOrDefault("Key")?.ToString() ?? inputs.GetValueOrDefault("key")`.
   - `ToolModelBinder` đã tự động xử lý case-insensitive, type conversion (JSON, Guid, Primitives, Collections) và null handling.
2. **Phân biệt rạch ròi Pure Operator vs Stateful Action:**
   - Nếu Tool chỉ tính toán dữ liệu (như gộp chuỗi, map key, format text): để `IsPure => true`. Tuyệt đối không đọc/ghi biến toàn cục, không hack `scope: null`.
   - Chỉ `SetVariableTool` và `GetVariableTool` mới chịu trách nhiệm lưu/đọc biến trạng thái.
3. **Bảo toàn Tương thích ngược qua `Aliases`:**
   - Nếu đổi tên Tool hoặc gộp nhiều tool thành một, BẮT BUỘC khai báo `public override IReadOnlyList<string> Aliases => ["TenToolCu"];` để các pipeline cũ đã lưu trong DB không bị gãy kết nối.
4. **Tự động đăng ký DI (Zero Ceremony):**
   - Mọi class kế thừa `BaseResolverTool` (hoặc `IResolverTool`) trong assembly `Automation.Pipeline` đều được tự động scan và đăng ký Scoped vào DI bởi `services.AddPipelineTools()`. Không cần đăng ký tay trong `Program.cs` hay `PipelineModule.cs`.
5. **Bắt buộc viết Unit Test:**
   - Thêm bài test tương ứng trong `tests/Automation.Pipeline.Tests/CollectionToolsTests.cs` (hoặc file test chuyên biệt) để đảm bảo tool chạy đúng cả logic lẫn binding.
