# MultiView Dynamic Views (Framework de Vues Dynamiques)

Framework Blazor/MudBlazor inspiré des vues dynamiques Odoo, implémenté proprement en C#/.NET 10 avec une architecture modulaire et extensible.

## Version

- `net10.0`
- MudBlazor `9.0.0`
- ASP.NET Core `InteractiveServer`
- C# 12 / 14-compatible syntaxe (ciblage `net10`)

## Objectifs du projet

- Exposer un modèle de définition de vues sérialisable (JSON) indépendant de la UI.
- Rendre dynamiquement des vues métier sous plusieurs formes.
- Distinguer clairement les couches métier, métier-application et UI.
- Déporter règles métier (visible, readonly, required, enabled) dans des expressions.
- Permettre l’extension via widgets, handlers, providers et stores de définition.
- Prévoir les caches de définitions, de résolution de widget et de règles compilées.
- Préparer l’évolution vers des vues analytiques avancées et des plugins métier.

Vue actuellement supportée :
- `Form`
- `List`
- `Kanban`
- `Search`
- `Graph`
- `Pivot`
- `Calendar`
- `Gantt`

## Arborescence complète

```text
src
  MultiView.DynamicViews.Domain
  MultiView.DynamicViews.Core
  MultiView.DynamicViews.Blazor
  MultiView.DynamicViews.Sample
    Components
    Data
    Actions
    Models
    Definitions
    wwwroot
    Properties
```

## Architecture cible

### 1) Domain

Le domaine contient uniquement les contrats, enums et métamodèles de vue.

Fichiers clés :

- [DynamicViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/DynamicViewDefinition.cs)
- [DynamicViewKind.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/ViewKind.cs)
- [ViewFieldDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/ViewFieldDefinition.cs)
- [ViewRuleSet.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/ViewRuleSet.cs)
- [ViewCompositionDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/ViewCompositionDefinition.cs)
- [ViewActionDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/ViewActionDefinition.cs)
- [FormViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/FormViewDefinition.cs)
- [ListViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/ListViewDefinition.cs)
- [KanbanViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/KanbanViewDefinition.cs)
- [SearchViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/SearchViewDefinition.cs)
- [GraphViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/GraphViewDefinition.cs)
- [PivotViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/PivotViewDefinition.cs)
- [CalendarViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/CalendarViewDefinition.cs)
- [GanttViewDefinition.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Domain/GanttViewDefinition.cs)

## 2) Core

Le core implémente :

- stockage sérialisé des vues
- résolution + héritage/composition
- cache mémoire
- moteur de règles
- registre de widgets
- dispatch d’actions
- services DI transverses

Fichiers clés :

- [IViewDefinitionStore.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/ViewDefinitionContracts.cs)
- [ISerializedViewDefinitionStore.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/ViewDefinitionContracts.cs)
- [IDataProvider.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/DataProviderContracts.cs)
- [IViewActionDispatcher.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/ActionContracts.cs)
- [IViewActionHandler.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/ActionContracts.cs)
- [IRecordPropertyAccessor.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/RuleContracts.cs)
- [DefaultRuleEvaluator.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/RuleEvaluator/DefaultRuleEvaluator.cs)
- [FieldWidgetRegistry.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Widgets/FieldWidgetRegistry.cs)
- [CachedViewDefinitionStore.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Caching/CachedViewDefinitionStore.cs)
- [ViewDefinitionSerializer.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Serialization/ViewDefinitionSerializer.cs)
- [JsonFileSerializedViewDefinitionSource.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Services/JsonFileSerializedViewDefinitionSource.cs)
- [InMemorySerializedViewDefinitionSource.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Services/InMemorySerializedViewDefinitionSource.cs)
- [DefaultViewActionDispatcher.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Views/DefaultViewActionDispatcher.cs)
- [ServiceCollectionExtensions.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/ServiceCollectionExtensions.cs)

## 3) Blazor

La couche Blazor n’a aucune logique métier métier applicative.

- composant routeur dynamique `DynamicViewRenderer`
- composants dédiés par type de vue
- moteur de champs via `DynamicFieldRenderer`
- widgets MudBlazor réutilisables

Fichiers clés :

- [DynamicViewRenderer.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/DynamicViewRenderer.razor)
- [DynamicFieldRenderer.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Fields/DynamicFieldRenderer.razor)
- [FieldWidgetBase.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Fields/FieldWidgetBase.cs)
- [DynamicFormView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicFormView.razor)
- [DynamicListView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicListView.razor)
- [DynamicKanbanView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicKanbanView.razor)
- [DynamicSearchView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicSearchView.razor)
- [DynamicGraphView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicGraphView.razor)
- [DynamicPivotView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicPivotView.razor)
- [DynamicCalendarView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicCalendarView.razor)
- [DynamicGanttView.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Views/DynamicGanttView.razor)
- [TextFieldWidget.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Widgets/TextFieldWidget.razor)
- [BooleanFieldWidget.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Widgets/BooleanFieldWidget.razor)
- [NumberFieldWidget.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Widgets/NumberFieldWidget.razor)
- [CurrencyFieldWidget.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Widgets/CurrencyFieldWidget.razor)
- [DateFieldWidget.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Widgets/DateFieldWidget.razor)
- [SelectFieldWidget.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Widgets/SelectFieldWidget.razor)
- [ServiceCollectionExtensions.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Services/ServiceCollectionExtensions.cs)

## 4) Exemple métier complet

Le sample implémente `SaleOrder` et une vue complète basée sur héritage et composition.

- [SaleOrder.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Models/SaleOrder.cs)
- [SaleOrderDataProvider.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Data/SaleOrderDataProvider.cs)
- [Home.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Components/Pages/Home.razor)

## Concepts clefs

### Vues dynamiques

Toutes les vues partent d’un JSON sérialisable en un dérivé de `DynamicViewDefinition`.

### Héritage + composition (préférence recommandée)

Le mécanisme supporte deux axes :

- `extends` : cette vue hérite d’une vue parente.
- `composition.includes` : inclusion de fragments par identifiant.
- `composition.remove*` : retrait de champs, sections, colonnes, actions après fusion.

La résolution se fait dans `CachedViewDefinitionStore` et se déroule en chaîne :

1. charge du JSON brut via `ISerializedViewDefinitionStore`
2. désérialisation selon `kind`
3. résolution de l’héritage récursif
4. fusion de la composition
5. application des suppressions
6. mise en cache avec TTL 10 minutes

Exemple concret (base + fragment + override) :

- [saleorder_form_base.json](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Definitions/saleorder-form-base.json)
- [saleorder_form_notes_fragment.json](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Definitions/saleorder-form-notes-fragment.json)
- [saleorder_form.json](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Definitions/saleorder-form.json)

### Règles conditionnelles

Les règles sont stockées dans `ViewRuleSet` sur les champs ou les actions.

Conditions supportées par le parseur :

- opérateurs logiques `and`, `or`, `!`, parenthèses
- opérateurs de comparaison `==`, `!=`, `<`, `<=`, `>`, `>=`
- booléens, chaînes, nombres, null, identifiants

Exemple :
- `Status != 'Cancelled'`
- `Confirmed == true and Amount > 10000`

Le compilateur de règles met en cache les fonctions évaluées dans `DefaultRuleEvaluator`.

### Cache

Trois caches opérationnels :

- cache de définition brute + résolue : [CachedViewDefinitionStore.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Caching/CachedViewDefinitionStore.cs)
- cache de résolution de widget : [FieldWidgetRegistry.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Widgets/FieldWidgetRegistry.cs)
- cache de règles compilées : [DefaultRuleEvaluator.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/RuleEvaluator/DefaultRuleEvaluator.cs)

## Cycle d’exécution du rendu

1. Charger les définitions via un store
2. Résoudre la définition (`extends`, `composition`, suppression)
3. Récupérer le renderer via `DynamicViewRenderer`
4. Résoudre les widgets dynamiques pour chaque champ
5. Évaluer les règles par champ/actions
6. Renvoyer la vue Blazor spécialisée

Flux détaillé :

- `Definition` chargé par [IViewDefinitionStore.GetAsync](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/ViewDefinitionContracts.cs)
- [DynamicViewRenderer](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/DynamicViewRenderer.razor) sélectionne le composant de vue
- [DynamicFieldRenderer](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Fields/DynamicFieldRenderer.razor) récupère la définition de champ, le record et le contexte
- [FieldWidgetRegistry](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Widgets/FieldWidgetRegistry.cs) résout le composant widget
- `IRuleEvaluator` applique les règles
- `IViewActionDispatcher` résout le handler d’action

## DI / intégration

### Pipeline recommandé

1. `AddMudServices()`
2. `AddDynamicViewsMudBlazor()`
3. Enregistrer providers métiers (`IDataProvider<T>`)
4. Déclarer handlers d’action (`AddDynamicViewActionHandler<THandler>()`)
5. Enregistrer les définitions (mémoire ou dossier)

Code d’exemple :

```csharp
builder.Services.AddMudServices();
builder.Services.AddDynamicViewsMudBlazor();
builder.Services.AddDynamicViewActionHandler<ToggleConfirmActionHandler>();
builder.Services.AddSingleton<IDataProvider<SaleOrder>, SaleOrderDataProvider>();

string definitionsFolder = Path.Combine(builder.Environment.ContentRootPath, "Definitions");
Dictionary<string, string> definitions = Directory.EnumerateFiles(definitionsFolder, "saleorder-*.json")
    .Select(File.ReadAllText)
    .Select(json => new { Json = json, Id = ExtractDefinitionId(json) })
    .Where(item => !string.IsNullOrWhiteSpace(item.Id))
    .ToDictionary(item => item.Id!, item => item.Json, StringComparer.OrdinalIgnoreCase);

builder.Services.AddDynamicViewDefinitions(definitions);
```

- [Program.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Program.cs)

### Enregistrer des providers métiers

Tous les handlers et providers sont découplés de l’UI.

- Voir [IDataProvider<TModel>](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/DataProviderContracts.cs)
- Implémentation exemple : [SaleOrderDataProvider.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Data/SaleOrderDataProvider.cs)

### Ajouter un handler d’action personnalisé

Le handler implémente `IViewActionHandler` et est injecté via DI.

- [ViewActionContext](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Core/Abstractions/ActionContracts.cs)
- `ActionName`
- `CanHandle(ViewActionContext)`
- `HandleAsync`

Exemple : [ToggleConfirmActionHandler.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Actions/ToggleConfirmActionHandler.cs)

### Ajouter un widget personnalisé

Le système autorise des widgets plugins :

- composant Blazor qui dérive de [FieldWidgetBase](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Components/Fields/FieldWidgetBase.cs)
- enregistrement via `registerWidgets` dans `AddDynamicViewsMudBlazor`

Exemple d’usage :

```csharp
builder.Services.AddDynamicViewsMudBlazor(registry =>
{
    registry.Register("my_widget", typeof(MonWidgetCible));
    registry.RegisterFallback(ViewFieldKind.Text, typeof(MonWidgetFallback));
});
```

Voir l’implémentation [ServiceCollectionExtensions.cs](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Blazor/Services/ServiceCollectionExtensions.cs)

## Schéma de métamodèle (JSON)

### Form

```json
{
  "id": "saleorder_form",
  "name": "saleorder.form",
  "kind": "Form",
  "model": "SaleOrder",
  "title": "Commande de vente",
  "extends": "saleorder_form_base",
  "composition": {
    "includes": ["saleorder_form_notes_fragment"],
    "removeFields": ["SalesTeam"]
  },
  "fields": [
    { "name": "Number", "label": "N° commande", "kind": "Text", "rules": { "readonly": "Status == 'Confirmed'" } },
    { "name": "Amount", "label": "Montant", "kind": "Currency", "rules": { "readonly": "Status == 'Cancelled'" } },
    { "name": "Currency", "label": "Devise", "kind": "Select", "widget": "select", "widgetOptions": ["EUR", "USD", "GBP"], "rules": { "readonly": "Status == 'Cancelled'" } },
    { "name": "Confirmed", "label": "Confirmée", "kind": "Boolean" }
  ],
  "sections": [
    {
      "id": "main",
      "label": "Principale",
      "columns": 2,
      "fields": ["Number", "CustomerName", "OrderDate", "Status", "Amount", "Currency", "Confirmed"]
    },
    {
      "id": "extra",
      "label": "Complément",
      "columns": 1,
      "fields": ["Origin", "Notes"]
    }
  ],
  "actions": [
    { "name": "toggle_confirm", "label": "Confirmer / Réouvrir", "enabledRule": "Status != 'Cancelled'" }
  ]
}
```

### List

```json
{
  "id": "saleorder_list",
  "name": "saleorder.list",
  "kind": "List",
  "model": "SaleOrder",
  "title": "Liste des commandes",
  "fields": [
    { "name": "Number", "label": "N°", "kind": "Text" },
    { "name": "CustomerName", "label": "Client", "kind": "Text" },
    { "name": "Amount", "label": "Montant", "kind": "Currency" },
    { "name": "OrderDate", "label": "Date", "kind": "Date" },
    { "name": "Status", "label": "État", "kind": "Select", "widget": "select", "widgetOptions": ["Draft", "Confirmed", "Cancelled"] }
  ],
  "columns": [
    { "field": "Number", "header": "N°" },
    { "field": "CustomerName", "header": "Client" },
    { "field": "Amount", "header": "Montant" },
    { "field": "Status", "header": "État" }
  ],
  "enableSearch": true,
  "enablePaging": true
}
```

### Search

```json
{
  "id": "saleorder_search",
  "name": "saleorder.search",
  "kind": "Search",
  "model": "SaleOrder",
  "title": "Recherche commandes",
  "searchFields": ["Number", "CustomerName", "Origin", "Status", "Notes"],
  "defaultPageSize": 20,
  "fields": [
    { "name": "Number", "kind": "Text" },
    { "name": "CustomerName", "kind": "Text" },
    { "name": "Origin", "kind": "Text" },
    { "name": "Status", "kind": "Select", "searchWidget": "select", "searchWidgetOptions": ["Draft", "Confirmed", "Cancelled"] },
    { "name": "Amount", "kind": "Currency" }
  ]
}
```

Dans une SearchView, la recherche ciblée privilégie désormais :
- `searchWidget` / `searchWidgetOptions` si présents
- sinon `widget` / `widgetOptions`
- sinon `kind` par défaut

Cette règle évite d’interpréter les widgets destinés au formulaire/tableau comme widgets de filtre par défaut dans la SearchView.

### Kanban

```json
{
  "id": "saleorder_kanban",
  "name": "saleorder.kanban",
  "kind": "Kanban",
  "model": "SaleOrder",
  "title": "Kanban des commandes",
  "groupByField": "Status",
  "card": {
    "headerField": "Number",
    "footerField": "CustomerName",
    "detailFields": ["Amount", "OrderDate", "Status", "Confirmed"],
    "colorField": "Status"
  },
  "fields": [
    { "name": "Number", "kind": "Text" },
    { "name": "CustomerName", "kind": "Text" },
    { "name": "Amount", "kind": "Currency" },
    { "name": "OrderDate", "kind": "Date" }
  ]
}
```

### Graph

```json
{
  "id": "saleorder_graph",
  "name": "saleorder.graph",
  "kind": "Graph",
  "model": "SaleOrder",
  "title": "Répartition du CA",
  "categoryField": "Status",
  "valueField": "Amount",
  "seriesField": "Currency",
  "aggregation": "Sum",
  "chartType": "Bar",
  "limit": 20,
  "fields": [
    { "name": "Status", "kind": "Select", "widget": "select", "widgetOptions": ["Draft", "Confirmed", "Cancelled"] },
    { "name": "Amount", "kind": "Currency" },
    { "name": "Currency", "kind": "Select", "widget": "select", "widgetOptions": ["EUR", "USD", "GBP"] }
  ]
}
```

### Pivot

```json
{
  "id": "saleorder_pivot",
  "name": "saleorder.pivot",
  "kind": "Pivot",
  "model": "SaleOrder",
  "title": "Pivot: Statut x Origine",
  "rowField": "Status",
  "columnField": "Origin",
  "valueField": "Amount",
  "aggregation": "Sum",
  "valuePrecision": 2,
  "fields": [
    { "name": "Status", "kind": "Select", "widget": "select", "widgetOptions": ["Draft", "Confirmed", "Cancelled"] },
    { "name": "Origin", "kind": "Text" },
    { "name": "Amount", "kind": "Currency" }
  ]
}
```

### Calendar

```json
{
  "id": "saleorder_calendar",
  "name": "saleorder.calendar",
  "kind": "Calendar",
  "model": "SaleOrder",
  "title": "Calendrier des commandes",
  "startDateField": "OrderDate",
  "endDateField": "OrderDate",
  "titleField": "Number",
  "subtitleField": "CustomerName",
  "bucket": "day",
  "limitPerBucket": 10,
  "fields": [
    { "name": "Number", "kind": "Text" },
    { "name": "CustomerName", "kind": "Text" },
    { "name": "OrderDate", "kind": "Date" },
    { "name": "Amount", "kind": "Currency" },
    { "name": "Status", "kind": "Select", "widget": "select", "widgetOptions": ["Draft", "Confirmed", "Cancelled"] }
  ]
}
```

### Gantt

```json
{
  "id": "saleorder_gantt",
  "name": "saleorder.gantt",
  "kind": "Gantt",
  "model": "SaleOrder",
  "title": "Tâches",
  "startDateField": "OrderDate",
  "endDateField": "OrderDate",
  "labelField": "Number",
  "groupByField": "Status",
  "limit": 20,
  "fields": [
    { "name": "Number", "kind": "Text" },
    { "name": "CustomerName", "kind": "Text" },
    { "name": "OrderDate", "kind": "Date" },
    { "name": "Status", "kind": "Select", "widget": "select", "widgetOptions": ["Draft", "Confirmed", "Cancelled"] },
    { "name": "Amount", "kind": "Currency" }
  ]
}
```

## Intégration MudBlazor / layout

`MudSelect` et tous les overlays MudBlazor requièrent les providers appropriés dans le layout racine en mode interactif.

- [MainLayout.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Components/Layout/MainLayout.razor)

Le layout d’exemple contient :

- `MudThemeProvider`
- `MudPopoverProvider`
- `MudDialogProvider`
- `MudSnackbarProvider`

Si ces composants manquent, les dropdowns et menus peuvent échouer en `InteractiveServer`.

## Routes et rendu interactif

Le sample utilise `App.razor` + `AppRoutes.razor`.

- [App.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Components/App.razor)
- [AppRoutes.razor](C:/Users/jambert/source/repos/MultiView/src/MultiView.DynamicViews.Sample/Components/AppRoutes.razor)

## Scripts de build et run

- `dotnet build MultiView.slnx`
- `dotnet run --project src/MultiView.DynamicViews.Sample`

## Commandes utiles pour tests

- `dotnet run --project src/MultiView.DynamicViews.Sample`
- Ouvrir `https://localhost:5001` (ou l’URL affichée)

## Points de personnalisation avancée

### Ajouter de nouveaux kinds de vue

Ajouter un contrat métier dans `Domain`, implémenter le composant Razor dans `Blazor/Components/Views`, puis étendre :

- `ViewDefinitionSerializer`
- `CachedViewDefinitionStore` (résolution/merge/removal)
- `DynamicViewRenderer` switch
- éventuellement les helpers de tests/fixtures

### Persistance réelle

Le sample utilise une source JSON et un provider mémoire. En production, brancher :

- une source DB/HTTP dans `JsonFileSerializedViewDefinitionSource`
- cache distribué
- politique d’invalidation basée sur version de définition
- audit des modifications de vues

### Sécurité

Le framework actuel ne gère pas le multitenant, l’authentification d’action ni l’autorisation au niveau champ.

A intégrer via :

- expression d’évaluation enrichie avec contexte utilisateur
- middleware de sécurité dans `CanHandle`
- filtrage de définition serveur
- signatures/validation JSON au chargement

## Checklist technique déjà couverte

- Inversion de dépendance entre couches
- séparation définition/rendu
- sérialisation versionnée par `kind`
- héritage de vue
- composition et suppression
- cache mémoire
- règles conditionnelles compilées
- dispatcher d’actions
- widgets extensibles

## Limites connues

- Les vues Graph/Pivot/Calendar/Gantt restent des implémentations fonctionnelles mais simplifiées.
- Les styles sont encore orientés démo visuelle.
- Pas de tests unitaires/intégration automatiques dans le sample.
- Pas de mécanisme d’édition de vues en runtime.

## Roadmap technique

- Migrations de version et validation JSON Schema.
- Pagination côté serveur pour List/Search.
- Virtualisation de grands jeux de données.
- Cache mémoire + cache distribué (Redis).
- Plugins OData/EF/GraphQL côté `IDataProvider`.
- Moteur d’expression plus riche (fonctions métier, `in`, `contains`, date diff).
- Edition graphique des vues (designer).

## Validation initiale

Le projet compile tel quel avec les vues manquantes précédemment demandées implémentées (`Search`, `Graph`, `Pivot`, `Calendar`, `Gantt`).

Vérifier via :

- `dotnet build`
- `dotnet run --project src/MultiView.DynamicViews.Sample`

