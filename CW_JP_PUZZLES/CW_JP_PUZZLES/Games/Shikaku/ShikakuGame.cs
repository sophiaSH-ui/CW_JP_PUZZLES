using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core;
using CW_JP_PUZZLES.Core.Cells;

namespace CW_JP_PUZZLES.Games.Shikaku
{
    public class ShikakuGame : PuzzleBase
    {
        private ShikakuCell[,] _grid = null!;
        private readonly ShikakuSolver _solver = new();
        private readonly ShikakuGenerator _generator = new();

        public ShikakuCell[,] Grid => _grid;

        public override void GenerateField(int size, Difficulty difficulty)
        {
            Size = size;
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

            int newRegionId = GetNextRegionId();
            for (int rx = x1; rx <= x2; rx++)
                for (int ry = y1; ry <= y2; ry++)
                    if (_grid[rx, ry].RegionId >= 0) return false;

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

        public override string GetHint()
        {
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    var cell = _grid[x, y];
                    if (cell.ClueValue > 0 && cell.RegionId < 0)
                        return $"Підказка ({x + 1},{y + 1}) зі значенням {cell.ClueValue} ще не має прямокутника.";
                }

            return "Всі підказки покриті — перевір правильність прямокутників.";
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

        private int GetNextRegionId()
        {
            int max = -1;
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    if (_grid[x, y].RegionId > max)
                        max = _grid[x, y].RegionId;
            return max + 1;
        }
    }
}