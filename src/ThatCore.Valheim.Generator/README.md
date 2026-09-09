# ThatCore.Valheim.Generator
 
Emits the ThatCore, ThatCore.Shared and ThatCore.Valheim source directly 
into the assembly that references it.

This is primarily core tooling used by the authors other Valheim mods.

## Why

ThatCore is shared by several mods that release independently but may all be
loaded at once. Referencing it as a normal assembly means the first mod to load
wins and the others get whatever version it happened to bring - so each mod
wants its own private copy.

This package hands each consuming project its own copy at compile time, which
has had all its public access modifiers changed to internal (except for Shared).

## Requirements

The generated source is compiled as part of consuming project, so the consuming 
project must supply the references ThatCore and ThatCore.Valheim themselves need.

- BepInEx
- UnityEngine
- UnityEngine.CoreModule
- assembly_valheim
- assembly_utils
- HarmonyX
- YamlDotNet

## Links
 
- Source: https://github.com/ASharpPen/Valheim.ThatCore
- License: Unlicense