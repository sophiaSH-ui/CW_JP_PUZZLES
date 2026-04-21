using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Shikaku
{
    public class ShikakuGenerator : IGenerator<ShikakuCell>
    {
        private readonly ShikakuSolver _solver = new();
        private readonly Random _rng = new();

        public ShikakuCell[,] Generate(int size, Difficulty difficulty)
        {
            ShikakuCell[,] field;
            int attempts = 0;

            do
            {
                field = BuildPuzzle(size, difficulty);
                attempts++;
            }
            while (!_solver.HasUniqueSolution(field) && attempts < 50);

            return field;
        }

        private ShikakuCell[,] BuildPuzzle(int size, Difficulty difficulty)
        {
            var field = new ShikakuCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y] = new ShikakuCell { X = x, Y = y };

            int targetRegions = difficulty switch
            {
                Difficulty.Easy => 6,
                Difficulty.Medium => 9,
                Difficulty.Hard => 12,
                _ => 14
            };

            bool success = PartitionField(field, size, targetRegions);
            if (!success) return field;

            PlaceCluesAndReset(field, size);

            return field;
        }

        private bool PartitionField(ShikakuCell[,] field, int size, int targetRegions)
        {
            var regions = new List<(int x1, int y1, int x2, int y2)>
            {
                (0, 0, size - 1, size - 1)
            };

            int regionId = 0;

            while (regions.Count < targetRegions)
            {
                var largest = regions
                    .Where(r => CanSplit(r))
                    .OrderByDescending(r => (r.x2 - r.x1 + 1) * (r.y2 - r.y1 + 1))
                    .FirstOrDefault();

                if (largest == default) break; 

                regions.Remove(largest);
                var (a, b) = SplitRegion(largest);
                regions.Add(a);
                regions.Add(b);
            }

            foreach (var (x1, y1, x2, y2) in regions)
            {
                for (int x = x1; x <= x2; x++)
                    for (int y = y1; y <= y2; y++)
                        field[x, y].RegionId = regionId;
                regionId++;
            }

            return true;
        }

        private bool CanSplit((int x1, int y1, int x2, int y2) r)
        {
            int w = r.x2 - r.x1 + 1;
            int h = r.y2 - r.y1 + 1;
            return w * h > 2;
        }

        private ((int, int, int, int), (int, int, int, int)) SplitRegion(
            (int x1, int y1, int x2, int y2) r)
        {
            int w = r.x2 - r.x1 + 1;
            int h = r.y2 - r.y1 + 1;

            if (w >= h && w > 1)
            {
                int cut = r.x1 + _rng.Next(1, w);
                return (
                    (r.x1, r.y1, cut - 1, r.y2),
                    (cut, r.y1, r.x2, r.y2)
                );
            }
            else
            {
                int cut = r.y1 + _rng.Next(1, h);
                return (
                    (r.x1, r.y1, r.x2, cut - 1),
                    (r.x1, cut, r.x2, r.y2)
                );
            }
        }

        private void PlaceCluesAndReset(ShikakuCell[,] field, int size)
        {
            var groups = new Dictionary<int, List<(int x, int y)>>();
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    int rid = field[x, y].RegionId;
                    if (!groups.ContainsKey(rid))
                        groups[rid] = new List<(int, int)>();
                    groups[rid].Add((x, y));
                }

            foreach (var (_, cells) in groups)
            {
                var (cx, cy) = cells[_rng.Next(cells.Count)];
                field[cx, cy].ClueValue = cells.Count;
            }

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y].RegionId = -1;
        }
    }
}