using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core.Interfaces;

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

        public virtual void Reset()
        {
            MoveCount = 0;
            Timer.Reset();
        }
    }
}