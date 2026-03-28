# Hello World Scaffold Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bootstrap the Godot 4 + C# project so it opens, builds, runs, and matches the architecture spec's folder structure.

**Architecture:** Manual scaffold — all files hand-authored. Godot project in `src/`, C# solution at repo root. One scene with a label, one script that prints on ready.

**Tech Stack:** Godot 4.4.1 mono, C# (.NET 10), `godot-mono` binary

---

## File Structure

| File | Responsibility |
|------|---------------|
| `PathOfChaz.sln` | C# solution at repo root, references the .csproj in src/ |
| `src/project.godot` | Godot project config — C# enabled, pixel-art window, main scene set |
| `src/PathOfChaz.csproj` | C# project targeting Godot 4.4 |
| `src/Scripts/Core/Main.cs` | Root node script, prints to console on `_Ready()` |
| `src/Scenes/Core/Main.tscn` | Main scene — Control root + centered Label |
| `.editorconfig` | C# naming conventions |
| `src/Scenes/UI/.gitkeep` | Placeholder for future UI scenes |
| `src/Scenes/World/.gitkeep` | Placeholder for future World scenes |
| `src/Scripts/Characters/.gitkeep` | Placeholder for character scripts |
| `src/Scripts/Items/.gitkeep` | Placeholder for item scripts |
| `src/Scripts/World/.gitkeep` | Placeholder for world scripts |
| `src/Scripts/UI/.gitkeep` | Placeholder for UI scripts |
| `src/Resources/.gitkeep` | Placeholder for .tres data definitions |
| `src/Assets/Sprites/.gitkeep` | Placeholder for sprite exports |
| `src/Assets/Audio/.gitkeep` | Placeholder for audio |
| `src/Assets/Fonts/.gitkeep` | Placeholder for fonts |
| `src/Addons/.gitkeep` | Placeholder for Godot plugins |

---

### Task 1: Create the C# project and solution

**Files:**
- Create: `src/PathOfChaz.csproj`
- Create: `PathOfChaz.sln`

- [ ] **Step 1: Create the .csproj**

Create `src/PathOfChaz.csproj`:

```xml
<Project Sdk="Godot.NET.Sdk/4.4.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <RootNamespace>PathOfChaz</RootNamespace>
  </PropertyGroup>
</Project>
```

Note: Godot.NET.Sdk 4.4.0 targets net8.0 regardless of the system .NET version. This is correct.

- [ ] **Step 2: Create the .sln at repo root**

Run from repo root:

```bash
dotnet new sln --name PathOfChaz
dotnet sln add src/PathOfChaz.csproj
```

- [ ] **Step 3: Verify it builds**

Run: `dotnet build`

Expected: Build succeeded with 0 errors. Warnings about no source files are fine.

- [ ] **Step 4: Commit**

```bash
jj new
jj commit -m "scaffold: add C# solution and project file"
```

---

### Task 2: Create the .editorconfig

**Files:**
- Create: `.editorconfig`

- [ ] **Step 1: Create .editorconfig**

Create `.editorconfig` at repo root:

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# C# naming conventions
dotnet_naming_rule.private_fields_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_underscore.style = underscore_prefix
dotnet_naming_rule.private_fields_underscore.severity = suggestion

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.underscore_prefix.capitalization = camel_case
dotnet_naming_style.underscore_prefix.required_prefix = _

dotnet_naming_rule.public_members_pascal.symbols = public_members
dotnet_naming_rule.public_members_pascal.style = pascal_case_style
dotnet_naming_rule.public_members_pascal.severity = suggestion

dotnet_naming_symbols.public_members.applicable_kinds = property, method, event
dotnet_naming_symbols.public_members.applicable_accessibilities = public

dotnet_naming_style.pascal_case_style.capitalization = pascal_case
```

- [ ] **Step 2: Commit**

```bash
jj commit -m "scaffold: add .editorconfig with C# conventions"
```

---

### Task 3: Create the Main script

**Files:**
- Create: `src/Scripts/Core/Main.cs`

- [ ] **Step 1: Create the script**

Create `src/Scripts/Core/Main.cs`:

```csharp
using Godot;

namespace PathOfChaz.Core;

public partial class Main : Control
{
    public override void _Ready()
    {
        GD.Print("Path of Chaz is alive");
    }
}
```

- [ ] **Step 2: Verify it builds**

Run: `dotnet build`

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Commit**

```bash
jj commit -m "scaffold: add Main.cs entry point script"
```

---

### Task 4: Create the Main scene

**Files:**
- Create: `src/Scenes/Core/Main.tscn`

- [ ] **Step 1: Create the scene file**

Create `src/Scenes/Core/Main.tscn`:

```
[gd_scene load_steps=2 format=3 uid="uid://main_scene"]

[ext_resource type="Script" path="res://Scripts/Core/Main.cs" id="1"]

[node name="Main" type="Control"]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
script = ExtResource("1")

[node name="Label" type="Label" parent="."]
layout_mode = 1
anchors_preset = 8
anchor_left = 0.5
anchor_top = 0.5
anchor_right = 0.5
anchor_bottom = 0.5
offset_left = -80.0
offset_top = -12.0
offset_right = 80.0
offset_bottom = 12.0
grow_horizontal = 2
grow_vertical = 2
text = "Path of Chaz"
horizontal_alignment = 1
vertical_alignment = 1
```

- [ ] **Step 2: Commit**

```bash
jj commit -m "scaffold: add Main.tscn scene with centered label"
```

---

### Task 5: Create the Godot project file

**Files:**
- Create: `src/project.godot`

- [ ] **Step 1: Create project.godot**

Create `src/project.godot`:

```ini
; Engine configuration file.
; It's best edited using the editor UI and not directly,
; but it can also be manually edited.

config_version=5

[application]

config/name="Path of Chaz"
run/main_scene="res://Scenes/Core/Main.tscn"
config/features=PackedStringArray("4.4", "C#", "Forward Plus")

[display]

window/size/viewport_width=1280
window/size/viewport_height=720
window/stretch/mode="viewport"

[dotnet]

project/assembly_name="PathOfChaz"
```

- [ ] **Step 2: Commit**

```bash
jj commit -m "scaffold: add project.godot with C# and pixel-art config"
```

---

### Task 6: Create empty directory structure

**Files:**
- Create: `.gitkeep` files in all placeholder directories

- [ ] **Step 1: Create all placeholder directories with .gitkeep files**

```bash
for dir in \
  src/Scenes/UI \
  src/Scenes/World \
  src/Scripts/Characters \
  src/Scripts/Items \
  src/Scripts/World \
  src/Scripts/UI \
  src/Resources \
  src/Assets/Sprites \
  src/Assets/Audio \
  src/Assets/Fonts \
  src/Addons; do
  mkdir -p "$dir"
  touch "$dir/.gitkeep"
done
```

- [ ] **Step 2: Commit**

```bash
jj commit -m "scaffold: add empty directory structure per architecture spec"
```

---

### Task 7: Verify everything works

- [ ] **Step 1: Clean build from repo root**

Run: `dotnet build`

Expected: Build succeeded, 0 errors.

- [ ] **Step 2: Open in Godot**

Run: `godot-mono --path src/ --editor`

Expected: Godot opens with the project. Main.tscn is the main scene.

- [ ] **Step 3: Run the project**

Run from Godot editor (F5) or: `godot-mono --path src/`

Expected: Window opens showing "Path of Chaz" label centered. Godot output console shows "Path of Chaz is alive".

- [ ] **Step 4: Final commit if any fixups were needed**

```bash
jj commit -m "scaffold: fixups from verification"
```
