using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace CW_JP_PUZZLES.Common
{
    public class SoundService
    {
        private static SoundService? _instance;
        public static SoundService Instance => _instance ??= new SoundService();

        private readonly MediaPlayer _musicPlayer = new();
        private float _musicVolume = 0.5f;
        private float _sfxVolume = 0.5f;
        private bool _isMusicEnabled = true;
        private bool _isSfxEnabled = true;

        private static readonly string SoundsDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");

        private SoundService() { }

        public void ApplySettings(Models.Settings settings)
        {
            _isMusicEnabled = settings.IsMusicEnabled;
            _isSfxEnabled = settings.IsSfxEnabled;
            _musicVolume = settings.MusicVolume;
            _sfxVolume = settings.SfxVolume;

            _musicPlayer.Volume = _musicVolume;

            if (!_isMusicEnabled) StopMusic();
        }

        public void PlayMusic(MusicTrack track)
        {
            if (!_isMusicEnabled) return;

            string file = "background.mp3";
            string path = Path.Combine(SoundsDir, file);
            if (!File.Exists(path)) return;

            if (_musicPlayer.Source != null && 
                _musicPlayer.Source.AbsolutePath.Equals(new Uri(path, UriKind.Absolute).AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _musicPlayer.Stop();
            _musicPlayer.Open(new Uri(path, UriKind.Absolute));
            _musicPlayer.Volume = _musicVolume;
            _musicPlayer.MediaEnded += OnMusicEnded;
            _musicPlayer.Play();
        }

        public void StopMusic()
        {
            _musicPlayer.Stop();
            _musicPlayer.MediaEnded -= OnMusicEnded;
            _musicPlayer.Close();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Math.Clamp(volume, 0f, 1f);
            _musicPlayer.Volume = _musicVolume;
        }

        private void OnMusicEnded(object? sender, EventArgs e)
        {
            _musicPlayer.Position = TimeSpan.Zero;
            _musicPlayer.Play();
        }

        private readonly MediaPlayer _sfxPlayer = new();
        private bool _sfxLoaded = false;

        public void PlaySfx(SoundEffect sfx)
        {
            if (!_isSfxEnabled) return;

            if (sfx == SoundEffect.Place || sfx == SoundEffect.Remove || sfx == SoundEffect.Error)
                return;

            string file = "button click.mp3";
            string path = Path.Combine(SoundsDir, file);
            if (!File.Exists(path)) return;

            if (!_sfxLoaded)
            {
                _sfxPlayer.Open(new Uri(path, UriKind.Absolute));
                _sfxLoaded = true;
            }
            
            _sfxPlayer.Volume = _sfxVolume * 0.3f; 
            _sfxPlayer.Position = TimeSpan.Zero;
            _sfxPlayer.Play();
        }

        public void SetSfxVolume(float volume) => _sfxVolume = Math.Clamp(volume, 0f, 1f);
        public void SetSfxEnabled(bool enabled) => _isSfxEnabled = enabled;
        public void SetMusicEnabled(bool enabled)
        {
            _isMusicEnabled = enabled;
            if (!enabled) StopMusic();
        }
    }
    public enum SoundEffect { Click, Place, Remove, Error, Hint, Victory, Navigate }
    public enum MusicTrack { Menu, Akari, Hitori, Shikaku, Nurikabe }
}
