using MultiView.DynamicViews.Core.Abstractions;

namespace MultiView.DynamicViews.Core.Services;

public sealed class InMemorySerializedViewDefinitionSource : ISerializedViewDefinitionStore
{
    private readonly IDictionary<string, string> _definitions;

    public InMemorySerializedViewDefinitionSource(IDictionary<string, string> definitions)
    {
        _definitions = definitions;
    }

    public Task<string> GetRawAsync(string viewId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_definitions.TryGetValue(viewId, out string? definitionJson))
        {
            throw new KeyNotFoundException($"Aucune définition JSON trouvée pour '{viewId}'.");
        }

        return Task.FromResult(definitionJson);
    }
}
