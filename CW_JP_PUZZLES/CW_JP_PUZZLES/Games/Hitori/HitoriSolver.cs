using System.Collections.Generic;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Hitori
{
    public class HitoriSolver : ISolver<HitoriCell>
    {
        public bool IsValid(HitoriCell[,] field)
        {
            int size = field.GetLength(0);

            return NoDuplicatesInLines(field, size)
                && NoAdjacentBlacks(field, size)
                && AllWhitesConnected(field, size);
        }

        public bool HasUniqueSolution(HitoriCell[,] field)
        {
            int size = field.GetLength(0);
            int count = 0;
            Solve(field, size, 0, ref count);
            return count == 1;
        }

        private void Solve(HitoriCell[,] field, int size, int pos, ref int count)
        {
            if (count > 1) return;

            if (pos == size * size)
            {
                if (IsValid(field)) count++;
                return;
            }

            int x = pos / size, y = pos % size;

            if (CanBeWhite(field, x, y, size))
            {
                field[x, y].IsBlackened = false;
                Solve(field, size, pos + 1, ref count);
            }

            if (!HasAdjacentBlack(field, x, y, size))
            {
                field[x, y].IsBlackened = true;
                Solve(field, size, pos + 1, ref count);
                field[x, y].IsBlackened = false; 
            }
        }

        private bool CanBeWhite(HitoriCell[,] field, int x, int y, int size)
        {
            int val = field[x, y].Value;

            for (int j = 0; j < y; j++)
                if (!field[x, j].IsBlackened && field[x, j].Value == val) return false;

            for (int i = 0; i < x; i++)
                if (!field[i, y].IsBlackened && field[i, y].Value == val) return false;

            return true;
        }

        private bool NoDuplicatesInLines(HitoriCell[,] field, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (!IsLineUnique(field, size, i, true)) return false;
                if (!IsLineUnique(field, size, i, false)) return false;
            }
            return true;
        }

        private bool IsLineUnique(HitoriCell[,] field, int size, int index, bool isRow)
        {
            var seen = new HashSet<int>();
            for (int j = 0; j < size; j++)
            {
                var cell = isRow ? field[index, j] : field[j, index];
                if (cell.IsBlackened) continue;
                if (!seen.Add(cell.Value)) return false;
            }
            return true;
        }

        private bool NoAdjacentBlacks(HitoriCell[,] field, int size)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (!field[x, y].IsBlackened) continue;
                    if (HasAdjacentBlack(field, x, y, size)) return false;
                }
            return true;
        }

        private bool HasAdjacentBlack(HitoriCell[,] field, int x, int y, int size)
        {
            (int, int)[] neighbors = { (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y) };
            foreach (var (nx, ny) in neighbors)
                if (nx >= 0 && nx < size && ny >= 0 && ny < size && field[nx, ny].IsBlackened)
                    return true;
            return false;
        }

        private bool AllWhitesConnected(HitoriCell[,] field, int size)
        {
            (int sx, int sy) = (-1, -1);
            int totalWhite = 0;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (!field[x, y].IsBlackened)
                    {
                        totalWhite++;
                        if (sx == -1) { sx = x; sy = y; }
                    }

            if (totalWhite == 0) return false;

            var visited = new bool[size, size];
            var queue = new Queue<(int, int)>();
            queue.Enqueue((sx, sy));
            visited[sx, sy] = true;
            int reachable = 0;

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                reachable++;

                (int, int)[] neighbors = { (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y) };
                foreach (var (nx, ny) in neighbors)
                {
                    if (nx < 0 || nx >= size || ny < 0 || ny >= size) continue;
                    if (visited[nx, ny] || field[nx, ny].IsBlackened) continue;
                    visited[nx, ny] = true;
                    queue.Enqueue((nx, ny));
                }
            }

            return reachable == totalWhite;
        }
    }
}