using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CW_JP_PUZZLES.Core.Cells
{
    public abstract class CellBase
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsLocked { get; set; }

        public abstract void Reset();
    }
}