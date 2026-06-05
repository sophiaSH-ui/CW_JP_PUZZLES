# 🧩 Japanese Logic Puzzles Collection

A beautiful, desktop-based puzzle application built with C# and WPF. This project serves as a comprehensive collection of classic Japanese logic puzzles, featuring clean architecture, procedurally generated levels, and an intuitive user interface.

## ✨ Features

*   **Procedural Level Generation**: Never play the same puzzle twice! All grids are mathematically generated on the fly with guaranteed unique solutions.
*   **Multiple Difficulty Levels**: Choose between various grid sizes and difficulty parameters to match your skill level.
*   **Real-time Validation**: The game dynamically checks your moves and prevents illegal placements according to the rules of each specific puzzle.
*   **Modern UI/UX**: A clean, responsive, and aesthetically pleasing interface built with WPF, featuring animations and a unified design language.
*   **Save System**: User preferences (like language and volume) are automatically saved and restored using XML configuration.

## 🎮 Included Games

1.  **💡 Akari (Light Up)**
    *   Place lightbulbs to illuminate the entire grid.
    *   Bulbs cannot shine on each other, and numbered black cells dictate exactly how many bulbs must be placed adjacently.
2.  **⬛ Hitori**
    *   Eliminate duplicate numbers in rows and columns by painting them black.
    *   Black cells cannot touch horizontally or vertically, and all remaining white cells must form a single continuous area.
3.  **🏝️ Nurikabe**
    *   Form islands of white cells where each island contains exactly one number, which dictates its size.
    *   All black cells (the "river") must be connected, and no 2x2 blocks of black cells are allowed.
4.  **🔲 Shikaku**
    *   Divide the grid into rectangular or square pieces.
    *   Each rectangle must contain exactly one number, which represents the area (number of cells) of that rectangle.

## 🛠️ Tech Stack & Architecture

*   **Language**: C#
*   **Framework**: .NET 9 / WPF (Windows Presentation Foundation)
*   **Architecture Pattern**: MVVM (Model-View-ViewModel)

The project is strictly designed following object-oriented programming (OOP) principles. It leverages interfaces (`IPuzzleGame`, `IGenerator`, `ISolver`) and abstract base classes (`PuzzleBase`) to ensure that the core business logic remains entirely decoupled from the UI layer.

## 🚀 Getting Started

1.  Clone the repository.
2.  Open the solution `.sln` in **Visual Studio 2022** (or later).
3.  Ensure you have the `.NET Desktop Development` workload installed.
4.  Build and Run the project (F5).

---
*Developed as an Object-Oriented Programming coursework project.*
