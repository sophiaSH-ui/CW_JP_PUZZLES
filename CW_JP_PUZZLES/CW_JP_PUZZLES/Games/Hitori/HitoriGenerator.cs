using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Hitori
{
    public class HitoriGenerator : IGenerator<HitoriCell>
    {
        private readonly HitoriSolver _solver = new();
        private readonly Random _rng = new();

        public HitoriCell[,] Generate(int size, Difficulty difficulty)
        {
            double blackRatio = difficulty switch
            {
                Difficulty.Easy => 0.15,
                Difficulty.Hard => 0.22,
                _ => 0.15
            };

            HitoriCell[,] field = BuildPuzzle(size, blackRatio);
            int attempts = 0;

            while (attempts < 5)
            {
                var testField = CloneField(field, size);
                var task = Task.Run(() => _solver.HasUniqueSolution(testField));

                if (task.Wait(TimeSpan.FromMilliseconds(200)))
                {
                    if (task.Result) return field;
                }

                field = BuildPuzzle(size, blackRatio);
                attempts++;
            }

            return field;
        }

        private HitoriCell[,] BuildPuzzle(int size, double blackRatio)
        {
            var field = new HitoriCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y] = new HitoriCell { X = x, Y = y };

            var blackMask = PlaceBlacks(size, blackRatio);
            FillValuesLatinSquare(field, blackMask, size);

            return field;
        }

        private void FillValuesLatinSquare(HitoriCell[,] field, bool[,] blackMask, int size)
        {
            int[,] grid = new int[size, size];
            var rowBase = Enumerable.Range(1, size).OrderBy(_ => _rng.Next()).ToList();

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    grid[i, j] = rowBase[(i + j) % size];

            var rowIndices = Enumerable.Range(0, size).OrderBy(_ => _rng.Next()).ToList();
            var colIndices = Enumerable.Range(0, size).OrderBy(_ => _rng.Next()).ToList();

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    field[i, j].Value = grid[rowIndices[i], colIndices[j]];

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (!blackMask[x, y]) continue;

                    bool useRow = _rng.Next(2) == 0;
                    if (useRow)
                    {
                        var rowVals = Enumerable.Range(0, size)
                            .Where(j => j != y && !blackMask[x, j])
                            .Select(j => field[x, j].Value).ToList();
                        if (rowVals.Count > 0) field[x, y].Value = rowVals[_rng.Next(rowVals.Count)];
                    }
                    else
                    {
                        var colVals = Enumerable.Range(0, size)
                            .Where(i => i != x && !blackMask[i, y])
                            .Select(i => field[i, y].Value).ToList();
                        if (colVals.Count > 0) field[x, y].Value = colVals[_rng.Next(colVals.Count)];
                    }
                }
        }

        private bool[,] PlaceBlacks(int size, double ratio)
        {
            var mask = new bool[size, size];
            int target = (int)(size * size * ratio);
            int placed = 0;

            var positions = Enumerable.Range(0, size * size).OrderBy(_ => _rng.Next()).ToList();

            foreach (int pos in positions)
            {
                if (placed >= target) break;

                int x = pos / size, y = pos % size;
                if (HasAdjacentBlack(mask, x, y, size)) continue;

                mask[x, y] = true;

                if (!WhitesConnected(mask, size))
                    mask[x, y] = false;
                else
                    placed++;
            }

            return mask;
        }

        private bool HasAdjacentBlack(bool[,] mask, int x, int y, int size)
        {
            (int, int)[] nb = { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
            foreach (var (nx, ny) in nb)
                if (nx >= 0 && nx < size && ny >= 0 && ny < size && mask[nx, ny])
                    return true;
            return false;
        }

        private bool WhitesConnected(bool[,] mask, int size)
        {
            int total = 0;
            (int sx, int sy) = (-1, -1);

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (!mask[x, y]) { total++; if (sx == -1) { sx = x; sy = y; } }

            if (total == 0) return false;

            var visited = new bool[size, size];
            var queue = new Queue<(int, int)>();
            queue.Enqueue((sx, sy));
            visited[sx, sy] = true;
            int count = 0;

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                count++;
                (int, int)[] nb = { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
                foreach (var (nx, ny) in nb)
                {
                    if (nx < 0 || nx >= size || ny < 0 || ny >= size) continue;
                    if (visited[nx, ny] || mask[nx, ny]) continue;
                    visited[nx, ny] = true;
                    queue.Enqueue((nx, ny));
                }
            }
            return count == total;
        }

        private HitoriCell[,] CloneField(HitoriCell[,] src, int size)
        {
            var clone = new HitoriCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    clone[x, y] = new HitoriCell { X = x, Y = y, Value = src[x, y].Value };
            return clone;
        }
    }
}