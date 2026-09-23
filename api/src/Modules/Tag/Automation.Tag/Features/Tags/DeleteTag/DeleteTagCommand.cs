namespace Automation.Tag.Features.Tags.DeleteTag;

public record DeleteTagCommand(Guid Id, bool DeleteChildren = true);