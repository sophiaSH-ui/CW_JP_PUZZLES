using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Core.Interfaces;
using CW_JP_PUZZLES.Common;

namespace CW_JP_PUZZLES.Core
{
    public abstract class PuzzleBase : IGame
    {
        public int Size { get; protected set; }
        public GameTimer Timer { get; } = new GameTimer();
        public int MoveCount { get; protected set; }

        public abstract void GenerateField(int size, Difficulty difficulty);
        public abstract bool MakeMove(int x, int y, object moveData);
        public abstract bool IsGameOver();

        public virtual string GetHint() => "Підказок поки немає.";

        public virtual void Reset()
        {
            MoveCount = 0;
            Timer.Reset();
        }
    }
}
