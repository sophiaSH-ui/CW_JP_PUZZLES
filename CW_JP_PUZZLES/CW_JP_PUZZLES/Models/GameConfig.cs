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
        public string GameName { get; set; } = "Japanese Puzzle";
        public int Size { get; set; } = 7;
        public Difficulty Difficulty { get; set; } = Difficulty.Easy;
        public string LevelSourcePath { get; set; } = string.Empty;
    }
}