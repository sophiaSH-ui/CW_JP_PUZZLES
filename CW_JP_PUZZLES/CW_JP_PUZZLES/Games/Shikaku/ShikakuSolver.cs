using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Shikaku
{
    public class ShikakuSolver : ISolver<ShikakuCell>
    {
        public bool IsValid(ShikakuCell[,] field)
        {
            int size = field.GetLength(0);

            var regions = new Dictionary<int, List<(int x, int y)>>();

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    int rid = field[x, y].RegionId;
                    if (rid < 0) continue;
                    if (!regions.ContainsKey(rid))
                        regions[rid] = new List<(int, int)>();
                    regions[rid].Add((x, y));
                }

            foreach (var (rid, cells) in regions)
            {
                if (!IsRectangle(cells)) return false;

                int clueCount = 0;
                foreach (var (cx, cy) in cells)
                    if (field[cx, cy].ClueValue > 0) clueCount++;

                if (clueCount != 1) return false;

                var clue = cells.First(c => field[c.x, c.y].ClueValue > 0);
                if (field[clue.x, clue.y].ClueValue != cells.Count) return false;
            }

            return true;
        }

        public bool HasUniqueSolution(ShikakuCell[,] field)
        {
            int size = field.GetLength(0);

            var clues = new List<(int x, int y, int value)>();
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (field[x, y].ClueValue > 0)
                        clues.Add((x, y, field[x, y].ClueValue));

            var workField = CloneField(field, size);
            int count = 0;

            Solve(workField, size, clues, 0, ref count);
            return count == 1;
        }

        private void Solve(
            ShikakuCell[,] field, int size,
            List<(int x, int y, int value)> clues,
            int index, ref int count)
        {
            if (count > 1) return;

            if (index == clues.Count)
            {
                for (int x = 0; x < size; x++)
                    for (int y = 0; y < size; y++)
                        if (field[x, y].RegionId < 0) return;
                count++;
                return;
            }

            var (cx, cy, cv) = clues[index];

            if (field[cx, cy].RegionId >= 0)
            {
                Solve(field, size, clues, index + 1, ref count);
                return;
            }

            foreach (var rect in GetRectangles(cx, cy, cv, size))
            {
                var (rx1, ry1, rx2, ry2) = rect;

                if (!CanPlace(field, rx1, ry1, rx2, ry2)) continue;

                int regionId = index + 1;
                Place(field, rx1, ry1, rx2, ry2, regionId);
                Solve(field, size, clues, index + 1, ref count);
                Unplace(field, rx1, ry1, rx2, ry2);

                if (count > 1) return;
            }
        }

        private IEnumerable<(int x1, int y1, int x2, int y2)> GetRectangles(
            int cx, int cy, int area, int size)
        {
            for (int w = 1; w <= area; w++)
            {
                if (area % w != 0) continue;
                int h = area / w;

                for (int x1 = cx - w + 1; x1 <= cx; x1++)
                {
                    int x2 = x1 + w - 1;
                    if (x1 < 0 || x2 >= size) continue;

                    for (int y1 = cy - h + 1; y1 <= cy; y1++)
                    {
                        int y2 = y1 + h - 1;
                        if (y1 < 0 || y2 >= size) continue;

                        yield return (x1, y1, x2, y2);
                    }
                }
            }
        }

        private bool CanPlace(ShikakuCell[,] field, int x1, int y1, int x2, int y2)
        {
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    if (field[x, y].RegionId >= 0) return false;
            return true;
        }

        private void Place(ShikakuCell[,] field, int x1, int y1, int x2, int y2, int id)
        {
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    field[x, y].RegionId = id;
        }

        private void Unplace(ShikakuCell[,] field, int x1, int y1, int x2, int y2)
        {
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    field[x, y].RegionId = -1;
        }

        private bool IsRectangle(List<(int x, int y)> cells)
        {
            int minX = cells.Min(c => c.x), maxX = cells.Max(c => c.x);
            int minY = cells.Min(c => c.y), maxY = cells.Max(c => c.y);
            int expectedArea = (maxX - minX + 1) * (maxY - minY + 1);
            return cells.Count == expectedArea;
        }

        private ShikakuCell[,] CloneField(ShikakuCell[,] src, int size)
        {
            var clone = new ShikakuCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    clone[x, y] = new ShikakuCell
                    {
                        X = x,
                        Y = y,
                        ClueValue = src[x, y].ClueValue,
                        RegionId = -1
                    };
            return clone;
        }
    }
}