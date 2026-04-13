using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CW_JP_PUZZLES.Models
{
    public class GameSession
    {
        public DateTime StartTime { get; set; }
        public int MoveCount { get; set; }
        public TimeSpan ElapsedTime { get; set; }

        // можливо додам стан сітки чи клітинок
    }
}