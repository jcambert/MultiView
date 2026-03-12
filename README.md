# MultiView Dynamic Views

Ce projet propose un framework de vues dynamiques orienté Blazor/MudBlazor en .NET 10, basé sur des métadonnées sérialisables (JSON).

## Architecture

- `MultiView.DynamicViews.Domain` : purement métier (modèles de définition de vues, enums, contrats de base).
- `MultiView.DynamicViews.Core` : cœur applicatif indépendant de UI (sérialisation, accès aux données, moteur de règles, résolution des widgets, dispatch actions, caches).
- `MultiView.DynamicViews.Blazor` : couche de présentation (components de rendu Form/List/Kanban, widgets MudBlazor, règles d'intégration UI).
- `MultiView.DynamicViews.Sample` : démonstration complète avec modèle `SaleOrder`.

## Objectifs couverts

- Vues dynamiques : `Form`, `List`, `Kanban` (extensions prévues pour `Search`, `Graph`, `Pivot`, `Calendar`, `Gantt`).
- Modèle de définition de vue sérialisable (JSON) chargé depuis `ISerializedViewDefinitionStore`.
- Moteur de règles conditionnelles (visible / readonly / required / enabled).
- Registre de widgets de champs enregistrable et extensible.
- Provider de données via abstraction `IDataProvider<TModel>` (découplé du rendu).
- Dispatcher d’actions via abstraction `IViewActionDispatcher` + handlers.
- Intégration MudBlazor.
- Cache définitions, cache de widgets résolus, cache des règles compilées.

## Arborescence

```text
src
  MultiView.DynamicViews.Domain
  MultiView.DynamicViews.Core
  MultiView.DynamicViews.Blazor
  MultiView.DynamicViews.Sample
    Definitions
```

## Démarrage

- Prérequis : .NET 10 SDK + MudBlazor 9.
- Restauration/build :
  - `dotnet build`
- Lancement sample :
  - `dotnet run --project src/MultiView.DynamicViews.Sample`

## Flux d’exécution

1. Les JSON de vue sont chargés par le store sérialisé.
2. Le store met en cache la définition désérialisée.
3. `DynamicViewRenderer` résout la vue à afficher.
4. Chaque vue récupère champs et règles.
5. `DynamicFieldRenderer` évalue les règles via `IRuleEvaluator`.
6. Le widget est résolu via `IFieldWidgetRegistry` (avec cache interne) et rendu.
7. Les actions sont dispatchées par `IViewActionDispatcher`.

## Tests

- Aucun test automatisé n’est fourni dans ce socle initial.
- Point d’entrée recommandé : ajouter des tests unitaires sur `DefaultRuleEvaluator` et `CachedViewDefinitionStore`.
