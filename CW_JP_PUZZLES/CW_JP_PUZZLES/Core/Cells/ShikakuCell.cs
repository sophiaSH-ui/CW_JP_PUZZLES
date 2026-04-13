using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_JP_PUZZLES.Core.Cells
{
    public class ShikakuCell : CellBase
    {
        public int ClueValue { get; set; } = -1; 
        public int RegionId { get; set; } = -1; 
        public override void Reset() { RegionId = -1; }
    }
}