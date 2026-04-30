using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Nurikabe
{
    public class NurikabeSolver : ISolver<NurikabeCell>
    {
        public bool IsValid(NurikabeCell[,] field)
        {
            int size = field.GetLength(0);

            return IslandSizesCorrect(field, size)
                && IslandsNotTouching(field, size)
                && RiverConnected(field, size)
                && NoTwoByTwoBlack(field, size);
        }

        public bool HasUniqueSolution(NurikabeCell[,] field)
        {
            int size = field.GetLength(0);
            var clone = CloneField(field, size);
            int count = 0;
            Solve(clone, size, 0, ref count);
            return count == 1;
        }

        private void Solve(NurikabeCell[,] field, int size, int pos, ref int count)
        {
            if (count > 1) return;

            if (pos == size * size)
            {
                if (IsValid(field)) count++;
                return;
            }

            int x = pos / size, y = pos % size;
            var cell = field[x, y];

            if (cell.ClueValue > 0 || cell.IsLocked)
            {
                Solve(field, size, pos + 1, ref count);
                return;
            }

            cell.IsBlack = false;
            if (!IsPartiallyInvalid(field, size, x, y))
                Solve(field, size, pos + 1, ref count);

            if (count > 1) return;

            cell.IsBlack = true;
            if (!IsPartiallyInvalid(field, size, x, y))
                Solve(field, size, pos + 1, ref count);

            cell.IsBlack = false; 
        }

        private bool IsPartiallyInvalid(NurikabeCell[,] field, int size, int x, int y)
        {
            for (int dx = -1; dx <= 0; dx++)
                for (int dy = -1; dy <= 0; dy++)
                {
                    int x1 = x + dx, y1 = y + dy;
                    if (x1 < 0 || x1 + 1 >= size || y1 < 0 || y1 + 1 >= size) continue;
                    if (field[x1, y1].IsBlack && field[x1 + 1, y1].IsBlack &&
                        field[x1, y1 + 1].IsBlack && field[x1 + 1, y1 + 1].IsBlack)
                        return true;
                }

            return false;
        }

        private bool IslandSizesCorrect(NurikabeCell[,] field, int size)
        {
            var visited = new bool[size, size];

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (field[x, y].IsBlack || visited[x, y]) continue;
                    if (field[x, y].ClueValue <= 0) continue;

                    int clue = field[x, y].ClueValue;
                    int islandSize = 0;
                    int clueCount = 0;

                    var queue = new Queue<(int, int)>();
                    queue.Enqueue((x, y));
                    visited[x, y] = true;

                    while (queue.Count > 0)
                    {
                        var (cx, cy) = queue.Dequeue();
                        islandSize++;
                        if (field[cx, cy].ClueValue > 0) clueCount++;

                        foreach (var (nx, ny) in Neighbors(cx, cy))
                        {
                            if (!InBounds(nx, ny, size)) continue;
                            if (visited[nx, ny] || field[nx, ny].IsBlack) continue;
                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                        }
                    }

                    if (clueCount != 1) return false;
                    if (islandSize != clue) return false;
                }

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (!field[x, y].IsBlack && !visited[x, y])
                        return false;

            return true;
        }

        private bool IslandsNotTouching(NurikabeCell[,] field, int size)
        {
            var islandMap = new int[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    islandMap[i, j] = -1;

            int id = 0;
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (field[x, y].IsBlack || islandMap[x, y] >= 0) continue;
                    if (field[x, y].ClueValue <= 0) continue;

                    var queue = new Queue<(int, int)>();
                    queue.Enqueue((x, y));
                    islandMap[x, y] = id;

                    while (queue.Count > 0)
                    {
                        var (cx, cy) = queue.Dequeue();
                        foreach (var (nx, ny) in Neighbors(cx, cy))
                        {
                            if (!InBounds(nx, ny, size)) continue;
                            if (field[nx, ny].IsBlack || islandMap[nx, ny] >= 0) continue;
                            islandMap[nx, ny] = id;
                            queue.Enqueue((nx, ny));
                        }
                    }
                    id++;
                }

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (field[x, y].IsBlack || islandMap[x, y] < 0) continue;
                    foreach (var (nx, ny) in Neighbors(x, y))
                    {
                        if (!InBounds(nx, ny, size)) continue;
                        if (field[nx, ny].IsBlack) continue;
                        if (islandMap[nx, ny] >= 0 && islandMap[nx, ny] != islandMap[x, y])
                            return false;
                    }
                }

            return true;
        }

        private bool RiverConnected(NurikabeCell[,] field, int size)
        {
            (int sx, int sy) = (-1, -1);
            int totalBlack = 0;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (field[x, y].IsBlack)
                    {
                        totalBlack++;
                        if (sx == -1) { sx = x; sy = y; }
                    }

            if (totalBlack == 0) return true;

            var visited = new bool[size, size];
            var queue = new Queue<(int, int)>();
            queue.Enqueue((sx, sy));
            visited[sx, sy] = true;
            int reached = 0;

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                reached++;
                foreach (var (nx, ny) in Neighbors(x, y))
                {
                    if (!InBounds(nx, ny, size)) continue;
                    if (visited[nx, ny] || !field[nx, ny].IsBlack) continue;
                    visited[nx, ny] = true;
                    queue.Enqueue((nx, ny));
                }
            }

            return reached == totalBlack;
        }

        private bool NoTwoByTwoBlack(NurikabeCell[,] field, int size)
        {
            for (int x = 0; x < size - 1; x++)
                for (int y = 0; y < size - 1; y++)
                    if (field[x, y].IsBlack && field[x + 1, y].IsBlack &&
                        field[x, y + 1].IsBlack && field[x + 1, y + 1].IsBlack)
                        return false;
            return true;
        }

        private NurikabeCell[,] CloneField(NurikabeCell[,] src, int size)
        {
            var clone = new NurikabeCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    clone[x, y] = new NurikabeCell
                    {
                        X = x,
                        Y = y,
                        ClueValue = src[x, y].ClueValue,
                        IsBlack = src[x, y].IsBlack,
                        IsLocked = src[x, y].IsLocked
                    };
            return clone;
        }

        private static (int, int)[] Neighbors(int x, int y) =>
            new[] { (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y) };

        private static bool InBounds(int x, int y, int size) =>
            x >= 0 && x < size && y >= 0 && y < size;
    }
}