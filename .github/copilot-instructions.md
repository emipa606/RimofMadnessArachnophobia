# GitHub Copilot Instructions for the Arachnophobia Mod

## Mod Overview and Purpose

The Arachnophobia mod for RimWorld introduces giant spiders into the game, enhancing gameplay with the introduction of new mechanics revolving around these creatures. These spiders bring new challenges and interactions within the world, affecting both colonists and the environment. The purpose of the mod is to create a thrilling and challenging experience by introducing a unique arachnid ecosystem that players must navigate.

## Key Features and Systems

- **Building Components:** The mod includes the `Building_Cocoon` and `Building_Web` classes which handle the creation and effects of spider-related structures. These buildings impact the environment and interact with pawns.

- **Spider Behavior:** Implemented through classes like `PawnWebSpinner` and `JobDriver_ConsumeCocoon`, spiders have defined behaviors such as spinning webs, consuming cocoons, and affecting pawn actions.

- **Hatching and Lifecycle:** The `CompMultiHatcher` class manages the lifecycle of spiders, handling the hatching process of spider eggs placed by mature spiders.

- **Incident Events:** The `IncidentWorker_GiantSpiders` class randomly introduces events involving spider appearances, creating dynamic challenges for players.

- **Harmony Patching:** Modifies base game functionality using the `HarmonyPatches` class to integrate the new systems seamlessly.

- **Sanity and Psychology Impact:** Introduces a psychological aspect through the `SanityLossSeverity` class, affecting the mental state of colonists in relation to spiders.

## Coding Patterns and Conventions

- **Class Naming:** Classes follow PascalCase naming conventions, reflecting their functionality (e.g., `Building_Cocoon`, `PawnWebSpinner`).

- **Method Naming:** Methods are named using camelCase and clearly describe their operation, ensuring readability (e.g., `isPathableBy`, `Notify_Placed`).

- **Inheritance and Interfaces:** Utilizes inheritance to extend base game classes (e.g., `Building_Cocoon` inherits from `Building_Casket`) and interfaces where appropriate.

- **Modular Design:** The mod is structured in a modular fashion to separate different systems logically, enhancing maintainability and ease of expansion.

## XML Integration

- **Game Definition Files:** XML files define objects, events, and other aspects not covered by the C# classes. Ensure XMLs are consistent with C# implementations to avoid runtime errors.

- **Def Linking:** Use XML definitions to link C# classes with game content, like items and events, enabling features like custom buildings and incidents.

## Harmony Patching

- **Patch Management:** Use the `HarmonyPatches` class to modify game behaviors. Ensure patches are well-commented for clarity.

- **Safety and Checks:** Implement checks within patches to maintain compatibility and avoid conflicts with other mods. Utilize conditional patching techniques.

## Suggestions for Copilot

- **Predictive Code Suggestions:** Enable auto-suggestions for routine coding tasks like creating getters/setters, implementing interfaces, and iterating through collections.

- **Pattern Recognition:** Use Copilot to recognize code patterns, such as command execution in `JobDriver` methods, to streamline new job creation.

- **Harmony Integration:** Enhance patch creation with Copilot by suggesting template patches and common patching requirements.

- **XML Assistance:** Utilize Copilot to generate XML templates for new content that aligns with the existing mod structure.

Remember, while GitHub Copilot can be a powerful tool for generating code, always review and test generated code carefully to ensure it adheres to your mod’s requirements and maintains compatibility with the RimWorld game engine.
