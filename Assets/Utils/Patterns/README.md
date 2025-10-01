# Design Patterns

This directory contains implementations of various common design patterns, tailored for Unity development. These patterns provide robust and flexible solutions for structuring your codebase, managing dependencies, and handling complex logic.

## Implemented Patterns

### 1. Command Pattern

-   **Purpose**: Encapsulates a request as an object, thereby letting you parameterize clients with different requests, queue or log requests, and support undoable operations.
-   **Key Components**:
    -   `ICommand`: Base interface for all commands.
    -   `BaseCommand`: Abstract base class for commands, including `UndoableCommand` and `ParameterizedCommand`.
    -   `CommandManager`: A MonoBehaviour that executes, queues, and manages undo/redo functionality for commands. Supports asynchronous command execution using `UniTask`.

### 2. Factory Pattern

-   **Purpose**: Provides an interface for creating objects in a superclass, but allows subclasses to alter the type of objects that will be created.
-   **Key Components**:
    -   `IFactory`, `IAsyncFactory`, `IManagedFactory`: Interfaces for synchronous, asynchronous, and managed object creation.
    -   `GameObjectFactory`: A MonoBehaviour factory for creating and managing `GameObjects` from prefabs, with integrated object pooling and support for Unity Addressable Assets.

### 3. Service Locator Pattern

-   **Purpose**: Provides a global point of access to services without coupling the client to the concrete classes that implement them.
-   **Key Components**:
    -   `IServiceLocator`: Interface for registering and resolving services.
    -   `ServiceLocator`: A pure C# implementation of the service locator.
    -   `ServiceLocatorManager`: A MonoBehaviour-based singleton that provides global access to the `IServiceLocator` and integrates with Unity's lifecycle.
    -   `ServiceInjector`: Utility for automatic dependency injection, supporting `InjectServiceAttribute` and `IServiceInjectable` interface for performance.

### 4. Singleton Pattern

-   **Purpose**: Ensures a class has only one instance, while providing a global access point to this instance.
-   **Key Components**:
    -   `Singleton<T>`: A generic base class for non-MonoBehaviour classes, providing a thread-safe and lazily-initialized singleton instance.

### 5. State Machine Pattern

-   **Purpose**: Allows an object to alter its behavior when its internal state changes. The object will appear to change its class.
-   **Key Components**:
    -   `IState`, `ITickableState`: Interfaces for states, supporting asynchronous entry/exit and periodic updates.
    -   `BaseState`, `BaseTickableState`: Abstract base classes for states, including generic versions for context-aware states.
    -   `StateMachine`: A type-safe, pure C# implementation that manages state transitions using `Type` as keys, supporting conditional transitions and manual ticking.

### 6. Strategy Pattern

-   **Purpose**: Defines a family of algorithms, encapsulates each one, and makes them interchangeable. Strategy lets the algorithm vary independently from clients that use it.
-   **Key Components**:
    -   `IStrategy`: Interfaces for defining different strategies (with or without parameters/return values).
    -   `StrategyContext`, `StrategyContextMono`: Context classes that hold a reference to a strategy and execute it. `StrategyContextMono` is a MonoBehaviour version for Unity integration.
