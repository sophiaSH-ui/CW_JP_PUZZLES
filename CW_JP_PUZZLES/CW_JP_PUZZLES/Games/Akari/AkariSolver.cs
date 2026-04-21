using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Akari
{
    public class AkariSolver : ISolver<AkariCell>
    {
        public bool IsValid(AkariCell[,] field)
        {
            int size = field.GetLength(0);

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    var cell = field[x, y];

                    if (cell.WallNumber >= 0)
                    {
                        int bulbs = CountBulbsAround(field, x, y, size);
                        if (bulbs != cell.WallNumber) return false;
                    }

                    if (cell.HasBulb && IsConflicting(field, x, y, size))
                        return false;
                }

            return true;
        }

        public bool HasUniqueSolution(AkariCell[,] field)
        {
            int size = field.GetLength(0);
            int count = 0;
            Solve(field, size, 0, 0, ref count);
            return count == 1;
        }

        private void Solve(AkariCell[,] field, int size, int startX, int startY, ref int count)
        {
            if (count > 1) return; 

            for (int x = startX; x < size; x++)
            {
                int yStart = (x == startX) ? startY : 0;
                for (int y = yStart; y < size; y++)
                {
                    var cell = field[x, y];
                    if (cell.IsLocked || cell.WallNumber >= 0) continue;
                    if (cell.HasBulb) continue; 

                    cell.HasBulb = true;
                    if (IsPartiallyValid(field, size, x, y))
                        Solve(field, size, x, y + 1, ref count);
                    cell.HasBulb = false;

                    Solve(field, size, x, y + 1, ref count);
                    return;
                }
            }
            
            if (IsCompleteSolution(field, size))
                count++;
        }

        private bool IsPartiallyValid(AkariCell[,] field, int size, int x, int y)
        {
            if (IsConflicting(field, size: size, x: x, y: y)) return false;

            foreach (var (nx, ny) in GetNeighbors(x, y))
            {
                if (!InBounds(nx, ny, size)) continue;
                var wall = field[nx, ny];
                if (wall.WallNumber < 0) continue;
                if (CountBulbsAround(field, nx, ny, size) > wall.WallNumber) return false;
            }

            return true;
        }

        private bool IsCompleteSolution(AkariCell[,] field, int size)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    var cell = field[x, y];
                    if (cell.WallNumber >= 0)
                    {
                        if (CountBulbsAround(field, x, y, size) != cell.WallNumber) return false;
                    }
                    else if (!cell.IsLocked)
                    {
                        if (!cell.IsIlluminated && !cell.HasBulb) return false;
                    }
                }
            return !HasConflicts(field, size);
        }

        private bool HasConflicts(AkariCell[,] field, int size)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (field[x, y].HasBulb && IsConflicting(field, x, y, size))
                        return true;
            return false;
        }

        private bool IsConflicting(AkariCell[,] field, int x, int y, int size)
        {
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int dir = 0; dir < 4; dir++)
            {
                int nx = x + dx[dir], ny = y + dy[dir];
                while (InBounds(nx, ny, size) && field[nx, ny].WallNumber < 0 && !field[nx, ny].IsLocked)
                {
                    if (field[nx, ny].HasBulb) return true;
                    nx += dx[dir]; ny += dy[dir];
                }
            }
            return false;
        }

        private int CountBulbsAround(AkariCell[,] field, int x, int y, int size)
        {
            int count = 0;
            foreach (var (nx, ny) in GetNeighbors(x, y))
                if (InBounds(nx, ny, size) && field[nx, ny].HasBulb)
                    count++;
            return count;
        }

        private static bool InBounds(int x, int y, int size) =>
            x >= 0 && x < size && y >= 0 && y < size;

        private static (int, int)[] GetNeighbors(int x, int y) =>
            new[] { (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y) };
    }
}