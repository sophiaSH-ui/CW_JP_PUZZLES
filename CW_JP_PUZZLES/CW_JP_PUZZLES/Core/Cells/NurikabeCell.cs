using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_JP_PUZZLES.Core.Cells
{
    public class NurikabeCell : CellBase
    {
        public bool IsBlack { get; set; }
        public int IslandId { get; set; } = -1;
        public int ClueValue { get; set; } = -1;
        public override void Reset() { IsBlack = false; IslandId = -1; }
    }
}
