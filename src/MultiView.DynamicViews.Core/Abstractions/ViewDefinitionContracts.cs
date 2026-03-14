using MultiView.DynamicViews.Domain.Model;
using MultiView.DynamicViews.Core.Validation;

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

public interface IViewDefinitionValidator
{
    ViewDefinitionValidationResult Validate(string json);
}
