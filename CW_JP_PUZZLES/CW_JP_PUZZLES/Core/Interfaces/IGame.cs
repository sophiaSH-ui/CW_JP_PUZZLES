using CW_JP_PUZZLES.Common;

namespace CW_JP_PUZZLES.Core.Interfaces
{
    public interface IGame
    {
        void GenerateField(int size, Difficulty difficulty);
        bool MakeMove(int x, int y, object moveData);
        bool IsGameOver();
        void Reset();
    }
}