using System;
using System.Collections.Generic;
using System.Linq;
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
            var (islandCount, minIslandSize, maxIslandSize) = (size, difficulty) switch
            {
                (5, Difficulty.Easy) => (4, 2, 4),
                (5, Difficulty.Hard) => (4, 2, 4),
                (7, Difficulty.Easy) => (6, 3, 5),
                (7, Difficulty.Hard) => (8, 3, 5),
                _ => (4, 2, 4)
            };

            for (int attempt = 0; attempt < 25; attempt++)
            {
                var solution = BuildSolutionField(size, islandCount, minIslandSize, maxIslandSize);
                if (solution == null) continue;
                if (!_solver.IsValid(solution)) continue;

                var puzzle = StripForPlayer(solution, size);
                var testField = CloneField(puzzle, size);
                var task = Task.Run(() => _solver.HasUniqueSolution(testField));

                if (task.Wait(TimeSpan.FromMilliseconds(size <= 5 ? 300 : 600)))
                {
                    if (task.Result) return puzzle;
                }
            }

            for (int attempt = 0; attempt < 60; attempt++)
            {
                var solution = BuildSolutionField(size, islandCount, minIslandSize, maxIslandSize);
                if (solution != null && _solver.IsValid(solution))
                    return StripForPlayer(solution, size);
            }

            int fallbackCount = Math.Max(3, islandCount - 2);
            int fallbackMin = 3;
            int fallbackMax = Math.Min(size, maxIslandSize + 1);
            for (int attempt = 0; attempt < 60; attempt++)
            {
                var solution = BuildSolutionField(size, fallbackCount, fallbackMin, fallbackMax);
                if (solution != null && _solver.IsValid(solution))
                    return StripForPlayer(solution, size);
            }

            for (int attempt = 0; attempt < 200; attempt++)
            {
                var solution = BuildSolutionField(size, 3, 3, size);
                if (solution != null && _solver.IsValid(solution))
                    return StripForPlayer(solution, size);
            }

            return BuildEmergencyPuzzle(size);
        }

        private NurikabeCell[,]? BuildSolutionField(int size, int islandCount, int minIslandSize, int maxIslandSize)
        {
            var field = new NurikabeCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y] = new NurikabeCell { X = x, Y = y, IsBlack = true };

            if (!PlaceIslands(field, size, islandCount, minIslandSize, maxIslandSize))
                return null;

            if (!FixTwoByTwo(field, size, maxIslandSize))
                return null;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (field[x, y].IsBlack) continue;
                    if (field[x, y].IslandId < 0) return null;
                }

            return field;
        }

        private NurikabeCell[,] StripForPlayer(NurikabeCell[,] solution, int size)
        {
            var puzzle = CloneField(solution, size);
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    if (!puzzle[x, y].IsLocked)
                    {
                        puzzle[x, y].IsBlack = false;
                        puzzle[x, y].IslandId = -1;
                    }
                }
            return puzzle;
        }

        private bool PlaceIslands(NurikabeCell[,] field, int size, int islandCount, int minIslandSize, int maxIslandSize)
        {
            int totalCells = size * size;
            var positions = Enumerable.Range(0, totalCells).OrderBy(_ => _rng.Next()).ToList();
            var islandSeeds = new List<(int x, int y)>();

            int minDist = size <= 5 ? 3 : 3;

            foreach (int pos in positions)
            {
                if (islandSeeds.Count >= islandCount) break;

                int x = pos / size, y = pos % size;
                bool tooClose = islandSeeds.Any(s => Math.Abs(s.x - x) + Math.Abs(s.y - y) < minDist);

                if (!tooClose) islandSeeds.Add((x, y));
            }

            if (islandSeeds.Count < islandCount) return false;

            int islandId = 0;
            foreach (var (sx, sy) in islandSeeds)
            {
                int targetSize = _rng.Next(minIslandSize, maxIslandSize + 1);
                GrowIsland(field, size, sx, sy, targetSize, islandId);
                islandId++;
            }

            return true;
        }

        private void GrowIsland(NurikabeCell[,] field, int size, int sx, int sy, int targetSize, int islandId)
        {
            var island = new List<(int x, int y)>();
            var frontier = new List<(int x, int y)>();

            field[sx, sy].IsBlack = false;
            field[sx, sy].IslandId = islandId;
            island.Add((sx, sy));

            foreach (var nb in GridManager.GetNeighbors(sx, sy))
                if (GridManager.IsInBounds(nb.x, nb.y, size, size) && CanJoinIsland(field, size, nb.x, nb.y, islandId))
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

                foreach (var nb in GridManager.GetNeighbors(nx, ny))
                    if (GridManager.IsInBounds(nb.x, nb.y, size, size) &&
                        CanJoinIsland(field, size, nb.x, nb.y, islandId) &&
                        !frontier.Contains(nb))
                        frontier.Add(nb);
            }

            var (cx, cy) = island[island.Count / 2];
            field[cx, cy].ClueValue = island.Count;
            field[cx, cy].IsLocked = true;
        }

        private bool CanJoinIsland(NurikabeCell[,] field, int size, int x, int y, int islandId)
        {
            if (!field[x, y].IsBlack) return false;

            foreach (var (nx, ny) in GridManager.GetNeighbors(x, y))
            {
                if (!GridManager.IsInBounds(nx, ny, size, size)) continue;
                var nb = field[nx, ny];
                if (!nb.IsBlack && nb.IslandId >= 0 && nb.IslandId != islandId)
                    return false;
            }

            return true;
        }

        private bool FixTwoByTwo(NurikabeCell[,] field, int size, int maxIslandSize = 6)
        {
            int maxPasses = size * size * 2;
            bool changed = true;

            while (changed && maxPasses-- > 0)
            {
                changed = false;
                for (int x = 0; x < size - 1; x++)
                    for (int y = 0; y < size - 1; y++)
                    {
                        if (!field[x, y].IsBlack || !field[x + 1, y].IsBlack ||
                            !field[x, y + 1].IsBlack || !field[x + 1, y + 1].IsBlack)
                            continue;

                        var block = new[] { (x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1) }
                            .Where(c => field[c.Item1, c.Item2].ClueValue <= 0).ToList();

                        if (block.Count == 0) continue;

                        var adjacentToIsland = block
                            .Where(c => HasAdjacentIsland(field, size, c.Item1, c.Item2))
                            .ToList();

                        var candidates = adjacentToIsland.Count > 0 ? adjacentToIsland : block;
                        var (fx, fy) = candidates[_rng.Next(candidates.Count)];

                        field[fx, fy].IsBlack = false;

                        if (!AssignToNearestIsland(field, size, fx, fy, maxIslandSize))
                        {
                            field[fx, fy].IsBlack = true;

                            bool assigned = false;
                            foreach (var (cx, cy) in block.Where(c => c != (fx, fy)))
                            {
                                field[cx, cy].IsBlack = false;
                                if (AssignToNearestIsland(field, size, cx, cy, maxIslandSize))
                                {
                                    assigned = true;
                                    break;
                                }
                                field[cx, cy].IsBlack = true;
                            }

                            if (!assigned) return false;
                        }

                        changed = true;
                    }
            }

            return maxPasses > 0;
        }

        private bool HasAdjacentIsland(NurikabeCell[,] field, int size, int x, int y)
        {
            foreach (var (nx, ny) in GridManager.GetNeighbors(x, y))
            {
                if (!GridManager.IsInBounds(nx, ny, size, size)) continue;
                if (!field[nx, ny].IsBlack && field[nx, ny].IslandId >= 0)
                    return true;
            }
            return false;
        }

        private bool AssignToNearestIsland(NurikabeCell[,] field, int size, int x, int y, int maxIslandSize = 6)
        {
            var adjacentIslands = new List<int>();
            foreach (var (nx, ny) in GridManager.GetNeighbors(x, y))
            {
                if (!GridManager.IsInBounds(nx, ny, size, size)) continue;
                if (!field[nx, ny].IsBlack && field[nx, ny].IslandId >= 0)
                {
                    int id = field[nx, ny].IslandId;
                    if (!adjacentIslands.Contains(id))
                        adjacentIslands.Add(id);
                }
            }

            foreach (int targetId in adjacentIslands)
            {
                if (!WouldCauseIslandTouch(field, size, x, y, targetId)
                    && GetIslandSize(field, size, targetId) < maxIslandSize)
                {
                    field[x, y].IslandId = targetId;
                    UpdateClueValue(field, size, targetId);
                    return true;
                }
            }

            if (!WouldCauseIslandTouchAny(field, size, x, y))
            {
                int newId = GetNextIslandId(field, size);
                field[x, y].IslandId = newId;
                field[x, y].ClueValue = 1;
                field[x, y].IsLocked = true;
                return true;
            }

            return false;
        }

        private bool WouldCauseIslandTouch(NurikabeCell[,] field, int size, int x, int y, int islandId)
        {
            foreach (var (nx, ny) in GridManager.GetNeighbors(x, y))
            {
                if (!GridManager.IsInBounds(nx, ny, size, size)) continue;
                var nb = field[nx, ny];
                if (!nb.IsBlack && nb.IslandId >= 0 && nb.IslandId != islandId)
                    return true;
            }
            return false;
        }

        private bool WouldCauseIslandTouchAny(NurikabeCell[,] field, int size, int x, int y)
        {
            foreach (var (nx, ny) in GridManager.GetNeighbors(x, y))
            {
                if (!GridManager.IsInBounds(nx, ny, size, size)) continue;
                var nb = field[nx, ny];
                if (!nb.IsBlack && nb.IslandId >= 0)
                    return true;
            }
            return false;
        }

        private int GetNextIslandId(NurikabeCell[,] field, int size)
        {
            int maxId = -1;
            for (int ix = 0; ix < size; ix++)
                for (int iy = 0; iy < size; iy++)
                    if (field[ix, iy].IslandId > maxId)
                        maxId = field[ix, iy].IslandId;
            return maxId + 1;
        }

        private int GetIslandSize(NurikabeCell[,] field, int size, int islandId)
        {
            int count = 0;
            for (int ix = 0; ix < size; ix++)
                for (int iy = 0; iy < size; iy++)
                    if (field[ix, iy].IslandId == islandId)
                        count++;
            return count;
        }

        private void UpdateClueValue(NurikabeCell[,] field, int size, int islandId)
        {
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
        }

        private NurikabeCell[,] BuildEmergencyPuzzle(int size)
        {
            var field = new NurikabeCell[size, size];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y] = new NurikabeCell { X = x, Y = y, IsBlack = true };

            int id = 0;
            int islandSize = 0;
            for (int i = 0; i < Math.Min(3, size); i++)
            {
                field[0, i].IsBlack = false;
                field[0, i].IslandId = id;
                islandSize++;
            }
            for (int i = 1; i < Math.Min(3, size); i++)
            {
                field[i, 0].IsBlack = false;
                field[i, 0].IslandId = id;
                islandSize++;
            }
            field[0, 0].ClueValue = islandSize;
            field[0, 0].IsLocked = true;

            id = 1;
            islandSize = 0;
            for (int i = size - 1; i >= Math.Max(size - 3, 0); i--)
            {
                field[size - 1, i].IsBlack = false;
                field[size - 1, i].IslandId = id;
                islandSize++;
            }
            for (int i = size - 2; i >= Math.Max(size - 3, 0); i--)
            {
                field[i, size - 1].IsBlack = false;
                field[i, size - 1].IslandId = id;
                islandSize++;
            }
            field[size - 1, size - 1].ClueValue = islandSize;
            field[size - 1, size - 1].IsLocked = true;

            FixTwoByTwo(field, size);

            return StripForPlayer(field, size);
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
    }
}