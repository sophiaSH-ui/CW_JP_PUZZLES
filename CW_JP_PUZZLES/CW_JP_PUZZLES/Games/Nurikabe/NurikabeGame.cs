using System.Collections.Generic;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core;
using CW_JP_PUZZLES.Core.Cells;

namespace CW_JP_PUZZLES.Games.Nurikabe
{
    public class NurikabeGame : PuzzleBase
    {
        private NurikabeCell[,] _grid = null!;
        private readonly NurikabeSolver _solver = new();
        private readonly NurikabeGenerator _generator = new();

        public NurikabeCell[,] Grid => _grid;

        public override void GenerateField(int size, Difficulty difficulty)
        {
            Size = size;
            MoveCount = 0;
            Timer.Reset();
            _grid = _generator.Generate(size, difficulty);
            Timer.Start();
        }

        public override bool MakeMove(int x, int y, object? moveData = null)
        {
            if (!IsInBounds(x, y)) return false;

            var cell = _grid[x, y];
            if (cell.IsLocked) return false;
            if (cell.ClueValue > 0) return false;

            string action = moveData as string ?? "black";

            if (action == "black")
                cell.IsBlack = !cell.IsBlack;
            else if (action == "white")
                cell.IsBlack = false;

            MoveCount++;
            RecalculateIslands();
            return true;
        }

        public override bool IsGameOver()
        {
            if (!_solver.IsValid(_grid)) return false;
            Timer.Stop();
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    _grid[x, y].Reset();
            RecalculateIslands();
            Timer.Start();
        }

        private void RecalculateIslands()
        {
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (!_grid[x, y].IsBlack)
                        _grid[x, y].IslandId = -1;

            int islandId = 0;
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    var cell = _grid[x, y];
                    if (cell.ClueValue <= 0 || cell.IsBlack) continue;
                    if (cell.IslandId >= 0) continue;

                    var queue = new Queue<(int, int)>();
                    queue.Enqueue((x, y));
                    cell.IslandId = islandId;

                    while (queue.Count > 0)
                    {
                        var (cx, cy) = queue.Dequeue();
                        foreach (var (nx, ny) in GridManager.GetNeighbors(cx, cy))
                        {
                            if (!IsInBounds(nx, ny)) continue;
                            var neighbor = _grid[nx, ny];
                            if (neighbor.IsBlack || neighbor.IslandId >= 0) continue;
                            neighbor.IslandId = islandId;
                            queue.Enqueue((nx, ny));
                        }
                    }

                    islandId++;
                }
        }

        private bool IsInBounds(int x, int y) =>
            GridManager.IsInBounds(x, y, Size, Size);
    }
}