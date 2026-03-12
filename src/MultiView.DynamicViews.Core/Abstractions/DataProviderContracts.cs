using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Core.Abstractions;

public interface IDataProvider<TModel>
    where TModel : class
{
    ValueTask<IReadOnlyList<TModel>> ListAsync(CancellationToken cancellationToken = default);

    ValueTask<TModel?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    ValueTask SaveAsync(TModel model, CancellationToken cancellationToken = default);

    ValueTask DeleteAsync(TModel model, CancellationToken cancellationToken = default);
}
