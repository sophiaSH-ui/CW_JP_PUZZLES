using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Core;
using CW_JP_PUZZLES.Core.Interfaces;
using CW_JP_PUZZLES.Games.Akari;
using CW_JP_PUZZLES.Games.Hitori;
using CW_JP_PUZZLES.Games.Nurikabe;
using CW_JP_PUZZLES.Games.Shikaku;
using CW_JP_PUZZLES.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace CW_JP_PUZZLES.UI.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly GameConfig _config;
        private readonly PuzzleBase _game;

        private readonly DispatcherTimer _uiTimer;

        public ObservableCollection<CellViewModel> Cells { get; } = new();

        public int GridSize => _config.Size; 

        private string _timeDisplay = "00:00";
        public string TimeDisplay
        {
            get => _timeDisplay;
            set => SetField(ref _timeDisplay, value);
        }

        private int _moveCount;
        public int MoveCount
        {
            get => _moveCount;
            set => SetField(ref _moveCount, value);
        }

        private string _hintText = string.Empty;
        public string HintText
        {
            get => _hintText;
            set => SetField(ref _hintText, value);
        }

        private bool _isHintVisible;
        public bool IsHintVisible
        {
            get => _isHintVisible;
            set => SetField(ref _isHintVisible, value);
        }

        public string GameName => _config.GameName;
        public string Difficulty => _config.Difficulty.ToString();

        public ICommand HintCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand BackCommand { get; }

        public GameViewModel(MainViewModel main, GameConfig config)
        {
            _main = main;
            _config = config;
            _game = CreateGame(config.GameName);

            _game.GenerateField(config.Size, config.Difficulty);

            BuildCells();

            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _uiTimer.Tick += (_, _) =>
            {
                TimeDisplay = _game.Timer.GetFormattedTime();
                MoveCount = _game.MoveCount;
            };
            _uiTimer.Start();

            HintCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Hint);
                HintText = _game.GetHint();
                IsHintVisible = true;
            });

            ResetCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Click);
                _game.Reset();
                RefreshAllCells();
                IsHintVisible = false;
            });

            BackCommand = new RelayCommand(() =>
            {
                _uiTimer.Stop();
                SoundService.Instance.PlaySfx(SoundEffect.Navigate);
                _main.NavigateToMenu();
            });
        }

        private static PuzzleBase CreateGame(string name) => name switch
        {
            "Akari" => new AkariGame(),
            "Hitori" => new HitoriGame(),
            "Shikaku" => new ShikakuGame(),
            "Nurikabe" => new NurikabeGame(),
            _ => throw new ArgumentException($"Невідома гра: {name}")
        };

        private void BuildCells()
        {
            Cells.Clear();

            for (int x = 0; x < GridSize; x++)
                for (int y = 0; y < GridSize; y++)
                {
                    var cell = new CellViewModel(x, y, _config.GameName, OnCellAction);
                    SyncCell(cell, x, y);
                    Cells.Add(cell);
                }
        }

        private void OnCellAction(int x, int y, object? moveData)
        {
            bool moved = _game.MakeMove(x, y, moveData);

            if (!moved)
            {
                SoundService.Instance.PlaySfx(SoundEffect.Error);
                return;
            }

            SoundService.Instance.PlaySfx(
                moveData is "black" or null ? SoundEffect.Place : SoundEffect.Remove);

            RefreshAllCells();

            if (_game.IsGameOver())
            {
                _uiTimer.Stop();
                var result = new GameResult
                {
                    IsVictory = true,
                    TimeSpent = _game.Timer.Elapsed,
                    MoveCount = _game.MoveCount,
                    Difficulty = _config.Difficulty
                };
                _main.NavigateToResult(result, _config);
            }
        }

        private void RefreshAllCells()
        {
            foreach (var cell in Cells)
                SyncCell(cell, cell.Row, cell.Col);
        }

        private void SyncCell(CellViewModel vm, int x, int y)
        {
            switch (_config.GameName)
            {
                case "Akari":
                    {
                        var g = (AkariGame)_game;
                        var c = g.Grid[x, y];
                        vm.HasBulb = c.HasBulb;
                        vm.IsIlluminated = c.IsIlluminated;
                        vm.IsWall = c.IsLocked;
                        vm.WallNumber = c.WallNumber;
                        break;
                    }
                case "Hitori":
                    {
                        var g = (HitoriGame)_game;
                        var c = g.Grid[x, y];
                        vm.IsBlackened = c.IsBlackened;
                        vm.IsCircled = c.IsCircled;
                        vm.DisplayValue = c.Value.ToString();
                        break;
                    }
                case "Shikaku":
                    {
                        var g = (ShikakuGame)_game;
                        var c = g.Grid[x, y];
                        vm.ClueValue = c.ClueValue > 0 ? c.ClueValue.ToString() : "";
                        vm.RegionId = c.RegionId;
                        break;
                    }
                case "Nurikabe":
                    {
                        var g = (NurikabeGame)_game;
                        var c = g.Grid[x, y];
                        vm.IsBlackened = c.IsBlack;
                        vm.ClueValue = c.ClueValue > 0 ? c.ClueValue.ToString() : "";
                        vm.IslandId = c.IslandId;
                        break;
                    }
            }
        }
    }
}