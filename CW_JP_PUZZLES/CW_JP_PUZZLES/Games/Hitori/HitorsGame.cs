using System;
using System.Collections.Generic;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core;
using CW_JP_PUZZLES.Core.Cells;

namespace CW_JP_PUZZLES.Games.Hitori
{
    public class HitoriGame : PuzzleBase
    {
        private HitoriCell[,] _grid = null!;
        private readonly HitoriSolver _solver = new();
        private readonly HitoriGenerator _generator = new();

        public HitoriCell[,] Grid => _grid;

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

            string action = moveData as string ?? "black";

            if (action == "black")
            {
                if (cell.IsCircled) cell.IsCircled = false;
                cell.IsBlackened = !cell.IsBlackened;
            }
            else if (action == "circle")
            {
                if (cell.IsBlackened) cell.IsBlackened = false;
                cell.IsCircled = !cell.IsCircled;
            }

            MoveCount++;
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

            Timer.Start();
        }

        private bool IsInBounds(int x, int y) =>
            GridManager.IsInBounds(x, y, Size, Size);
    }
}