# Helper Utilities

This folder contains a collection of helper scripts and utilities to simplify common tasks in Unity development.

## Features

### Console Logger

A static class that provides enhanced logging capabilities. It allows you to log messages to the Unity console with different colors and supports conditional compilation using the `ALL_LOG` scripting define. This is useful for enabling or disabling logs in different builds.

#### How to Use

```csharp
// Log a message with a specific color
ConsoleLogger.LogColor("This is a blue message.", ColorLog.BLUE);

// Log an error message
ConsoleLogger.LogError("This is an error!");

// Log a warning message
ConsoleLogger.LogWarning("This is a warning.");
```

### Mono Singleton

A generic base class for creating singletons that inherit from `MonoBehaviour`. It provides a thread-safe way to access a single instance of a class and handles the creation and destruction of the instance.

#### How to Use

```csharp
public class MyManager : MonoSingleton<MyManager>
{
    // Your manager's logic here
}

// Access the singleton instance
MyManager.Instance.MyMethod();
```
