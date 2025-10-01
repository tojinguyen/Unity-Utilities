# GEMINI.md

## Project Context

This project is a comprehensive utilities package for the **Unity Engine (v6000.3.0a2)**, designed to accelerate game development by providing robust, reusable, and high-performance solutions for common problems.

### Core Modules
The package is organized into several key modules:
- **Event System:** A high-performance, zero-allocation event bus for decoupled communication between different parts of the game.
- **UI Management:** A service-based framework for managing UI screens, popups, and toasts, integrated with Addressables for dynamic loading.
- **Object Pooling:** Generic and Addressables-aware object pooling to reduce garbage collection and improve performance.
- **Audio Management:** A centralized service for controlling music, SFX, and other audio sources, configured via Scriptable Objects.
- **Data Management:** A powerful system for data persistence (save/load) with support for encryption and compression.
- **Monetization:** Pre-built, service-oriented wrappers for **Unity IAP (v4.12.2)** and **Google Mobile Ads (AdMob)**.
- **Addressables Helper:** Simplifies loading, caching, and memory management for assets using the **Addressables (v2.6.0)** system.
- **Patterns & Utilities:** Includes common patterns like a thread-safe `MonoSingleton` and various helper/extension methods.

### Tech Stack & Architecture

-   **C# & Unity Engine**: The core development language and platform.
-   **UniTask**: Used extensively for asynchronous operations. It provides a more efficient and allocation-free alternative to standard `Task` and coroutines, which is critical for performance in games.
-   **Addressable Assets (v2.6.0)**: The primary mechanism for managing assets. It allows for dynamic loading and updating of content, reducing initial build size and improving memory management.
-   **Newtonsoft.Json (v3.2.1)**: Used for flexible and powerful JSON serialization and deserialization, particularly within the Data Management system.
-   **Unity IAP (v4.12.2)** & **Unity Mobile Notifications (v2.4.2)**: Integrated for handling in-app purchases and push notifications.

The architecture is heavily **service-oriented** and **event-driven**:
-   **Services** (e.g., `AudioService`, `IAPService`) provide a clean, static API that can be accessed from anywhere in the codebase, abstracting away the complex underlying implementation.
-   The **Event System** allows modules to communicate without holding direct references to each other, promoting loose coupling and making the codebase easier to maintain and extend.
-   **Scriptable Objects** are used for configuration (e.g., `AudioDatabase`, IAP product catalogs), allowing developers and designers to modify behavior without changing code.

## Instructions for Gemini
- Provide concise answers, focusing on technical details.
- Code samples should be runnable and demonstrate best practices within this project's architecture (e.g., using services, publishing events).
- Always explain step-by-step before providing code.
- Use Markdown for readable output.

## Examples

### Example 1: Writing a New Feature
**Request:** "Create a new 'PowerUp' system. When a player collects a power-up, it should play a sound and show a notification on the screen."

**Gemini's Approach:**
1.  Define a `PowerUpCollected` event struct.
2.  Create a `PowerUpManager` that subscribes to this event.
3.  Inside the event handler, use `AudioService.PlaySFX()` to play a sound and `UIService.ShowUI<NotificationToast>()` to display a message.
4.  In the `PowerUp` pickup object, publish the `PowerUpCollected` event when the player collides with it.

### Example 2: Explaining Code
**Request:** "Explain how the `AddressablesObjectPooling` class works."

**Gemini's Approach:**
1.  Describe the purpose: To pool objects that are loaded via the Addressables system.
2.  Explain the workflow:
    *   `Prewarm`: Loads the asset from Addressables and instantiates a number of objects to fill the pool.
    *   `Get`: Retrieves an inactive object from the pool or, if none are available, instantiates a new one (up to a limit). It keeps track of the Addressable handle.
    *   `Return`: Deactivates the object and returns it to the pool for reuse.
    *   `ClearFeature`: Releases the Addressable asset handle and destroys all pooled objects associated with a specific feature tag, freeing up memory.
3.  Highlight the benefits: Combines the memory efficiency of pooling with the dynamic content delivery of Addressables.

### Example 3: Refactoring Code
**Request:** "I have a `Health` component that directly calls `UIManager.UpdateHealthBar()`. Refactor this to use the event system."

**Gemini's Approach:**
1.  Identify the tight coupling between `Health` and `UIManager`.
2.  Propose creating a `HealthChanged` event struct containing the new health value.
3.  Modify the `Health` component: Instead of calling the `UIManager` directly, it will now publish the `HealthChanged` event.
4.  Modify the `UIManager`: It will now subscribe to the `HealthChanged` event and update the health bar when the event is received.
5.  Provide the `replace` tool calls to perform the changes.