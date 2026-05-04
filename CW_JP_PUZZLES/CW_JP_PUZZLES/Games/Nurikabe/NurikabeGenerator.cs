using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Core.Interfaces;

namespace CW_JP_PUZZLES.Games.Nurikabe
{
    public class NurikabeGenerator : IGenerator<NurikabeCell>
    {
        private readonly NurikabeSolver _solver = new();
        private readonly Random _rng = new();

        public NurikabeCell[,] Generate(int size, Difficulty difficulty)
        {
            NurikabeCell[,] field;
            int attempts = 0;

            do
            {
                field = BuildPuzzle(size, difficulty);
                attempts++;
            }
            while (!_solver.HasUniqueSolution(field) && attempts < 100);

            return field;
        }

        private NurikabeCell[,] BuildPuzzle(int size, Difficulty difficulty)
        {
            var field = new NurikabeCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    field[x, y] = new NurikabeCell { X = x, Y = y, IsBlack = true };
                }

            var (islandCount, maxIslandSize) = difficulty switch
            {
                Difficulty.Easy => (4, 4),
                //Difficulty.Medium => (5, 5),
                Difficulty.Hard => (6, 4)
            };

            bool success = PlaceIslands(field, size, islandCount, maxIslandSize);
            if (!success) return field;

            FixTwoByTwo(field, size);

            FinalizeForPlayer(field, size);

            return field;
        }

        private bool PlaceIslands(NurikabeCell[,] field, int size,
            int islandCount, int maxIslandSize)
        {
            int totalCells = size * size;
            int blackMin = islandCount; 

            var positions = Enumerable.Range(0, totalCells)
                                      .OrderBy(_ => _rng.Next())
                                      .ToList();

            var islandSeeds = new List<(int x, int y)>();

            foreach (int pos in positions)
            {
                if (islandSeeds.Count >= islandCount) break;

                int x = pos / size, y = pos % size;

                bool tooClose = islandSeeds.Any(s =>
                    Math.Abs(s.x - x) + Math.Abs(s.y - y) < 3);

                if (!tooClose)
                    islandSeeds.Add((x, y));
            }

            if (islandSeeds.Count < islandCount) return false;

            int islandId = 0;
            foreach (var (sx, sy) in islandSeeds)
            {
                int targetSize = _rng.Next(2, maxIslandSize + 1);
                GrowIsland(field, size, sx, sy, targetSize, islandId);
                islandId++;
            }

            return true;
        }

        private void GrowIsland(NurikabeCell[,] field, int size,
            int sx, int sy, int targetSize, int islandId)
        {
            var island = new List<(int x, int y)>();
            var frontier = new List<(int x, int y)>();

            field[sx, sy].IsBlack = false;
            field[sx, sy].IslandId = islandId;
            island.Add((sx, sy));

            foreach (var nb in Neighbors(sx, sy))
                if (InBounds(nb.x, nb.y, size) && CanJoinIsland(field, size, nb.x, nb.y, islandId))
                    frontier.Add(nb);

            while (island.Count < targetSize && frontier.Count > 0)
            {
                int idx = _rng.Next(frontier.Count);
                var (nx, ny) = frontier[idx];
                frontier.RemoveAt(idx);

                if (!CanJoinIsland(field, size, nx, ny, islandId)) continue;

                field[nx, ny].IsBlack = false;
                field[nx, ny].IslandId = islandId;
                island.Add((nx, ny));

                foreach (var nb in Neighbors(nx, ny))
                    if (InBounds(nb.x, nb.y, size) &&
                        CanJoinIsland(field, size, nb.x, nb.y, islandId) &&
                        !frontier.Contains(nb))
                        frontier.Add(nb);
            }

            var (cx, cy) = island[island.Count / 2];
            field[cx, cy].ClueValue = island.Count;
            field[cx, cy].IsLocked = true;
        }

        private bool CanJoinIsland(NurikabeCell[,] field, int size,
            int x, int y, int islandId)
        {
            if (!field[x, y].IsBlack) return false; 

            foreach (var (nx, ny) in Neighbors(x, y))
            {
                if (!InBounds(nx, ny, size)) continue;
                var nb = field[nx, ny];
                if (!nb.IsBlack && nb.IslandId >= 0 && nb.IslandId != islandId)
                    return false; 
            }

            return true;
        }

        private void FixTwoByTwo(NurikabeCell[,] field, int size)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int x = 0; x < size - 1; x++)
                    for (int y = 0; y < size - 1; y++)
                    {
                        if (!field[x, y].IsBlack || !field[x + 1, y].IsBlack ||
                            !field[x, y + 1].IsBlack || !field[x + 1, y + 1].IsBlack)
                            continue;

                        var candidates = new[]
                        {
                            (x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1)
                        }.Where(c => field[c.Item1, c.Item2].ClueValue <= 0).ToList();

                        if (candidates.Count == 0) continue;

                        var (fx, fy) = candidates[_rng.Next(candidates.Count)];
                        field[fx, fy].IsBlack = false;
                        AssignToNearestIsland(field, size, fx, fy);
                        changed = true;
                    }
            }
        }

        private void AssignToNearestIsland(NurikabeCell[,] field, int size, int x, int y)
        {
            foreach (var (nx, ny) in Neighbors(x, y))
            {
                if (!InBounds(nx, ny, size)) continue;
                if (!field[nx, ny].IsBlack && field[nx, ny].IslandId >= 0)
                {
                    field[x, y].IslandId = field[nx, ny].IslandId;

                    int islandId = field[x, y].IslandId;
                    int newSize = 0;
                    (int cx, int cy) = (0, 0);
                    for (int ix = 0; ix < size; ix++)
                        for (int iy = 0; iy < size; iy++)
                            if (field[ix, iy].IslandId == islandId)
                            {
                                newSize++;
                                if (field[ix, iy].ClueValue > 0) { cx = ix; cy = iy; }
                            }
                    field[cx, cy].ClueValue = newSize;
                    return;
                }
            }
        }

        private void FinalizeForPlayer(NurikabeCell[,] field, int size)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (!field[x, y].IsLocked)
                    {
                        field[x, y].IsBlack = false;
                        field[x, y].IslandId = -1;
                    }
                }
        }

        private static (int x, int y)[] Neighbors(int x, int y) =>
            new[] { (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y) };

        private static bool InBounds(int x, int y, int size) =>
            x >= 0 && x < size && y >= 0 && y < size;
    }
}