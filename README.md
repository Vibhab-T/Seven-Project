---

# Wave Function Collapse (WFC) Implementation in Unity (Tile-Based Approach).

*A procedural tile-based map generator using the Wave Function Collapse algorithm in Unity.*

---

## Overview

This project implements a Wave Function Collapse (WFC) algorithm for procedural map generation in Unity. It generates tile-based maps by respecting tile socket compatibility to ensure seamless connections between terrain types such as roads, grass, and crossings.

---

## Attribution

This project is inspired by and based on the **Wave Function Collapse (WFC)** algorithm originally created by **Maxim Gumin**.

- Original repository: [https://github.com/mxgmn/WaveFunctionCollapse](https://github.com/mxgmn/WaveFunctionCollapse)
- Original article and explanation: [https://github.com/mxgmn/WaveFunctionCollapse/blob/master/README.md](https://github.com/mxgmn/WaveFunctionCollapse/blob/master/README.md)

The Unity implementation and modifications in this project are my own.

---

## Features

- Procedural generation of maps with variable width and height.
- Tile compatibility defined via socket types for top, bottom, left, and right sides.
- Supports multiple tile types including grass, horizontal and vertical roads, connectors, and crossings.
- Ensures valid tile placement by propagating constraints during map generation.
- Prevents invalid tile connections to maintain natural map layouts.
- Expandable system easily allowing new tile prefabs and socket definitions.

---

## How It Works

- Each grid cell holds a list of possible tile prefabs (options).
- Tiles collapse one by one, starting from a random cell.
- Collapsing selects one tile randomly from possible options and removes other options.
- Constraints propagate to neighbors to filter their tile options, maintaining socket compatibility.
- The process repeats until all cells are collapsed with exactly one tile.

---

## Socket Definitions

Each tile prefab has socket definitions indicating which socket types it can connect to on each side.

    CrossingHorizontal:
        Top:Grass,
        Bottom:Grass,
        Left:Road Horizontal,
        Right:Road Horizontal

    CrossingVertical:
        Top:Road Vertical,
        Bottom:Road Vertical,
        Left:Grass,
        Right:Grass

    grass_1:
        Top:Grass
        Bottom:Grass
        Left:Grass
        Right:Grass

    grass_2:
        Top:Grass
        Bottom:Grass
        Left:Grass
        Right:Grass

    grass_3:
        Top:Grass
        Bottom:Grass
        Left:Grass
        Right:Grass

    RoadHorizontal:
        Top:Grass,
        Bottom:Grass,
        Left:Road Horizontal, Connectors, Crossing
        Right:Road Horizontal, Connectors, Crossing

    RoadLeftBottom:
        Top:Grass,
        Bottom:Road Vertical
        Left:Road Horizontal
        Right:Grass

    RoadRightBottom:
        Top:Grass,
        Bottom:Road Vertical
        Left:Grass
        Right:Road Horizontal

    RoadTopLeft:
        Top:Road Vertical
        Bottom: Grass
        Left: Road Horizontal
        Right: Grass

    RoadTopRight:
        Top:Road Vertical
        Bottom: Grass
        Left: Grass
        Right: Road Horizontal

    RoadVertical:
        Top:Road Vertical, Connectors, Crossing
        Bottom: Road Vertical, Connectors, Crossing
        Left: Grass
        Right: Grass

---

## Getting Started

### Prerequisites

- Unity 2021.3 or later recommended.
- Basic knowledge of Unity Editor and C# scripting.

### Installation

1. Clone or download this repository.
2. Open the project in Unity.
3. Import your tile prefabs into the `allTilePrefabs` list in the `MapGenerator` component.
4. Configure grid size and tile size as needed.

### Usage

- Attach the `MapGenerator` script to an empty GameObject.
- Assign your tile prefabs.
- Run the scene to generate the map.

---

## Input Tiles

![Input Tiles](Assets/Sample%20IO/inputs.png)

---

## Output Variations

![Output](Assets/Sample%20IO/outputs.png)
![WFC Example](<Assets/Sample IO/WFC MAP EXAMPLE 2.png>)
![WFC Example](<Assets/Sample IO/WFC MAP EXAMPLE 3.png>)
![WFC Example](<Assets/Sample IO/WFC MAP EXAMPLE.png>)

---

## Future Improvements

- Loop prevention to avoid circular road networks.
- Integration of weighted tiles for more natural distributions.
- Visual debugging tools for socket compatibility.

---

Thank you for checking out this project!  
Feel free to contribute, report issues, or suggest improvements.

---
