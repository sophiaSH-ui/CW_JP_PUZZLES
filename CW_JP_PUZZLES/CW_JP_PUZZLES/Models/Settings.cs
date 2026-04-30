using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW_JP_PUZZLES.Models
{
    public class Settings
    {
        public string Theme { get; set; } = "Dark";
        public bool IsSoundEnabled { get; set; } = true;
        public string Language { get; set; } = "en-US";

        // version btw
        public string Version { get; set; } = "1.0.0";


        public bool IsMusicEnabled { get; set; } = true;
        public bool IsSfxEnabled { get; set; } = true;
        public float MusicVolume { get; set; } = 0.4f;
        public float SfxVolume { get; set; } = 0.8f;
    }
}