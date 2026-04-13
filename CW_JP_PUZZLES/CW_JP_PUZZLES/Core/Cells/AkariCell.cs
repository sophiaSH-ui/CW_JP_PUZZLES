using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_JP_PUZZLES.Core.Cells
{
    public class AkariCell : CellBase
    {
        public bool HasBulb { get; set; }
        public bool IsIlluminated { get; set; }
        public int WallNumber { get; set; } = -1; 
        public override void Reset()
        {
            HasBulb = false;
            IsIlluminated = false;
        }
    }
}