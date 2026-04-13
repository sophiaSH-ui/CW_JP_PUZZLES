using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CW_JP_PUZZLES.Core.Cells;

namespace CW_JP_PUZZLES.Core.Interfaces
{
    public interface ISolver<T> where T : CellBase
    {
        bool HasUniqueSolution(T[,] field);
        bool IsValid(T[,] field);
    }
}