# Feature Architecture Guidelines

This document outlines the standard architecture and conventions for building features in this project, extracted from the `users` feature module. Following this structure ensures consistency, separation of concerns, and maintainability across all feature modules.

## Directory Structure Convention

A standard feature module should be organized into the following directories:

```text
src/features/<feature-name>/
├── components/       # Feature-specific UI components
├── context/          # Local state and query providers
├── dialogs/          # Modal/Dialog components and registry
├── hooks/            # Data fetching and mutation hooks
├── schemas/          # Validation schemas
└── <Feature>Page.tsx # Main entry point for the feature
```

## Module Responsibilities

### 1. Entry Point (`<Feature>Page.tsx`)
- Acts as the main container for the feature.
- Responsible for wrapping the view with necessary context providers (e.g., query contexts).
- Orchestrates layout and feature-level state but avoids complex business logic.

### 2. Components (`components/`)
- Contains feature-specific UI elements such as tables, forms, and custom filters.
- **Convention:** Form components should be separated from Dialog components to allow reusability (e.g., `CreateUserForm` vs `CreateUserDialog`).

### 3. Context (`context/`)
- Contains feature-level state management, typically query contexts for table filtering, pagination, and sorting.
- **Convention (Context-Driven Table State):**
  - **Single Source of Truth:** The URL is the single source of truth for table state. The Context Schema MUST exactly match the Backend API Schema (e.g., `zGetUsersQuery`).
  - **No JSON Strings in URL:** Do not stringify complex objects (like `filters` arrays) into the URL. Use native Object/Array query parameters so TanStack Router can parse them directly into API shapes.
  - **Thin Context Wrapper:** Use `createQueryContext` to bridge the routing state with the component tree. The Context should merely expose `Route.useSearch()` and `Route.useNavigate()`, avoiding heavy business logic or intermediate string parsing.
  - **Encapsulated Filter Panel:** Any transformation from UI form state to the API shape (e.g., `buildFilterArray` or `parseFilterArray`) must be fully encapsulated within the `FilterPanel` component or its helpers. The Table and Context only interact with the final API shape (e.g., `FilterField[]`).

### 4. Dialogs (`dialogs/`)
- Contains all modal/dialog components for the feature.
- **Convention (`dialogs/index.ts`):** 
  - Must include an `index.ts` file that serves as a dialog registry.
  - Lazily import dialog components to optimize bundle size.
  - Use `registerDialog` to register each dialog with a unique ID.
  - Augment the `GlobalDialogRegistry` interface to provide strict type safety for dialog IDs and their expected parameters.

### 5. Hooks (`hooks/`)
- Contains custom React hooks, primarily for data fetching and mutations.
- **Convention:** 
  - Abstract data access library implementations (e.g., `@tanstack/react-query`).
  - Encapsulate query keys, mutation options, and invalidation logic within these hooks to keep components clean.

### 6. Schemas (`schemas/`)
- Contains validation schemas (e.g., Zod) for form data, API requests, and search parameters.
- **Convention:** Keep validation logic separate from UI components. Use these schemas in forms (via resolvers) and hooks.

## Key Architectural Rules
- **Decoupled Dialogs:** Dialogs must be registered globally via the registry pattern and triggered via a global dialog store. Components should not manage dialog visibility state locally.
- **URL-Driven State:** Table filters and pagination should be synchronized with the URL using the routing context.
- **Type Safety:** Always augment global interfaces (like `GlobalDialogRegistry`) when exposing feature-level constructs to global stores.
- **Lazy Loading:** Dialogs and heavy components should be lazily loaded to maintain performance.
