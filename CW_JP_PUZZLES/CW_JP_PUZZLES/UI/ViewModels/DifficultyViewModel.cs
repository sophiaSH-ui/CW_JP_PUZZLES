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
    public class DifficultyViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        public string GameName { get; }

        private Difficulty _selected = Difficulty.Easy;
        public Difficulty SelectedDifficulty
        {
            get => _selected;
            set => SetField(ref _selected, value);
        }

        public bool IsEasy
        {
            get => _selected == Difficulty.Easy;
            set { if (value) SelectedDifficulty = Difficulty.Easy; }
        }
        public bool IsMedium
        {
            get => _selected == Difficulty.Medium;
            set { if (value) SelectedDifficulty = Difficulty.Medium; }
        }
        public bool IsHard
        {
            get => _selected == Difficulty.Hard;
            set { if (value) SelectedDifficulty = Difficulty.Hard; }
        }
        public bool IsExpert
        {
            get => _selected == Difficulty.Expert;
            set { if (value) SelectedDifficulty = Difficulty.Expert; }
        }

        public ICommand StartCommand { get; }
        public ICommand BackCommand { get; }

        public DifficultyViewModel(MainViewModel main, string gameName)
        {
            _main = main;
            GameName = gameName;

            StartCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Click);
                var config = new GameConfig
                {
                    GameName = GameName,
                    Size = 7,
                    Difficulty = SelectedDifficulty
                };
                _main.NavigateToGame(config);
            });

            BackCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Navigate);
                _main.NavigateToMenu();
            });
        }
    }
}