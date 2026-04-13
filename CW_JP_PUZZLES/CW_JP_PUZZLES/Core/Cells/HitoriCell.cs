using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_JP_PUZZLES.Core.Cells
{
    public class HitoriCell : CellBase
    {
        public int Value { get; set; }
        public bool IsBlackened { get; set; }
        public bool IsCircled { get; set; }
        public override void Reset() { IsBlackened = false; IsCircled = false; }
    }
}