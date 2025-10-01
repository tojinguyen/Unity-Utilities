# Unity Editor Utilities

This folder contains various editor utilities to improve workflow and productivity within the Unity Editor.

## Subdirectories

### Addressable Importer

Contains the requirements and documentation for a tool to streamline importing assets into the Addressable Asset System.

- **`Requirements.md`**: A detailed design document outlining the features and functionality of the Addressable Importer tool.

### Prefab Management

Tools for managing and maintaining prefabs in the project.

- **`MissingScriptRemover.cs`**: An editor window that scans the entire project to find prefabs with missing script references. It provides an interface to view these prefabs and remove the missing scripts, either individually or all at once. This is useful for cleaning up projects and preventing potential runtime errors.

### Scene Management

Utilities for easier scene navigation and management.

- **`SceneToolbarUtilities.cs`**: Adds a custom dropdown menu to the Unity toolbar that lists all scenes included in the build settings. This allows for quick and easy switching between scenes without having to navigate through the project folders.
