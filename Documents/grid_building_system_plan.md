# Grid-Based Building System Plan (Unity)

## Overview

A lightweight, flexible grid-based building system for Unity, inspired by "Cult of the Lamb."  
**Key features:**
- 1x1 Unity unit grid, fixed size, fills a ground object's area (with MeshRenderer) in a 3D world.
- Buildings are not rotatable/flippable; each has a defined grid footprint.
- Visual grid appears in build mode; ghost object follows pointer, tints green/red for valid/invalid placement; left-click places building.
- Integrates with Unity’s standard input and UI systems. No Tilemap, lightweight.

---

## System Architecture

```mermaid
flowchart TD
    UI[UI: Build Mode Toggle, Building Selection]
    Input[Unity Input System]
    Ground[Ground Object (MeshRenderer)]
    GridSys[Grid System (Script)]
    Ghost[Ghost Object Controller]
    Validator[Placement Validator]
    Placer[Building Placer]
    Buildings[Placed Buildings (List/Array)]
    
    UI -- toggles --> GridSys
    Input -- pointer position --> Ghost
    Ground -- bounds --> GridSys
    GridSys -- grid data --> Ghost
    Ghost -- position, size --> Validator
    Validator -- valid/invalid --> Ghost
    Validator -- valid --> Placer
    Placer -- instantiate --> Buildings
    Buildings -- update grid --> GridSys
```

---

## Step-by-Step Implementation Plan

### 1. Grid System
- **Responsibility:**  
  Generate a 2D grid overlay matching the ground object's bounds. Track which grid cells are occupied.
- **Implementation:**  
  - On start, read the ground object's MeshRenderer bounds.
  - Calculate grid dimensions (width, height) based on bounds and 1x1 cell size.
  - Store grid as a 2D array (e.g., `bool[,] occupied`).
  - Provide methods: `IsAreaFree(x, y, w, h)`, `OccupyArea(x, y, w, h)`, `FreeArea(x, y, w, h)`.

### 2. Grid Visualizer
- **Responsibility:**  
  Draw grid lines or quads over the ground in build mode.
- **Implementation:**  
  - Use `LineRenderer`, `GL.Lines`, or lightweight mesh overlay.
  - Only visible in build mode.

### 3. Ghost Object Controller
- **Responsibility:**  
  Display a transparent/ghost version of the selected building, following the mouse pointer and snapping to the grid. Change tint to green/red based on placement validity.
- **Implementation:**  
  - Raycast from camera to ground to get mouse world position.
  - Snap position to nearest grid cell.
  - Query `GridSystem.IsAreaFree()` for the building’s footprint.
  - Change material color (green/red) accordingly.

### 4. Placement Validator
- **Responsibility:**  
  Check if the ghost object’s current grid area is valid for placement.
- **Implementation:**  
  - Use `GridSystem.IsAreaFree()` and ensure within grid bounds.

### 5. Building Placer
- **Responsibility:**  
  On left-click, if placement is valid, instantiate the building prefab at the snapped position and mark grid cells as occupied.
- **Implementation:**  
  - Update `GridSystem` occupancy.
  - Add building to a list for future reference (e.g., for removal).

### 6. UI Integration
- **Responsibility:**  
  Toggle build mode, select building type, display resource costs, etc.
- **Implementation:**  
  - Use Unity UI (Canvas, Buttons, etc.).
  - When build mode is active, enable grid and ghost object.

---

## Visual Example

- **Grid:** White/gray lines overlaying the ground.
- **Ghost Object:** Semi-transparent, green if valid, red if invalid.
- **Occupied Cells:** Not highlighted unless hovered.
- **UI:** Building selection, resource display, build/cancel buttons.

---

## Key Classes & Responsibilities

| Class/Script              | Responsibility                                      |
|---------------------------|-----------------------------------------------------|
| `GridSystem`              | Grid data, occupancy, bounds, utility methods       |
| `GridVisualizer`          | Draws grid overlay                                 |
| `GhostObjectController`   | Handles ghost object movement and tinting           |
| `PlacementValidator`      | Checks if placement is valid                        |
| `BuildingPlacer`          | Handles actual placement and grid updates           |
| `BuildModeUIController`   | UI for toggling build mode, selecting buildings     |

---

## Potential File Structure

```
Assets/Scripts/GridBuilding/
    GridSystem.cs
    GridVisualizer.cs
    GhostObjectController.cs
    PlacementValidator.cs
    BuildingPlacer.cs
    BuildModeUIController.cs
```

---

## Example: Grid System Pseudocode

```csharp
public class GridSystem : MonoBehaviour {
    public Vector3 origin;
    public int width, height;
    public float cellSize = 1f;
    private bool[,] occupied;

    public void Initialize(Bounds groundBounds) {
        // Calculate width/height from bounds
        // Initialize occupied array
    }

    public bool IsAreaFree(int x, int y, int w, int h) { ... }
    public void OccupyArea(int x, int y, int w, int h) { ... }
    public void FreeArea(int x, int y, int w, int h) { ... }
    public Vector3 GetWorldPosition(int x, int y) { ... }
    public Vector2Int GetGridPosition(Vector3 worldPos) { ... }
}
```

---

## Next Steps

1. Switch to code mode to begin implementation.
2. Integrate with Unity’s standard input and UI systems as described.