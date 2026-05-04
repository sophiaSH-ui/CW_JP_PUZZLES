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

                    if (IsWall(cell))
                    {
                        if (cell.WallNumber >= 0)
                        {
                            int bulbs = CountBulbsAround(field, x, y, size);
                            if (bulbs != cell.WallNumber) return false;
                        }
                        continue;
                    }

                    if (cell.HasBulb && IsConflicting(field, x, y, size))
                        return false;
                }

            var illuminated = ComputeIllumination(field, size);

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (!IsWall(field[x, y]) && !illuminated[x, y])
                        return false;

            return true;
        }

        public bool HasUniqueSolution(AkariCell[,] field)
        {
            int size = field.GetLength(0);
            var clone = CloneField(field, size);
            int count = 0;
            Solve(clone, size, 0, ref count);
            return count == 1;
        }

        private void Solve(AkariCell[,] field, int size, int pos, ref int count)
        {
            if (count > 1) return;

            while (pos < size * size)
            {
                int x = pos / size, y = pos % size;
                if (!IsWall(field[x, y])) break;
                pos++;
            }

            if (pos == size * size)
            {
                if (IsSolution(field, size)) count++;
                return;
            }

            int cx = pos / size, cy = pos % size;

            field[cx, cy].HasBulb = true;
            if (IsPartiallyValid(field, size, cx, cy))
                Solve(field, size, pos + 1, ref count);

            field[cx, cy].HasBulb = false;
            if (count <= 1)
                Solve(field, size, pos + 1, ref count);
        }

        private bool IsPartiallyValid(AkariCell[,] field, int size, int x, int y)
        {
            if (IsConflicting(field, x, y, size)) return false;

            foreach (var (nx, ny) in GetNeighbors(x, y))
            {
                if (!InBounds(nx, ny, size)) continue;
                var wall = field[nx, ny];
                if (!IsWall(wall) || wall.WallNumber < 0) continue;
                if (CountBulbsAround(field, nx, ny, size) > wall.WallNumber)
                    return false;
            }

            return true;
        }

        private bool IsSolution(AkariCell[,] field, int size)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    var cell = field[x, y];
                    if (IsWall(cell))
                    {
                        if (cell.WallNumber >= 0 &&
                            CountBulbsAround(field, x, y, size) != cell.WallNumber)
                            return false;
                    }
                }

            if (HasConflicts(field, size)) return false;

            var illuminated = ComputeIllumination(field, size);
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (!IsWall(field[x, y]) && !illuminated[x, y])
                        return false;

            return true;
        }

        public bool[,] ComputeIllumination(AkariCell[,] field, int size)
        {
            var lit = new bool[size, size];
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (!field[x, y].HasBulb) continue;
                    lit[x, y] = true;

                    for (int dir = 0; dir < 4; dir++)
                    {
                        int nx = x + dx[dir], ny = y + dy[dir];
                        while (InBounds(nx, ny, size) && !IsWall(field[nx, ny]))
                        {
                            lit[nx, ny] = true;
                            nx += dx[dir];
                            ny += dy[dir];
                        }
                    }
                }

            return lit;
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
                while (InBounds(nx, ny, size) && !IsWall(field[nx, ny]))
                {
                    if (field[nx, ny].HasBulb) return true;
                    nx += dx[dir];
                    ny += dy[dir];
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

        private AkariCell[,] CloneField(AkariCell[,] src, int size)
        {
            var clone = new AkariCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    clone[x, y] = new AkariCell
                    {
                        X = x,
                        Y = y,
                        IsLocked = src[x, y].IsLocked,
                        WallNumber = src[x, y].WallNumber,
                        HasBulb = false
                    };
            return clone;
        }

        private static bool IsWall(AkariCell c) => c.IsLocked;
        private static bool InBounds(int x, int y, int size) =>
            x >= 0 && x < size && y >= 0 && y < size;
        private static (int, int)[] GetNeighbors(int x, int y) =>
            new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
    }
}