using Automation.Pipeline.Engine.Models;

namespace Automation.Pipeline.Engine.Models;

public class UnresolvedPinsError : Error
{
    public IReadOnlyList<UnresolvedPin> UnresolvedPins { get; }

    public UnresolvedPinsError(IReadOnlyList<UnresolvedPin> unresolvedPins)
        : base($"Pipeline has {unresolvedPins.Count} unresolved required pin(s).")
    {
        UnresolvedPins = unresolvedPins;
        Metadata.Add("UnresolvedPins", unresolvedPins);
    }
}
