namespace Automation.SharedKernel.Domain.Interfaces;

public interface IAuditTrackable
{
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditIgnoreAttribute : Attribute;


