using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Diagnostics;

namespace CW_JP_PUZZLES.Common
{
    public class GameTimer
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public void Start() => _stopwatch.Start();
        public void Stop() => _stopwatch.Stop();
        public void Reset() => _stopwatch.Reset();

        public TimeSpan Elapsed => _stopwatch.Elapsed;
        public bool IsRunning => _stopwatch.IsRunning;

        public string GetFormattedTime() => _stopwatch.Elapsed.ToString(@"mm\:ss");
    }
}