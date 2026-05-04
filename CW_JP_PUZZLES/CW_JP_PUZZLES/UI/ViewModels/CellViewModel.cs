using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CW_JP_PUZZLES.Common;
using System.Windows.Input;

namespace CW_JP_PUZZLES.UI.ViewModels
{
    public class CellViewModel : ViewModelBase
    {
        public int Row { get; }
        public int Col { get; }

        private readonly string _gameName;
        private readonly Action<int, int, object?> _onAction;

        private bool _isWall;
        public bool IsWall
        {
            get => _isWall;
            set => SetField(ref _isWall, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetField(ref _hasError, value);
        }
        private bool _isBlackened;
        public bool IsBlackened
        {
            get => _isBlackened;
            set => SetField(ref _isBlackened, value);
        }

        private string _displayValue = "";
        public string DisplayValue
        {
            get => _displayValue;
            set => SetField(ref _displayValue, value);
        }

        private string _clueValue = "";
        public string ClueValue
        {
            get => _clueValue;
            set => SetField(ref _clueValue, value);
        }

        private bool _hasBulb;
        public bool HasBulb
        {
            get => _hasBulb;
            set => SetField(ref _hasBulb, value);
        }

        private bool _isIlluminated;
        public bool IsIlluminated
        {
            get => _isIlluminated;
            set => SetField(ref _isIlluminated, value);
        }

        private int _wallNumber = -1;
        public int WallNumber
        {
            get => _wallNumber;
            set
            {
                SetField(ref _wallNumber, value);
                OnPropertyChanged(nameof(WallNumberText));
            }
        }

        public string WallNumberText => WallNumber >= 0 ? WallNumber.ToString() : "";

        private bool _isCircled;
        public bool IsCircled
        {
            get => _isCircled;
            set => SetField(ref _isCircled, value);
        }

        private int _regionId = -1;
        public int RegionId
        {
            get => _regionId;
            set
            {
                SetField(ref _regionId, value);
                OnPropertyChanged(nameof(RegionColor));
            }
        }

        public string RegionColor => RegionId < 0
            ? "Transparent"
            : RegionColors[RegionId % RegionColors.Length];

        private static readonly string[] RegionColors =
        {
            "#3a7bd5", "#e84393", "#f7971e", "#21d4fd",
            "#b721ff", "#08f76e", "#ff6b35", "#00d2ff"
        };
        
        private int _islandId = -1;
        public int IslandId
        {
            get => _islandId;
            set => SetField(ref _islandId, value);
        }
        
        public ICommand LeftClickCommand { get; }
        public ICommand RightClickCommand { get; }

        public CellViewModel(int row, int col, string gameName,
            Action<int, int, object?> onAction)
        {
            Row = row;
            Col = col;
            _gameName = gameName;
            _onAction = onAction;

            LeftClickCommand = new RelayCommand(() =>
            {
                object? data = _gameName switch
                {
                    "Hitori" => "black",
                    "Nurikabe" => "black",
                    _ => null
                };
                _onAction(Row, Col, data);
            });

            RightClickCommand = new RelayCommand(() =>
            {
                object? data = _gameName switch
                {
                    "Hitori" => "circle",
                    "Nurikabe" => "white",
                    "Shikaku" => "remove",
                    _ => null
                };
                _onAction(Row, Col, data);
            });
        }
    }
}