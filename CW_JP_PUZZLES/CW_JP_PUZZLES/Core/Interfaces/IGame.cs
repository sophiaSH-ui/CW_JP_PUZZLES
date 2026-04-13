using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using CW_JP_PUZZLES.Common;

namespace CW_JP_PUZZLES.Core.Interfaces
{
    public interface IGame
    {
        void GenerateField(int size, Difficulty difficulty);
        bool MakeMove(int x, int y, object moveData);
        bool IsGameOver();
        string GetHint();
        void Reset();
    }
}