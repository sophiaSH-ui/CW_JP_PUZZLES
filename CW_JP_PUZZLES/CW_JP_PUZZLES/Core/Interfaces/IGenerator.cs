using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Core.Cells;
using CW_JP_PUZZLES.Common;

namespace CW_JP_PUZZLES.Core.Interfaces
{
    public interface IGenerator<T> where T : CellBase
    {
        T[,] Generate(int size, Difficulty difficulty);
    }
}
