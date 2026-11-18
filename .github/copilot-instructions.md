# GitHub Copilot Instructions for Rim of Madness - Arachnophobia (Continued)

Welcome to the Rim of Madness - Arachnophobia (Continued) mod project! This mod adds spiders to the world of RimWorld, making them tameable, spawnable, and featuring unique abilities. Below are some guidelines and instructions to help you contribute to this project using GitHub Copilot.

## Mod Overview and Purpose

Rim of Madness - Arachnophobia (Continued) is an unofficial port of the original Arachnophobia mod. It aims to bring the experience of living with spiders in RimWorld to a new level by adding various spider species, including regular spiders, giant spiders, and flying spider queens. The mod is compatible with save games and integrates with Combat Extended.

## Key Features and Systems

- **Spiders**: Regular spiders that can be tamed and used for various purposes.
- **Giant Spiders**: Larger, genetically-engineered spiders with enhanced abilities.
- **Flying Spider Queens**: Genetic experiments that can create giant spider eggs and fly short distances.
- **Webs**: Spiders can weave webs to stun smaller animals.
- **Cocoons**: Spiders spin cocoons from dead or downed prey for shelter and food.
- **Spider Silk**: Webs and cocoons occasionally drop spider silk, which can be used in crafting.

## Coding Patterns and Conventions

To maintain consistency and readability, we follow certain coding patterns and conventions:

- **Namespace**: Use `Arachnophobia` for all C# files.
- **Naming Conventions**: Use PascalCase for class names, camelCase for method names, and ALL_CAPS for constants.
- **Comments**: Add comments where necessary to explain complex logic or changes.
- **XML Naming**: Use clear and descriptive names for XML definitions.

## XML Integration

XML files are used to define various game elements such as damage types, body parts, pawn kinds, and hediffs. Here are some key XML files and their purposes:

- **DamageDefs**: Defines the damage types associated with spiders.
- **BodyDefs**: Defines body parts and groups used by spider pawns.
- **PawnKindDefs**: Defines the different types of spiders, including regular, giant, and queen spiders.
- **HediffDefs**: Defines the effects of venom and other spider-related conditions.

## Harmony Patching

Harmony is used for patching vanilla RimWorld methods to integrate new features. Here are some common patching patterns:

- **Prefixes**: Use prefixes to modify or extend the behavior of existing methods before they execute.
- **Postfixes**: Use postfixes to modify or extend the behavior of existing methods after they execute.
- **Transpilers**: Use transpilers to modify the IL code of existing methods.

Example of a Harmony patch:
csharp
[HarmonyPatch(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeTick))]
public static class Pawn_AgeTracker_AgeTick_Patch
{
    [Prefix]
    public static bool Prefix(Pawn_AgeTracker __instance)
    {
        // Custom logic before the original method executes
        return true; // Return true to continue executing the original method
    }

    [Postfix]
    public static void Postfix(Pawn_AgeTracker __instance)
    {
        // Custom logic after the original method executes
    }
}


## Suggestions for Copilot

- **Code Generation**: Use Copilot to generate basic structures for new classes, methods, and XML definitions.
- **Method Implementations**: Use Copilot to generate method implementations for existing classes.
- **Patch Suggestions**: Use Copilot to generate Harmony patches for existing RimWorld methods.
- **XML Templates**: Use Copilot to generate XML templates for new game elements.

## Contributing

If you want to contribute to this project, follow these steps:

1. Fork the repository.
2. Clone your fork to your local machine.
3. Make your changes, following the coding patterns and conventions.
4. Commit your changes with a descriptive commit message.
5. Push your changes to your fork.
6. Create a pull request to the main repository.

For best support, please use the Discord-channel for error-reporting. Always post your log using the Log Uploader when reporting errors.

## Questions and Feedback

If you have any questions, comments, or suggestions, please visit our Discord server.

Thank you for contributing to Rim of Madness - Arachnophobia (Continued)!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

