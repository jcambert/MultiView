# Roadmap Dynamic View Framework

_Dernière mise à jour : 2026-03-13_

Ce document regroupe les évolutions en backlog, priorisées par user stories.
Objectif : garder une source unique de vérité sur les fonctionnalités livrées, en cours et restantes.

## Convention

- `Done` : implémenté et utilisé.
- `In Progress` : en cours de développement ou de validation.
- `Planned` : planifié, non démarré.
- `Blocked` : attente dépendante externe.

| Etat | Référence | Epic | User Story | Priorité | Résumé |
|------|-----------|------|------------|----------|--------|
| Done | US-DS-01 | Core des définitions | Métamodèle de vue sérialisable | P0 | Définir les contrats métiers et les définitions par type (Form/List/Kanban/Search/Graph/Pivot/Calendar/Gantt). |
| Done | US-DS-02 | Core des définitions | Chargement JSON + cache de vues | P0 | Lecture des définitions via source sérialisée et cache mémoire. |
| Done | US-DS-03 | Core des définitions | Héritage (`extends`) + composition | P0 | Fusion de définitions parent/enfant + inclusions + suppressions. |
| Done | US-DS-04 | Core des définitions | Éviter la récursion cyclique | P0 | Détection de cycle sur la résolution d’héritage/composition. |
| Done | US-UI-01 | Moteur de rendu | Rendu dynamique par type de vue | P0 | `DynamicViewRenderer` avec switch de types de vue. |
| Done | US-UI-02 | Champs dynamiques | Résolution dynamique de widget | P0 | `FieldWidgetRegistry` (widget explicite + fallback par `ViewFieldKind`). |
| Done | US-UI-03 | Champs dynamiques | Règles de visibilité/édition | P0 | `visible`, `readonly`, `required`, `enabled` dans `DynamicFieldRenderer`. |
| Done | US-BIZ-01 | Moteur de règles | Expressions booléennes compilées | P0 | Parser de règles + cache de compilation dans `DefaultRuleEvaluator`. |
| Done | US-BIZ-02 | Actions | Dispatcher d’actions découplé | P0 | `IViewActionDispatcher` + `IViewActionHandler`. |
| Done | US-CONF-01 | Paramétrage Search | Distinguer widget de formulaire/list vs filtre | P1 | `searchWidget` / `searchWidgetOptions` introduits pour `SearchView`. |
| Done | US-CONF-02 | UX | Layout MudBlazor stable (dropdown/menus) | P0 | Documentation + providers racine (`MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`, `MudSnackbarProvider`). |
| In Progress | US-DS-05 | Core des définitions | Versioning des définitions | P1 | Stocker la version d’une définition et gérer la compatibilité évolutive. |
| In Progress | US-DS-06 | Core des définitions | Validation JSON Schema | P1 | Valider format, types, dépendances et champs inconnus à l’import. |
| Done | US-UI-04 | List View | Pagination côté client + sélection d’affichage | P1 | `enablePaging` pilote la pagination avec taille de page configurable. |
| Done | US-UI-05 | List View | Recherche globale + tri multi-colonnes | P1 | Tri multi-colonnes côté client implémenté avec pagination associée. |
| Done | US-UI-06 | Search View | Pagination + tri dans Search | P1 | Tri + pagination côté client dans Search avec `defaultPageSize` et reset d’état. |
| Done | US-UI-07 | Kanban View | Colonnes dynamiques sur règles | P2 | Colonnes de Kanban explicites, règles `when`, fallback `groupByField` et option `showUnassignedColumn`. |
| Planned | US-UI-08 | Graph View | Vue analytique réelle | P2 | Agrégations groupées, tooltips, périodes, export. |
| Planned | US-UI-09 | Pivot View | Données pivots multi-indicateurs | P2 | Colonnes dynamiques par définition + visualisation croisée réelle. |
| Planned | US-UI-10 | Calendar View | Composant calendrier robuste | P2 | Bucketing hebdomadaire/mensuel, collisions, actions sur période. |
| Planned | US-UI-11 | Gantt View | Gestion complète des tâches | P2 | Lignes/groupes, dépendances, progression, vue temporelle. |
| Planned | US-SEC-01 | Sécurité | Règles avec contexte utilisateur | P0 | Ajouter contexte `user`, rôles et claims dans l’évaluation des règles. |
| Planned | US-SEC-02 | Sécurité | Contrôle d’accès aux actions/affichages | P0 | Verrouiller visibilités sensibles au niveau actions + champs selon permissions. |
| Planned | US-OPS-01 | Observabilité | Journal d’audit des changements de vues | P2 | Traçabilité import/édition/validation des définitions. |
| Planned | US-OPS-02 | Performance | Cache distribué + invalidation | P2 | Ajouter Redis (ou équivalent), invalidation basée version. |
| Planned | US-OPS-03 | Qualité | Couverture de tests (Core + Blazor) | P1 | Suites unitaires + tests de résolution + tests de rendu de base. |

## Feuille de route par version

### Version 0.3 (Priorité production MVP+)

- Finaliser `Validation JSON Schema` (US-DS-06).
- Implémenter `pagination` et `tri` pour List/Search (US-UI-04, US-UI-05, US-UI-06). ✅
- Finaliser Kanban avancé (US-UI-07). ✅
- Renforcer les vues Graph/Pivot/Calendar/Gantt avec fonctionnalités métier minimales (US-UI-08..11 selon priorité).
- Ajouter sécurité de base sur règles/actions (US-SEC-01, US-SEC-02).

### Release Notes

- 2026-03-13 — US-UI-04 / US-UI-05 / US-UI-06 : pagination côté client, tri sur colonnes et sélection de taille de page pour List/Search (`enablePaging`, `defaultPageSize`), avec reset de pagination sur changements de filtre/recherche.
- 2026-03-13 — US-UI-07 : colonnes Kanban explicites (`columns`), matching par expression (`when`) ou par valeur (`value`), matching fallback par `groupByField`, et `showUnassignedColumn`.

### Version 0.4 (Qualité opérationnelle)

- Versioning + invalidation avancée du cache (US-DS-05, US-OPS-02).
- Journalisation & observabilité (US-OPS-01).
- Couverture tests complète (US-OPS-03).

## User Stories détaillées

### US-DS-06 — Validation JSON Schema

**En tant que** intégrateur de vues  
**Je veux** que chaque JSON soit validé avant chargement  
**Afin que** les erreurs de config soient détectées tôt.

**Critères d’acceptation**
- Un JSON invalide renvoie une erreur explicite (message + position + champ).
- Les champs inconnus déclenchent un warning/error configurable.
- Les règles incohérentes (`widget` introuvable, champs manquants) sont remontées.

### US-UI-04 — Pagination List/Search

**En tant que** utilisateur métier  
**Je veux** naviguer page par page sur de gros volumes  
**Afin de** conserver des temps de rendu constants.

**Critères d’acceptation**
- `enablePaging` active la pagination visuelle.
- `defaultPageSize` est utilisé par défaut.
- La page courante est persistée au changement de filtres/recherche.

### US-SEC-02 — Contrôle d’accès actions/champs

**En tant que** admin sécurité  
**Je veux** restreindre visibilité et interactivité selon les droits utilisateurs  
**Afin de** garantir la conformité métier.

**Critères d’acceptation**
- Les règles peuvent lire le contexte utilisateur.
- Les actions non autorisées ne sont ni visibles ni exécutables.
- Les champs non autorisés sont masqués ou en lecture seule selon politique.

## Mécanisme de maintien à jour

- À chaque session de travail, mettre à jour :
  - l’état des user stories (Done/In Progress/Planned/Blocked),
  - la version cible si applicable,
  - les notes d’implémentation dans la colonne "Résumé" et les critères validés.

- Quand une story sort de `In Progress` vers `Done`, ajouter dans cette section :

```text
### Release Notes
- US-XXX : court résumé + date + PR/commit associé
```

- Référencer les fichiers modifiés dans les stories résolues.
