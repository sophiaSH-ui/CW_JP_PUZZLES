using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace CW_JP_PUZZLES.Common
{
    public class GameResult
    {
        public bool IsVictory { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public int MoveCount { get; set; }
        public Difficulty Difficulty { get; set; }
    }
}