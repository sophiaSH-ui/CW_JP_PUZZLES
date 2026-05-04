using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Akari
{
    public class AkariGenerator : IGenerator<AkariCell>
    {
        private readonly AkariSolver _solver = new();
        private readonly Random _rng = new();

        public AkariCell[,] Generate(int size, Difficulty difficulty)
        {
            (double wallRatio, double hintRatio) = difficulty switch
            {
                Difficulty.Easy => (0.20, 0.90),
                Difficulty.Hard => (0.24, 0.70),
                _ => (0.20, 0.90)
            };

            AkariCell[,] field = BuildField(size, wallRatio, hintRatio);
            int attempts = 0;

            while (attempts < 5)
            {
                var testField = field;
                var task = Task.Run(() => _solver.HasUniqueSolution(testField));

                if (task.Wait(TimeSpan.FromMilliseconds(150)))
                {
                    if (task.Result) return field;
                }

                field = BuildField(size, wallRatio, hintRatio);
                attempts++;
            }

            return field;
        }

        private AkariCell[,] BuildField(int size, double wallRatio, double hintRatio)
        {
            var field = new AkariCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y] = new AkariCell { X = x, Y = y, WallNumber = -1 };

            PlaceRandomWalls(field, size, wallRatio);
            PlaceSolutionGreedy(field, size);
            AssignWallNumbers(field, size, hintRatio);

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (!field[x, y].IsLocked)
                        field[x, y].HasBulb = false;

            return field;
        }

        private void PlaceSolutionGreedy(AkariCell[,] field, int size)
        {
            var positions = Enumerable.Range(0, size * size).OrderBy(_ => _rng.Next()).ToList();

            foreach (int pos in positions)
            {
                int x = pos / size;
                int y = pos % size;

                if (field[x, y].IsLocked) continue;
                if (IsIlluminated(field, x, y, size)) continue;

                field[x, y].HasBulb = true;
            }
        }

        private bool IsIlluminated(AkariCell[,] field, int cx, int cy, int size)
        {
            if (field[cx, cy].HasBulb) return true;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int dir = 0; dir < 4; dir++)
            {
                int nx = cx + dx[dir], ny = cy + dy[dir];
                while (InBounds(nx, ny, size) && !field[nx, ny].IsLocked)
                {
                    if (field[nx, ny].HasBulb) return true;
                    nx += dx[dir];
                    ny += dy[dir];
                }
            }
            return false;
        }

        private void PlaceRandomWalls(AkariCell[,] field, int size, double wallRatio)
        {
            int totalCells = size * size;
            int targetWallCount = (int)(totalCells * wallRatio);
            int currentWallCount = 0;

            var availablePositions = Enumerable.Range(0, totalCells / 2 + (totalCells % 2))
                                                .OrderBy(_ => _rng.Next())
                                                .ToList();

            foreach (int pos in availablePositions)
            {
                if (currentWallCount >= targetWallCount) break;

                int x1 = pos / size;
                int y1 = pos % size;

                int x2 = size - 1 - x1;
                int y2 = size - 1 - y1;

                if (!field[x1, y1].IsLocked)
                {
                    if (Forms2x2Block(field, x1, y1, size)) continue;

                    field[x1, y1].IsLocked = true;

                    if (x1 != x2 || y1 != y2)
                    {
                        if (Forms2x2Block(field, x2, y2, size))
                        {
                            field[x1, y1].IsLocked = false;
                            continue;
                        }

                        field[x2, y2].IsLocked = true;
                        currentWallCount += 2;
                    }
                    else
                    {
                        currentWallCount++;
                    }
                }
            }
        }

        private bool Forms2x2Block(AkariCell[,] field, int x, int y, int size)
        {
            if (x > 0 && y > 0 && field[x - 1, y - 1].IsLocked && field[x - 1, y].IsLocked && field[x, y - 1].IsLocked) return true;
            if (x > 0 && y < size - 1 && field[x - 1, y + 1].IsLocked && field[x - 1, y].IsLocked && field[x, y + 1].IsLocked) return true;
            if (x < size - 1 && y > 0 && field[x + 1, y - 1].IsLocked && field[x + 1, y].IsLocked && field[x, y - 1].IsLocked) return true;
            if (x < size - 1 && y < size - 1 && field[x + 1, y + 1].IsLocked && field[x + 1, y].IsLocked && field[x, y + 1].IsLocked) return true;

            return false;
        }

        private void AssignWallNumbers(AkariCell[,] field, int size, double hintRatio)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (field[x, y].IsLocked)
                    {
                        int bulbsAround = CountBulbsAround(field, x, y, size);

                        if (bulbsAround == 4)
                        {
                            field[x, y].WallNumber = 4;
                            continue;
                        }

                        field[x, y].WallNumber = _rng.NextDouble() < hintRatio ? bulbsAround : -1;
                    }
                }
        }

        private int CountBulbsAround(AkariCell[,] field, int x, int y, int size)
        {
            int count = 0;
            (int, int)[] nb = { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
            foreach (var (nx, ny) in nb)
                if (InBounds(nx, ny, size) && field[nx, ny].HasBulb)
                    count++;
            return count;
        }

        private static bool InBounds(int x, int y, int size) =>
            x >= 0 && x < size && y >= 0 && y < size;
    }
}