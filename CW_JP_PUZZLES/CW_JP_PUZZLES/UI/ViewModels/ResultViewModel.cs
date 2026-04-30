using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Models;
using System.Windows.Input;

namespace CW_JP_PUZZLES.UI.ViewModels
{
    public class ResultViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly GameConfig _config;

        public string GameName { get; }
        public string TimeDisplay { get; }
        public string MoveDisplay { get; }
        public string Difficulty { get; }
        public bool IsVictory { get; }

        public string ResultMessage => IsVictory
            ? "[ PUZZLE SOLVED ]"
            : "[ GAME OVER ]";

        public string ResultSubMessage => IsVictory
            ? "Great job! You solved the PUZZLE."
            : "Hmm... something is wrong, try again!";

        public ICommand PlayAgainCommand { get; }
        public ICommand MenuCommand { get; }

        public ResultViewModel(MainViewModel main, GameResult result, GameConfig config)
        {
            _main = main;
            _config = config;

            GameName = config.GameName;
            Difficulty = config.Difficulty.ToString();
            IsVictory = result.IsVictory;
            TimeDisplay = result.TimeSpent.ToString(@"mm\:ss");
            MoveDisplay = result.MoveCount.ToString();

            PlayAgainCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Click);
                _main.NavigateToGame(_config); 
            });

            MenuCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Navigate);
                _main.NavigateToMenu();
            });
        }
    }
}
