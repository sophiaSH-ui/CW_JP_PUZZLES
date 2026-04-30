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
    public class SettingsViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;
        private readonly Action<Settings> _onSave;

        private bool _isMusicEnabled;
        private bool _isSfxEnabled;
        private float _musicVolume;
        private float _sfxVolume;

        public bool IsMusicEnabled
        {
            get => _isMusicEnabled;
            set
            {
                SetField(ref _isMusicEnabled, value);
                SoundService.Instance.SetMusicEnabled(value);
            }
        }

        public bool IsSfxEnabled
        {
            get => _isSfxEnabled;
            set
            {
                SetField(ref _isSfxEnabled, value);
                SoundService.Instance.SetSfxEnabled(value);
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                SetField(ref _musicVolume, value);
                SoundService.Instance.SetMusicVolume(value);
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => SetField(ref _sfxVolume, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public SettingsViewModel(MainViewModel main, Settings current, Action<Settings> onSave)
        {
            _main = main;
            _onSave = onSave;

            _isMusicEnabled = current.IsMusicEnabled;
            _isSfxEnabled = current.IsSfxEnabled;
            _musicVolume = current.MusicVolume;
            _sfxVolume = current.SfxVolume;

            SaveCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Click);
                _onSave(new Settings
                {
                    IsMusicEnabled = IsMusicEnabled,
                    IsSfxEnabled = IsSfxEnabled,
                    MusicVolume = MusicVolume,
                    SfxVolume = SfxVolume
                });
                _main.NavigateToMenu();
            });

            BackCommand = new RelayCommand(() =>
            {
                SoundService.Instance.PlaySfx(SoundEffect.Navigate);
                _main.NavigateToMenu();
            });
        }
    }
}