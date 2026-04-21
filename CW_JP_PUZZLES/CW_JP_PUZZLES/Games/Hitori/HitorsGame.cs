using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public override string GetHint()
        {
            for (int x = 0; x < Size; x++)
            {
                var duplicates = FindDuplicatesInRow(x);
                if (duplicates.Count > 0)
                    return $"Рядок {x + 1} має повторювані числа — спробуй зафарбувати одне з них.";
            }

            for (int y = 0; y < Size; y++)
            {
                var duplicates = FindDuplicatesInCol(y);
                if (duplicates.Count > 0)
                    return $"Стовпець {y + 1} має повторювані числа — спробуй зафарбувати одне з них.";
            }

            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    if (!_grid[x, y].IsBlackened) continue;
                    foreach (var (nx, ny) in GridManager.GetNeighbors(x, y))
                        if (IsInBounds(nx, ny) && _grid[nx, ny].IsBlackened)
                            return $"Клітинки ({x + 1},{y + 1}) та ({nx + 1},{ny + 1}) — дві чорні поруч, це заборонено.";
                }

            return "Виглядає добре! Перевір зв'язність білих клітинок.";
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

        private List<int> FindDuplicatesInRow(int x)
        {
            var seen = new HashSet<int>();
            var duplicates = new List<int>();
            for (int y = 0; y < Size; y++)
            {
                var cell = _grid[x, y];
                if (cell.IsBlackened) continue;
                if (!seen.Add(cell.Value)) duplicates.Add(y);
            }
            return duplicates;
        }

        private List<int> FindDuplicatesInCol(int y)
        {
            var seen = new HashSet<int>();
            var duplicates = new List<int>();
            for (int x = 0; x < Size; x++)
            {
                var cell = _grid[x, y];
                if (cell.IsBlackened) continue;
                if (!seen.Add(cell.Value)) duplicates.Add(x);
            }
            return duplicates;
        }
    }
}
