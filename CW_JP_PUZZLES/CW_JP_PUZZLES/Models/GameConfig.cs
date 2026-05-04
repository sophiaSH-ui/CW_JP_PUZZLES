using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;

namespace CW_JP_PUZZLES.Models
{
    public class GameConfig
    {
        public string GameName { get; set; } = "Akari";
        public Difficulty Difficulty { get; set; } = Difficulty.Easy;
        public int Size => GetSizeForDifficulty(Difficulty);

        private static int GetSizeForDifficulty(Difficulty diff) => diff switch
        {
            Difficulty.Easy => 5,
            Difficulty.Hard => 7,
            _ => 5
        };
    }
}