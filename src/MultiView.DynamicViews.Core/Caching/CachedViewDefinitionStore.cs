using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Core.Caching;

public sealed class CachedViewDefinitionStore : IViewDefinitionStore
{
    private const int DefaultCacheMinutes = 10;

    private readonly ISerializedViewDefinitionStore _serializedStore;
    private readonly IViewDefinitionSerializer _serializer;
    private readonly IMemoryCache _cache;

    public CachedViewDefinitionStore(
        ISerializedViewDefinitionStore serializedStore,
        IViewDefinitionSerializer serializer,
        IMemoryCache cache)
    {
        _serializedStore = serializedStore;
        _serializer = serializer;
        _cache = cache;
    }

    public async Task<DynamicViewDefinition> GetAsync(
        string viewId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(viewId))
        {
            throw new ArgumentException("viewId cannot be empty.", nameof(viewId));
        }

        if (_cache.TryGetValue(BuildCacheKey(viewId), out DynamicViewDefinition? cached))
        {
            return cached!;
        }

        DynamicViewDefinition resolved = await ResolveAsync(
                viewId,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                cancellationToken)
            .ConfigureAwait(false);

        _cache.Set(
            BuildCacheKey(viewId),
            resolved,
            TimeSpan.FromMinutes(DefaultCacheMinutes));

        return resolved;
    }

    private async Task<DynamicViewDefinition> ResolveAsync(
        string viewId,
        HashSet<string> resolving,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(BuildCacheKey($"resolved:{viewId}"), out DynamicViewDefinition? cachedResolved))
        {
            return cachedResolved!;
        }

        if (!resolving.Add(viewId))
        {
            throw new InvalidOperationException($"Cycle de résolution détecté pour la vue '{viewId}'.");
        }

        try
        {
            string rawDefinition = await _serializedStore.GetRawAsync(viewId, cancellationToken)
                .ConfigureAwait(false);

            DynamicViewDefinition definition = _serializer.Deserialize(rawDefinition);

            if (!string.Equals(definition.Id, viewId, StringComparison.OrdinalIgnoreCase))
            {
                definition = ReplaceDefinitionId(definition, viewId);
            }

            DynamicViewDefinition resolved = definition;

            if (!string.IsNullOrWhiteSpace(definition.Extends))
            {
                resolved = await ResolveInheritanceAsync(resolved, resolving, cancellationToken).ConfigureAwait(false);
            }

            resolved = await ResolveCompositionIncludesAsync(resolved, resolving, cancellationToken).ConfigureAwait(false);
            resolved = ApplyCompositionRemovals(resolved);

            _cache.Set(
                BuildCacheKey($"resolved:{viewId}"),
                resolved,
                TimeSpan.FromMinutes(DefaultCacheMinutes));

            return resolved;
        }
        finally
        {
            resolving.Remove(viewId);
        }
    }

    private async Task<DynamicViewDefinition> ResolveInheritanceAsync(
        DynamicViewDefinition child,
        HashSet<string> resolving,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(child.Extends))
        {
            return child;
        }

        DynamicViewDefinition baseDefinition = await ResolveAsync(
                child.Extends!,
                resolving,
                cancellationToken)
            .ConfigureAwait(false);

        return MergeDefinitions(baseDefinition, child);
    }

    private async Task<DynamicViewDefinition> ResolveCompositionIncludesAsync(
        DynamicViewDefinition source,
        HashSet<string> resolving,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> includes = source.Composition?.Includes ?? [];
        if (includes.Count == 0)
        {
            return source;
        }

        DynamicViewDefinition merged = source;

        foreach (string includeId in includes)
        {
            if (string.IsNullOrWhiteSpace(includeId))
            {
                continue;
            }

            DynamicViewDefinition includeDefinition = await ResolveAsync(includeId, resolving, cancellationToken)
                .ConfigureAwait(false);

            if (includeDefinition.Kind != merged.Kind)
            {
                throw new InvalidOperationException(
                    $"La composition de vue est invalide: '{includeId}' ({includeDefinition.Kind}) ne peut pas être inclus dans '{merged.Id}' ({merged.Kind}).");
            }

            merged = MergeDefinitions(merged, includeDefinition);
        }

        return merged;
    }

    private static DynamicViewDefinition MergeDefinitions(DynamicViewDefinition baseDefinition, DynamicViewDefinition overlay)
    {
        if (baseDefinition.Kind != overlay.Kind)
        {
            throw new InvalidOperationException("Impossible de combiner deux vues de types différents.");
        }

        return baseDefinition.Kind switch
        {
            DynamicViewKind.Form => MergeForm((FormViewDefinition)baseDefinition, (FormViewDefinition)overlay),
            DynamicViewKind.List => MergeList((ListViewDefinition)baseDefinition, (ListViewDefinition)overlay),
            DynamicViewKind.Kanban => MergeKanban((KanbanViewDefinition)baseDefinition, (KanbanViewDefinition)overlay),
            DynamicViewKind.Search => MergeSearch((SearchViewDefinition)baseDefinition, (SearchViewDefinition)overlay),
            DynamicViewKind.Graph => MergeGraph((GraphViewDefinition)baseDefinition, (GraphViewDefinition)overlay),
            DynamicViewKind.Pivot => MergePivot((PivotViewDefinition)baseDefinition, (PivotViewDefinition)overlay),
            DynamicViewKind.Calendar => MergeCalendar((CalendarViewDefinition)baseDefinition, (CalendarViewDefinition)overlay),
            DynamicViewKind.Gantt => MergeGantt((GanttViewDefinition)baseDefinition, (GanttViewDefinition)overlay),
            _ => throw new NotSupportedException($"Type de vue non supporté: {baseDefinition.Kind}")
        };
    }

    private static FormViewDefinition MergeForm(FormViewDefinition baseDefinition, FormViewDefinition overlay)
    {
        return new FormViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Sections = MergeByName(baseDefinition.Sections, overlay.Sections, section => section.Id),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules
        };
    }

    private static ListViewDefinition MergeList(ListViewDefinition baseDefinition, ListViewDefinition overlay)
    {
        return new ListViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Columns = MergeByName(baseDefinition.Columns, overlay.Columns, column => column.Field),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules,
            EnableSearch = overlay.EnableSearch,
            EnablePaging = overlay.EnablePaging,
            DefaultPageSize = overlay.DefaultPageSize ?? baseDefinition.DefaultPageSize
        };
    }

    private static KanbanViewDefinition MergeKanban(KanbanViewDefinition baseDefinition, KanbanViewDefinition overlay)
    {
        return new KanbanViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules,
            GroupByField = string.IsNullOrWhiteSpace(overlay.GroupByField) ? baseDefinition.GroupByField : overlay.GroupByField,
            Card = overlay.Card ?? baseDefinition.Card
        };
    }

    private static SearchViewDefinition MergeSearch(SearchViewDefinition baseDefinition, SearchViewDefinition overlay)
    {
        return new SearchViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules,
            SearchFields = MergeByName(baseDefinition.SearchFields, overlay.SearchFields, field => field),
            EnablePaging = overlay.EnablePaging,
            DefaultPageSize = overlay.DefaultPageSize ?? baseDefinition.DefaultPageSize
        };
    }

    private static GraphViewDefinition MergeGraph(GraphViewDefinition baseDefinition, GraphViewDefinition overlay)
    {
        return new GraphViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules,
            CategoryField = string.IsNullOrWhiteSpace(overlay.CategoryField) ? baseDefinition.CategoryField : overlay.CategoryField,
            ValueField = string.IsNullOrWhiteSpace(overlay.ValueField) ? baseDefinition.ValueField : overlay.ValueField,
            SeriesField = overlay.SeriesField ?? baseDefinition.SeriesField,
            Aggregation = overlay.Aggregation ?? baseDefinition.Aggregation,
            ChartType = overlay.ChartType ?? baseDefinition.ChartType,
            Limit = overlay.Limit ?? baseDefinition.Limit
        };
    }

    private static PivotViewDefinition MergePivot(PivotViewDefinition baseDefinition, PivotViewDefinition overlay)
    {
        return new PivotViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules,
            RowField = string.IsNullOrWhiteSpace(overlay.RowField) ? baseDefinition.RowField : overlay.RowField,
            ColumnField = string.IsNullOrWhiteSpace(overlay.ColumnField) ? baseDefinition.ColumnField : overlay.ColumnField,
            ValueField = string.IsNullOrWhiteSpace(overlay.ValueField) ? baseDefinition.ValueField : overlay.ValueField,
            Aggregation = overlay.Aggregation ?? baseDefinition.Aggregation,
            ValuePrecision = overlay.ValuePrecision ?? baseDefinition.ValuePrecision
        };
    }

    private static CalendarViewDefinition MergeCalendar(CalendarViewDefinition baseDefinition, CalendarViewDefinition overlay)
    {
        return new CalendarViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules,
            StartDateField = string.IsNullOrWhiteSpace(overlay.StartDateField) ? baseDefinition.StartDateField : overlay.StartDateField,
            EndDateField = overlay.EndDateField ?? baseDefinition.EndDateField,
            TitleField = overlay.TitleField ?? baseDefinition.TitleField,
            SubtitleField = overlay.SubtitleField ?? baseDefinition.SubtitleField,
            Bucket = overlay.Bucket ?? baseDefinition.Bucket,
            LimitPerBucket = overlay.LimitPerBucket ?? baseDefinition.LimitPerBucket
        };
    }

    private static GanttViewDefinition MergeGantt(GanttViewDefinition baseDefinition, GanttViewDefinition overlay)
    {
        return new GanttViewDefinition
        {
            Id = overlay.Id,
            Model = overlay.Model,
            Name = overlay.Name,
            Kind = overlay.Kind,
            Title = overlay.Title ?? baseDefinition.Title,
            Extends = overlay.Extends,
            Composition = MergeComposition(baseDefinition.Composition, overlay.Composition),
            Fields = MergeByName(baseDefinition.Fields, overlay.Fields, field => field.Name),
            Actions = MergeByName(baseDefinition.Actions, overlay.Actions, action => action.Name),
            Rules = overlay.Rules ?? baseDefinition.Rules,
            StartDateField = string.IsNullOrWhiteSpace(overlay.StartDateField) ? baseDefinition.StartDateField : overlay.StartDateField,
            EndDateField = string.IsNullOrWhiteSpace(overlay.EndDateField) ? baseDefinition.EndDateField : overlay.EndDateField,
            LabelField = string.IsNullOrWhiteSpace(overlay.LabelField) ? baseDefinition.LabelField : overlay.LabelField,
            GroupByField = overlay.GroupByField ?? baseDefinition.GroupByField,
            ProgressField = overlay.ProgressField ?? baseDefinition.ProgressField,
            Limit = overlay.Limit ?? baseDefinition.Limit
        };
    }

    private static DynamicViewDefinition ApplyCompositionRemovals(DynamicViewDefinition definition)
    {
        ViewCompositionDefinition? composition = definition.Composition;
        if (composition is null)
        {
            return definition;
        }

        IReadOnlyList<ViewFieldDefinition> fields = ApplyRemoval(
            definition.Fields,
            field => field.Name,
            composition.RemoveFields);

        IReadOnlyList<ViewActionDefinition> actions = ApplyRemoval(
            definition.Actions,
            action => action.Name,
            composition.RemoveActions);

        if (definition is ListViewDefinition listDefinition)
        {
            return new ListViewDefinition
            {
                Id = listDefinition.Id,
                Model = listDefinition.Model,
                Name = listDefinition.Name,
                Kind = listDefinition.Kind,
                Title = listDefinition.Title,
                Extends = listDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = listDefinition.Rules,
                Columns = ApplyRemoval(
                    listDefinition.Columns,
                    column => column.Field,
                    composition.RemoveColumns),
                EnableSearch = listDefinition.EnableSearch,
                EnablePaging = listDefinition.EnablePaging,
                DefaultPageSize = listDefinition.DefaultPageSize
            };
        }

        if (definition is FormViewDefinition formDefinition)
        {
            return new FormViewDefinition
            {
                Id = formDefinition.Id,
                Model = formDefinition.Model,
                Name = formDefinition.Name,
                Kind = formDefinition.Kind,
                Title = formDefinition.Title,
                Extends = formDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = formDefinition.Rules,
                Sections = ApplyRemoval(
                    formDefinition.Sections,
                    section => section.Id,
                    composition.RemoveSections)
            };
        }

        if (definition is KanbanViewDefinition kanbanDefinition)
        {
            return new KanbanViewDefinition
            {
                Id = kanbanDefinition.Id,
                Model = kanbanDefinition.Model,
                Name = kanbanDefinition.Name,
                Kind = kanbanDefinition.Kind,
                Title = kanbanDefinition.Title,
                Extends = kanbanDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = kanbanDefinition.Rules,
                GroupByField = kanbanDefinition.GroupByField,
                Card = kanbanDefinition.Card
            };
        }

        if (definition is SearchViewDefinition searchDefinition)
        {
            return new SearchViewDefinition
            {
                Id = searchDefinition.Id,
                Model = searchDefinition.Model,
                Name = searchDefinition.Name,
                Kind = searchDefinition.Kind,
                Title = searchDefinition.Title,
                Extends = searchDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = searchDefinition.Rules,
                SearchFields = searchDefinition.SearchFields,
                EnablePaging = searchDefinition.EnablePaging,
                DefaultPageSize = searchDefinition.DefaultPageSize
            };
        }

        if (definition is GraphViewDefinition graphDefinition)
        {
            return new GraphViewDefinition
            {
                Id = graphDefinition.Id,
                Model = graphDefinition.Model,
                Name = graphDefinition.Name,
                Kind = graphDefinition.Kind,
                Title = graphDefinition.Title,
                Extends = graphDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = graphDefinition.Rules,
                CategoryField = graphDefinition.CategoryField,
                ValueField = graphDefinition.ValueField,
                SeriesField = graphDefinition.SeriesField,
                Aggregation = graphDefinition.Aggregation,
                ChartType = graphDefinition.ChartType,
                Limit = graphDefinition.Limit
            };
        }

        if (definition is PivotViewDefinition pivotDefinition)
        {
            return new PivotViewDefinition
            {
                Id = pivotDefinition.Id,
                Model = pivotDefinition.Model,
                Name = pivotDefinition.Name,
                Kind = pivotDefinition.Kind,
                Title = pivotDefinition.Title,
                Extends = pivotDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = pivotDefinition.Rules,
                RowField = pivotDefinition.RowField,
                ColumnField = pivotDefinition.ColumnField,
                ValueField = pivotDefinition.ValueField,
                Aggregation = pivotDefinition.Aggregation,
                ValuePrecision = pivotDefinition.ValuePrecision
            };
        }

        if (definition is CalendarViewDefinition calendarDefinition)
        {
            return new CalendarViewDefinition
            {
                Id = calendarDefinition.Id,
                Model = calendarDefinition.Model,
                Name = calendarDefinition.Name,
                Kind = calendarDefinition.Kind,
                Title = calendarDefinition.Title,
                Extends = calendarDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = calendarDefinition.Rules,
                StartDateField = calendarDefinition.StartDateField,
                EndDateField = calendarDefinition.EndDateField,
                TitleField = calendarDefinition.TitleField,
                SubtitleField = calendarDefinition.SubtitleField,
                Bucket = calendarDefinition.Bucket,
                LimitPerBucket = calendarDefinition.LimitPerBucket
            };
        }

        if (definition is GanttViewDefinition ganttDefinition)
        {
            return new GanttViewDefinition
            {
                Id = ganttDefinition.Id,
                Model = ganttDefinition.Model,
                Name = ganttDefinition.Name,
                Kind = ganttDefinition.Kind,
                Title = ganttDefinition.Title,
                Extends = ganttDefinition.Extends,
                Composition = new ViewCompositionDefinition
                {
                    Includes = [],
                    RemoveFields = composition.RemoveFields,
                    RemoveSections = composition.RemoveSections,
                    RemoveColumns = composition.RemoveColumns,
                    RemoveActions = composition.RemoveActions
                },
                Fields = fields,
                Actions = actions,
                Rules = ganttDefinition.Rules,
                StartDateField = ganttDefinition.StartDateField,
                EndDateField = ganttDefinition.EndDateField,
                LabelField = ganttDefinition.LabelField,
                GroupByField = ganttDefinition.GroupByField,
                ProgressField = ganttDefinition.ProgressField,
                Limit = ganttDefinition.Limit
            };
        }

        return definition;
    }

    private static List<T> MergeByName<T>(
        IReadOnlyList<T> baseItems,
        IReadOnlyList<T> overlayItems,
        Func<T, string> keySelector)
    {
        StringComparer keyComparer = StringComparer.OrdinalIgnoreCase;
        List<T> merged = [.. baseItems];
        Dictionary<string, int> itemIndexByKey = baseItems
            .Select((item, index) => new { Key = keySelector(item), Index = index })
            .ToDictionary(item => item.Key, item => item.Index, keyComparer);

        foreach (T item in overlayItems)
        {
            string key = keySelector(item);
            if (itemIndexByKey.TryGetValue(key, out int existingIndex))
            {
                merged[existingIndex] = item;
                continue;
            }

            itemIndexByKey[key] = merged.Count;
            merged.Add(item);
        }

        return merged;
    }

    private static IReadOnlyList<T> ApplyRemoval<T>(
        IReadOnlyList<T> items,
        Func<T, string> keySelector,
        IReadOnlyList<string> removeKeys)
    {
        if (removeKeys.Count == 0)
        {
            return items;
        }

        HashSet<string> toRemove = new(removeKeys, StringComparer.OrdinalIgnoreCase);
        return [.. items.Where(item => !toRemove.Contains(keySelector(item)))];
    }

    private static ViewCompositionDefinition? MergeComposition(
        ViewCompositionDefinition? baseComposition,
        ViewCompositionDefinition? overlayComposition)
    {
        if (baseComposition is null)
        {
            return overlayComposition;
        }

        if (overlayComposition is null)
        {
            return baseComposition;
        }

        return new ViewCompositionDefinition
        {
            Includes = baseComposition.Includes.Concat(overlayComposition.Includes).ToList(),
            RemoveFields = baseComposition.RemoveFields.Concat(overlayComposition.RemoveFields).ToList(),
            RemoveSections = baseComposition.RemoveSections.Concat(overlayComposition.RemoveSections).ToList(),
            RemoveColumns = baseComposition.RemoveColumns.Concat(overlayComposition.RemoveColumns).ToList(),
            RemoveActions = baseComposition.RemoveActions.Concat(overlayComposition.RemoveActions).ToList()
        };
    }

    private static DynamicViewDefinition ReplaceDefinitionId(DynamicViewDefinition definition, string viewId)
    {
        return definition.Kind switch
        {
            DynamicViewKind.Form => new FormViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                Sections = ((FormViewDefinition)definition).Sections
            },
            DynamicViewKind.List => new ListViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                Columns = ((ListViewDefinition)definition).Columns,
                EnableSearch = ((ListViewDefinition)definition).EnableSearch,
                EnablePaging = ((ListViewDefinition)definition).EnablePaging,
                DefaultPageSize = ((ListViewDefinition)definition).DefaultPageSize
            },
            DynamicViewKind.Kanban => new KanbanViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                GroupByField = ((KanbanViewDefinition)definition).GroupByField,
                Card = ((KanbanViewDefinition)definition).Card
            },
            DynamicViewKind.Search => new SearchViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                SearchFields = ((SearchViewDefinition)definition).SearchFields,
                EnablePaging = ((SearchViewDefinition)definition).EnablePaging,
                DefaultPageSize = ((SearchViewDefinition)definition).DefaultPageSize
            },
            DynamicViewKind.Graph => new GraphViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                CategoryField = ((GraphViewDefinition)definition).CategoryField,
                ValueField = ((GraphViewDefinition)definition).ValueField,
                SeriesField = ((GraphViewDefinition)definition).SeriesField,
                Aggregation = ((GraphViewDefinition)definition).Aggregation,
                ChartType = ((GraphViewDefinition)definition).ChartType,
                Limit = ((GraphViewDefinition)definition).Limit
            },
            DynamicViewKind.Pivot => new PivotViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                RowField = ((PivotViewDefinition)definition).RowField,
                ColumnField = ((PivotViewDefinition)definition).ColumnField,
                ValueField = ((PivotViewDefinition)definition).ValueField,
                Aggregation = ((PivotViewDefinition)definition).Aggregation,
                ValuePrecision = ((PivotViewDefinition)definition).ValuePrecision
            },
            DynamicViewKind.Calendar => new CalendarViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                StartDateField = ((CalendarViewDefinition)definition).StartDateField,
                EndDateField = ((CalendarViewDefinition)definition).EndDateField,
                TitleField = ((CalendarViewDefinition)definition).TitleField,
                SubtitleField = ((CalendarViewDefinition)definition).SubtitleField,
                Bucket = ((CalendarViewDefinition)definition).Bucket,
                LimitPerBucket = ((CalendarViewDefinition)definition).LimitPerBucket
            },
            DynamicViewKind.Gantt => new GanttViewDefinition
            {
                Id = viewId,
                Model = definition.Model,
                Name = definition.Name,
                Kind = definition.Kind,
                Title = definition.Title,
                Extends = definition.Extends,
                Composition = definition.Composition,
                Fields = definition.Fields,
                Actions = definition.Actions,
                Rules = definition.Rules,
                StartDateField = ((GanttViewDefinition)definition).StartDateField,
                EndDateField = ((GanttViewDefinition)definition).EndDateField,
                LabelField = ((GanttViewDefinition)definition).LabelField,
                GroupByField = ((GanttViewDefinition)definition).GroupByField,
                ProgressField = ((GanttViewDefinition)definition).ProgressField,
                Limit = ((GanttViewDefinition)definition).Limit
            },
            _ => throw new NotSupportedException($"Type de vue non supporté: {definition.Kind}")
        };
    }

    private static string BuildCacheKey(string viewId) => $"dynamic-view-definition:{viewId}";
}
