using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            double wallRatio = difficulty switch
            {
                Difficulty.Easy => 0.15,
                Difficulty.Medium => 0.20,
                Difficulty.Hard => 0.25
            };

            AkariCell[,] field;

            int attempts = 0;
            do
            {
                field = BuildField(size, wallRatio);
                attempts++;
            }
            while (!_solver.HasUniqueSolution(field) && attempts < 50);

            return field;
        }

        private AkariCell[,] BuildField(int size, double wallRatio)
        {
            var field = new AkariCell[size, size];

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    field[x, y] = new AkariCell { X = x, Y = y, WallNumber = -1 };

            int wallCount = (int)(size * size * wallRatio);
            var positions = Enumerable.Range(0, size * size)
                                      .OrderBy(_ => _rng.Next())
                                      .Take(wallCount);

            foreach (int pos in positions)
            {
                int x = pos / size, y = pos % size;
                field[x, y].IsLocked = true;  
                field[x, y].WallNumber = _rng.Next(0, 7); 

                if (_rng.NextDouble() < 0.4) field[x, y].WallNumber = -2; 
            }

            return field;
        }
    }
}