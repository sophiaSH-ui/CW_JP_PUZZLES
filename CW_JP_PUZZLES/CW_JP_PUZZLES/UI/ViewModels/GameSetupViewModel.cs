using System;
using System.Collections.Generic;
using System.Windows.Input;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Models;

namespace CW_JP_PUZZLES.UI.ViewModels
{
    public class GameSetupViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly List<string> _games = new() { "Akari", "Hitori", "Shikaku", "Nurikabe" };
        private int _gameIndex;
        private Difficulty _difficulty = Difficulty.Easy;

        public string GameName => _games[_gameIndex];
        public Difficulty SelectedDifficulty
        {
            get => _difficulty;
            set
            {
                if (SetField(ref _difficulty, value))
                    OnPropertyChanged(nameof(SelectedDifficultyText));
            }
        }

        public string SelectedDifficultyText => SelectedDifficulty == Difficulty.Easy ? "EASY" : "HARD";

        public ICommand NextGameCommand { get; }
        public ICommand PrevGameCommand { get; }
        public ICommand ToggleDifficultyCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public GameSetupViewModel(MainViewModel main, string gameName)
        {
            _main = main;
            _gameIndex = Math.Max(0, _games.IndexOf(gameName));

            NextGameCommand = new RelayCommand(() => { _gameIndex = (_gameIndex + 1) % _games.Count; OnPropertyChanged(nameof(GameName)); });
            PrevGameCommand = new RelayCommand(() => { _gameIndex = (_gameIndex - 1 + _games.Count) % _games.Count; OnPropertyChanged(nameof(GameName)); });
            ToggleDifficultyCommand = new RelayCommand(() => SelectedDifficulty = SelectedDifficulty == Difficulty.Easy ? Difficulty.Hard : Difficulty.Easy);

            StartCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Click);
                _main.NavigateToGame(new GameConfig { GameName = GameName, Difficulty = SelectedDifficulty });
            });

            BackCommand = new RelayCommand(() => _main.NavigateToMenu());
            OpenSettingsCommand = new RelayCommand(() => _main.NavigateToSettings());
        }
    }
}