using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using CW_JP_PUZZLES.Data;
using CW_JP_PUZZLES.Models;

namespace CW_JP_PUZZLES.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel = null!;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetField(ref _currentViewModel, value);
        }

        private Settings _settings;
        private static readonly string SettingsPath = "Resources/Config/AppSettings.xml";

        public MainViewModel()
        {
            _settings = XmlConfigProvider.LoadFromFile<Settings>(SettingsPath);
            SoundService.Instance.ApplySettings(_settings);

            NavigateToMenu();
        }

        public void NavigateToMenu()
        {
            SoundService.Instance.PlayMusic(MusicTrack.Menu);
            CurrentViewModel = new MainMenuViewModel(this);
        }

        public void NavigateToDifficulty(string gameName)
        {
            SoundService.Instance.PlaySfx(SoundEffect.Navigate);
            CurrentViewModel = new DifficultyViewModel(this, gameName);
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
            CurrentViewModel = new GameViewModel(this, config);
        }

        public void NavigateToResult(GameResult result, GameConfig config)
        {
            SoundService.Instance.PlaySfx(SoundEffect.Victory);
            CurrentViewModel = new ResultViewModel(this, result, config);
        }

        public void NavigateToSettings()
        {
            SoundService.Instance.PlaySfx(SoundEffect.Navigate);
            CurrentViewModel = new SettingsViewModel(this, _settings, SaveSettings);
        }

        public void SaveSettings(Settings updated)
        {
            _settings = updated;
            XmlConfigProvider.SaveToFile(_settings, SettingsPath);
            SoundService.Instance.ApplySettings(_settings);
        }
    }
}