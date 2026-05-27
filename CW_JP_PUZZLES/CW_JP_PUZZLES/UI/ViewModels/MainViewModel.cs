using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Data;
using CW_JP_PUZZLES.Models;

namespace CW_JP_PUZZLES.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Stack<ViewModelBase> _viewHistory = new Stack<ViewModelBase>();

        private ViewModelBase _currentViewModel = null!;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetField(ref _currentViewModel, value);
        }

        private Settings _settings;
        private static readonly string SettingsPath = "Resources/Config/AppSettings.xml";

        public ICommand GoBackCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToMenuCommand { get; }

        public MainViewModel()
        {
            GoBackCommand = new RelayCommand(ExecuteGoBack, CanExecuteGoBack);
            ExitCommand = new RelayCommand(ExecuteExit);
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToSettings());
            NavigateToMenuCommand = new RelayCommand(_ => NavigateToMenu());

            _settings = XmlConfigProvider.LoadFromFile<Settings>(SettingsPath);
            SoundService.Instance.ApplySettings(_settings);

            NavigateToMenu();
        }

        public void NavigateToMenu()
        {
            SoundService.Instance.PlayMusic(MusicTrack.Menu);
            _viewHistory.Clear();
            CurrentViewModel = new MainMenuViewModel(this);
        }

        public void NavigateToDifficulty(string gameName)
        {
            SoundService.Instance.PlaySfx(SoundEffect.Navigate);
            if (CurrentViewModel != null) _viewHistory.Push(CurrentViewModel);
            CurrentViewModel = new GameSetupViewModel(this, gameName);
        }

        public void NavigateToGame(GameConfig config)
        {
            var track = config.GameName switch
            {
                "Akari" => MusicTrack.Akari,
                "Hitori" => MusicTrack.Hitori,
                "Shikaku" => MusicTrack.Shikaku,
                "Nurikabe" => MusicTrack.Nurikabe,
                _ => MusicTrack.Menu
            };
            SoundService.Instance.PlayMusic(track);
            if (CurrentViewModel != null) _viewHistory.Push(CurrentViewModel);
            CurrentViewModel = new GameViewModel(this, config);
        }

        public void NavigateToResult(GameResult result, GameConfig config)
        {
            SoundService.Instance.PlaySfx(SoundEffect.Victory);
            if (CurrentViewModel != null) _viewHistory.Push(CurrentViewModel);
            CurrentViewModel = new ResultViewModel(this, result, config);
        }

        public void NavigateToSettings()
        {
            SoundService.Instance.PlaySfx(SoundEffect.Navigate);
            if (CurrentViewModel != null) _viewHistory.Push(CurrentViewModel);
            CurrentViewModel = new SettingsViewModel(this, _settings, SaveSettings);
        }

        public void SaveSettings(Settings updated)
        {
            _settings = updated;
            XmlConfigProvider.SaveToFile(_settings, SettingsPath);
            SoundService.Instance.ApplySettings(_settings);
        }

        private void ExecuteGoBack(object obj)
        {
            if (_viewHistory.Count > 0)
            {
                SoundService.Instance.PlaySfx(SoundEffect.Navigate);
                var previousViewModel = _viewHistory.Pop();

                if (previousViewModel is MainMenuViewModel)
                {
                    SoundService.Instance.PlayMusic(MusicTrack.Menu);
                }

                CurrentViewModel = previousViewModel;
            }
        }

        private bool CanExecuteGoBack(object obj)
        {
            return _viewHistory.Count > 0;
        }

        private void ExecuteExit(object obj)
        {
            Application.Current.Shutdown();
        }
    }
}