using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using System.Windows.Input;

namespace CW_JP_PUZZLES.UI.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly MainViewModel _main;

        public List<GameEntry> Games { get; } = new()
        {
            new("Akari",
                "Розстав лампочки так, щоб освітити кожну клітинку.",
                "🕯"),
            new("Hitori",
                "Зафарбуй клітинки, щоб числа не повторювались у рядках і стовпцях.",
                "⬛"),
            new("Shikaku",
                "Розбий поле на прямокутники за підказками.",
                "▬"),
            new("Nurikabe",
                "Побудуй острови серед чорної ріки.",
                "🏝"),
        };

        private GameEntry? _selectedGame;
        public GameEntry? SelectedGame
        {
            get => _selectedGame;
            set => SetField(ref _selectedGame, value);
        }

        public ICommand SelectGameCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public MainMenuViewModel(MainViewModel main)
        {
            _main = main;

            SelectGameCommand = new RelayCommand(obj =>
            {
                if (obj is string name)
                {
                    SoundService.Instance.PlaySfx(SoundEffect.Click);
                    _main.NavigateToDifficulty(name);
                }
            });

            OpenSettingsCommand = new RelayCommand(() =>
                _main.NavigateToSettings());
        }
    }

    public record GameEntry(string Name, string Description, string Icon);
}