using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Akari
{
    public class AkariGame : PuzzleBase
    {
        private AkariCell[,] _grid = null!;
        private readonly AkariSolver _solver = new();
        private readonly AkariGenerator _generator = new();

        public AkariCell[,] Grid => _grid;
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
            if (cell.WallNumber >= 0) return false;   

            cell.HasBulb = !cell.HasBulb;
            MoveCount++;

            RecalculateIllumination();
            return true;
        }

        public override bool IsGameOver()
        {
            if (!_solver.IsValid(_grid)) return false;

            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    var cell = _grid[x, y];
                    if (cell.WallNumber < 0 && !cell.IsLocked)   
                        if (!cell.IsIlluminated && !cell.HasBulb)
                            return false;
                }

            Timer.Stop();
            return true;
        }

        public override string GetHint()
        {
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    var wall = _grid[x, y];
                    if (wall.WallNumber < 0) continue;

                    int bulbsAround = CountBulbsAround(x, y);
                    if (bulbsAround < wall.WallNumber)
                        return $"Стіна ({x},{y}) потребує {wall.WallNumber} лампочок, зараз {bulbsAround}.";
                }

            return "Спробуй пошукати незасвічену клітинку.";
        }

        public override void Reset()
        {
            base.Reset();
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    _grid[x, y].Reset();

            RecalculateIllumination();
            Timer.Start();
        }
        private bool IsInBounds(int x, int y) =>
            GridManager.IsInBounds(x, y, Size, Size);

        private void RecalculateIllumination()
        {
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    _grid[x, y].IsIlluminated = false;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                {
                    if (!_grid[x, y].HasBulb) continue;

                    _grid[x, y].IsIlluminated = true;

                    for (int dir = 0; dir < 4; dir++)
                    {
                        int nx = x + dx[dir];
                        int ny = y + dy[dir];

                        while (IsInBounds(nx, ny) && _grid[nx, ny].WallNumber < 0 && !_grid[nx, ny].IsLocked)
                        {
                            _grid[nx, ny].IsIlluminated = true;
                            nx += dx[dir];
                            ny += dy[dir];
                        }
                    }
                }
        }

        private int CountBulbsAround(int x, int y)
        {
            int count = 0;
            foreach (var (nx, ny) in GridManager.GetNeighbors(x, y))
                if (IsInBounds(nx, ny) && _grid[nx, ny].HasBulb)
                    count++;
            return count;
        }
    }
}