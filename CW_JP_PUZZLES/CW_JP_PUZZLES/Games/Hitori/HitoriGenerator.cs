using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            HitoriCell[,] field;
            int attempts = 0;

            do
            {
                field = BuildPuzzle(size, difficulty);
                attempts++;
            }
            while (!_solver.HasUniqueSolution(field) && attempts < 100);

            return field;
        }

        private HitoriCell[,] BuildPuzzle(int size, Difficulty difficulty)
        {
            double blackRatio = difficulty switch
            {
                Difficulty.Easy => 0.12,
                //Difficulty.Medium => 0.18,
                Difficulty.Hard => 0.24
            };

            var field = new HitoriCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y] = new HitoriCell { X = x, Y = y };

            var blackMask = PlaceBlacks(size, blackRatio);
            
            FillValues(field, blackMask, size);

            return field;
        }

        private bool[,] PlaceBlacks(int size, double ratio)
        {
            var mask = new bool[size, size];
            int target = (int)(size * size * ratio);
            int placed = 0;

            var positions = Enumerable.Range(0, size * size)
                                      .OrderBy(_ => _rng.Next())
                                      .ToList();

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

        private void FillValues(HitoriCell[,] field, bool[,] blackMask, int size)
        {
            for (int x = 0; x < size; x++)
            {
                var nums = Enumerable.Range(1, size).OrderBy(_ => _rng.Next()).ToList();
                for (int y = 0; y < size; y++)
                    field[x, y].Value = nums[y];
            }

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (!blackMask[x, y]) continue;

                    var rowVals = Enumerable.Range(0, size)
                        .Where(j => j != y && !blackMask[x, j])
                        .Select(j => field[x, j].Value)
                        .ToList();

                    if (rowVals.Count > 0)
                        field[x, y].Value = rowVals[_rng.Next(rowVals.Count)];
                }
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
    }
}