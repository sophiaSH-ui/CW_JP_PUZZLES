using System;
using System.Collections.Generic;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Shikaku
{
    public class ShikakuGame : PuzzleBase
    {
        private ShikakuCell[,] _grid = null!;
        private readonly ISolver<ShikakuCell> _solver;
        private readonly IGenerator<ShikakuCell> _generator;

        public ShikakuGame(ISolver<ShikakuCell> solver, IGenerator<ShikakuCell> generator)
        {
            _solver = solver;
            _generator = generator;
        }

        public ShikakuCell[,] Grid => _grid;

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
            if (moveData is not (int x2, int y2)) return false;
            if (!IsInBounds(x, y) || !IsInBounds(x2, y2)) return false;

            int x1 = Math.Min(x, x2), y1 = Math.Min(y, y2);
            x2 = Math.Max(x, x2); y2 = Math.Max(y, y2);

            int clueCount = 0;
            int clueValue = 0;
            int regionArea = (x2 - x1 + 1) * (y2 - y1 + 1);

            for (int rx = x1; rx <= x2; rx++)
                for (int ry = y1; ry <= y2; ry++)
                    if (_grid[rx, ry].ClueValue > 0)
                    {
                        clueCount++;
                        clueValue = _grid[rx, ry].ClueValue;
                    }

            if (clueCount != 1) return false;
            if (clueValue != regionArea) return false;

            for (int rx = x1; rx <= x2; rx++)
                for (int ry = y1; ry <= y2; ry++)
                    if (_grid[rx, ry].RegionId >= 0) return false;

            int newRegionId = GetNextRegionId(x1, y1, x2, y2);
            for (int rx = x1; rx <= x2; rx++)
                for (int ry = y1; ry <= y2; ry++)
                    _grid[rx, ry].RegionId = newRegionId;

            MoveCount++;
            return true;
        }

        public bool RemoveRegion(int x, int y)
        {
            if (!IsInBounds(x, y)) return false;
            int regionId = _grid[x, y].RegionId;
            if (regionId < 0) return false;

            for (int rx = 0; rx < Size; rx++)
                for (int ry = 0; ry < Size; ry++)
                    if (_grid[rx, ry].RegionId == regionId)
                        _grid[rx, ry].RegionId = -1;

            return true;
        }

        public override bool IsGameOver()
        {
            if (!_solver.IsValid(_grid)) return false;

            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (_grid[x, y].RegionId < 0) return false;

            Timer.Stop();
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    _grid[x, y].Reset();
            Timer.Start();
        }

        private bool IsInBounds(int x, int y) =>
            GridManager.IsInBounds(x, y, Size, Size);

        private int GetNextRegionId(int x1, int y1, int x2, int y2)
        {
            const int colorCount = 24;

            var neighborColors = new HashSet<int>();
            for (int rx = x1; rx <= x2; rx++)
                foreach (int ry in new[] { y1 - 1, y2 + 1 })
                {
                    if (!GridManager.IsInBounds(rx, ry, Size, Size)) continue;
                    int rid = _grid[rx, ry].RegionId;
                    if (rid >= 0) neighborColors.Add(rid % colorCount);
                }
            for (int ry = y1; ry <= y2; ry++)
                foreach (int rx in new[] { x1 - 1, x2 + 1 })
                {
                    if (!GridManager.IsInBounds(rx, ry, Size, Size)) continue;
                    int rid = _grid[rx, ry].RegionId;
                    if (rid >= 0) neighborColors.Add(rid % colorCount);
                }

            var usedColors = new HashSet<int>();
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (_grid[x, y].RegionId >= 0)
                        usedColors.Add(_grid[x, y].RegionId % colorCount);

            int targetColor = -1;
            for (int c = 0; c < colorCount; c++)
                if (!neighborColors.Contains(c) && !usedColors.Contains(c))
                { targetColor = c; break; }

            if (targetColor < 0)
                for (int c = 0; c < colorCount; c++)
                    if (!neighborColors.Contains(c))
                    { targetColor = c; break; }

            if (targetColor < 0) targetColor = 0;

            int maxId = -1;
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (_grid[x, y].RegionId > maxId)
                        maxId = _grid[x, y].RegionId;

            int nextId = maxId + 1;
            while (nextId % colorCount != targetColor) nextId++;
            return nextId;
        }

    }
}