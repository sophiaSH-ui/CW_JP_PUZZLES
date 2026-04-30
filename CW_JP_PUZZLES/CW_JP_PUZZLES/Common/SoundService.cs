using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace CW_JP_PUZZLES.Common
{
    public class SoundService
    {
        private static SoundService? _instance;
        public static SoundService Instance => _instance ??= new SoundService();

        private readonly MediaPlayer _musicPlayer = new();
        private float _musicVolume = 0.4f;
        private float _sfxVolume = 0.8f;
        private bool _isMusicEnabled = true;
        private bool _isSfxEnabled = true;

        private static readonly string SoundsDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sounds");

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

            string file = track switch
            {
                MusicTrack.Menu => "menu_theme.mp3",
                MusicTrack.Akari => "akari_ambient.mp3",
                MusicTrack.Hitori => "hitori_ambient.mp3",
                MusicTrack.Shikaku => "shikaku_ambient.mp3",
                MusicTrack.Nurikabe => "nurikabe_ambient.mp3",
                _ => "menu_theme.mp3"
            };

            string path = Path.Combine(SoundsDir, file);
            if (!File.Exists(path)) return;

            _musicPlayer.Stop();
            _musicPlayer.Open(new Uri(path, UriKind.Absolute));
            _musicPlayer.Volume = _musicVolume;
            _musicPlayer.MediaEnded += OnMusicEnded; // loop
            _musicPlayer.Play();
        }

        public void StopMusic()
        {
            _musicPlayer.Stop();
            _musicPlayer.MediaEnded -= OnMusicEnded;
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

        public void PlaySfx(SoundEffect sfx)
        {
            if (!_isSfxEnabled) return;

            string file = sfx switch
            {
                SoundEffect.Click => "click.wav",
                SoundEffect.Place => "place.wav",
                SoundEffect.Remove => "remove.wav",
                SoundEffect.Error => "error.wav",
                SoundEffect.Hint => "hint.wav",
                SoundEffect.Victory => "victory.wav",
                SoundEffect.Navigate => "navigate.wav",
                _ => "click.wav"
            };

            string path = Path.Combine(SoundsDir, file);
            if (!File.Exists(path)) return;

            Task.Run(() =>
            {
                using var player = new SoundPlayer(path);
                player.PlaySync();
            });
        }

        public void SetSfxEnabled(bool enabled) => _isSfxEnabled = enabled;
        public void SetMusicEnabled(bool enabled)
        {
            _isMusicEnabled = enabled;
            if (!enabled) StopMusic();
        }
    }

    public enum MusicTrack { Menu, Akari, Hitori, Shikaku, Nurikabe }
    public enum SoundEffect { Click, Place, Remove, Error, Hint, Victory, Navigate }
}
