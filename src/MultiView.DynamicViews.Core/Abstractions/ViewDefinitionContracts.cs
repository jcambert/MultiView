using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Core.Abstractions;

public interface IViewDefinitionStore
{
    Task<DynamicViewDefinition> GetAsync(string viewId, CancellationToken cancellationToken = default);
}

public interface ISerializedViewDefinitionStore
{
    Task<string> GetRawAsync(string viewId, CancellationToken cancellationToken = default);
}

public interface IViewDefinitionSerializer
{
    DynamicViewDefinition Deserialize(string json);

    string Serialize(DynamicViewDefinition definition);
}
