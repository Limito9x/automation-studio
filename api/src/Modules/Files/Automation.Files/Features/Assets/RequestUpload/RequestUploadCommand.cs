using Automation.Files.Contracts;

namespace Automation.Files.Features.Assets.RequestUpload;

public record RequestUploadCommand(List<UploadRequestItemDto> Items);



