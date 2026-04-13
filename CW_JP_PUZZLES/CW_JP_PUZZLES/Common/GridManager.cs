using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CW_JP_PUZZLES.Core.Cells;

namespace CW_JP_PUZZLES.Common
{
    public static class GridManager
    {
        public static bool IsInBounds(int x, int y, int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        public static (int x, int y)[] GetNeighbors(int x, int y)
        {
            return new[]
            {
                (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y)
            };
        }
    }
}