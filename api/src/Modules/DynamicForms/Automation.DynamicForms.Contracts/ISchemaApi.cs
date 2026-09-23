using System.Text.Json;
using FluentResults;

namespace Automation.DynamicForms.Contracts;

public interface ISchemaApi
{
    // Lấy active schema version để Frontend render Builder/Renderer
    Task<Result<SchemaVersionDto>> GetActiveVersionAsync(string ownerType, string ownerId, CancellationToken ct = default);

    // Validate + Lưu data của một entity
    Task<Result<SchemaDataDto>> SaveDataAsync(string ownerType, string ownerId, string clientId, string clientType, JsonDocument values, CancellationToken ct = default);

    // Lấy data đã lưu theo clientId (để hiện thị lại trên UI Edit)
    Task<Result<SchemaDataDto>> GetDataAsync(string clientId, string clientType, CancellationToken ct = default);

    // Lấy danh sách data đã lưu theo danh sách clientId (để fetch bulk tránh N+1)
    Task<Result<IEnumerable<SchemaDataDto>>> GetMultipleDataAsync(IEnumerable<string> clientIds, string clientType, CancellationToken ct = default);

    // Tạo hoặc cập nhật schema (khi Admin sửa Form Builder hoặc tạo Struct)
    Task<Result> UpsertSchemaAsync(string ownerType, string ownerId, string schemaName, JsonDocument fields, CancellationToken ct = default);

    // --- CÁC METHODS MỚI CHO PHASE 3 ---
    // Single-batch loading theo SchemaDefinitionId kèm toàn bộ dependencies lồng nhau
    Task<Result<SchemaWithDependenciesDto>> GetActiveVersionWithDependenciesAsync(Guid schemaDefinitionId, CancellationToken ct = default);

    // Single-batch loading theo OwnerType & OwnerId (cho ContentType, ContentItem)
    Task<Result<SchemaWithDependenciesDto>> GetActiveVersionWithDependenciesAsync(string ownerType, string ownerId, CancellationToken ct = default);

    // Kiểm tra an toàn trước khi xóa (Referential Integrity)
    Task<Result<bool>> CanDeleteSchemaAsync(Guid schemaDefinitionId, CancellationToken ct = default);

    // Lấy danh sách chi tiết các schema đang phụ thuộc vào struct này (phục vụ UX báo lỗi)
    Task<Result<IReadOnlyList<SchemaReferencingUsageDto>>> GetReferencingSchemasAsync(Guid schemaDefinitionId, CancellationToken ct = default);

    // Lấy toàn bộ schemas theo OwnerType & OwnerId (ví dụ: lấy tất cả Project Structs của 1 Project)
    Task<Result<IReadOnlyList<SchemaWithDependenciesDto>>> GetSchemasByOwnerAsync(string ownerType, string ownerId, CancellationToken ct = default);
}
