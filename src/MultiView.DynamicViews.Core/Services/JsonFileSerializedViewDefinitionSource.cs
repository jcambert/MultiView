using MultiView.DynamicViews.Core.Abstractions;

namespace MultiView.DynamicViews.Core.Services;

public sealed class JsonFileSerializedViewDefinitionSource : ISerializedViewDefinitionStore
{
    private readonly string _folderPath;

    public JsonFileSerializedViewDefinitionSource(string folderPath)
    {
        _folderPath = folderPath;
    }

    public async Task<string> GetRawAsync(string viewId, CancellationToken cancellationToken = default)
    {
        string filePath = Path.Combine(_folderPath, $"{viewId}.json");
        if (!File.Exists(filePath))
        {
            throw new KeyNotFoundException($"Le fichier de définition '{filePath}' est introuvable.");
        }

        return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
    }
}
