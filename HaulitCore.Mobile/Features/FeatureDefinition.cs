namespace HaulitCore.Mobile.Features;

public sealed class FeatureDefinition
{
    public AppFeature Feature { get; init; }
    public AppFeature? Parent { get; init; }
    public IReadOnlyList<AppFeature> DependsOn { get; init; } = Array.Empty<AppFeature>();
}