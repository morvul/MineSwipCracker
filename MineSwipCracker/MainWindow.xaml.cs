using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MineSwipCracker.Enums;
using MineSwipCracker.Properties;
using Image = System.Windows.Controls.Image;

namespace MineSwipCracker
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private GamePreset _gamePreset;
        private GameProcess _gameProcess;

        #region Main window

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            InitializePresetTab();
            InitializeGameProcessTab();
        }

        private void InitializeWindow()
        {
            Width = Settings.Default.WinWidth;
            Height = Settings.Default.WinHeight;
            Top = Settings.Default.WinTop;
            Left = Settings.Default.WinLeft;
            Topmost = Settings.Default.TopMost;
            SetOnTopFlag.IsChecked = Topmost;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _gameProcess.Save();
            Settings.Default.WinWidth = Width;
            Settings.Default.WinHeight = Height;
            Settings.Default.WinTop = Top;
            Settings.Default.WinLeft = Left;
            Settings.Default.TopMost = Topmost;
            Settings.Default.Save();
            base.OnClosing(e);
        }

        #endregion

        #region Game process tab

        private void InitializeGameProcessTab()
        {
            _gameProcess = GameProcess.Initialize(_gamePreset, this);
            DelayField.Text = _gameProcess.Delay.ToString();
            AccuracyField.Text = _gameProcess.AnalysisAccuracy.ToString();
            DispersionField.Text = _gameProcess.Dispersion.ToString();
            MouseSpeedField.Text = _gameProcess.MouseSpeed.ToString();
            MaxTurnDelayField.Text = _gameProcess.MaxTurnDelay.ToString();
            _gameProcess.OnGameStateUpdated += OnGameStateUpdated;
            if (!GameProcessTab.IsSelected)
            {
                TabControl.SelectedIndex = 1;
            }
        }

        private void OnGameStateUpdated()
        {
            Dispatcher.BeginInvoke(new Action(UpdateBoard));
        }

        private void UpdateBoard()
        {
            var needRebuild = Board.Columns != _gamePreset.Columns || Board.Rows != _gamePreset.Rows;
            if (needRebuild)
            {
                Board.Children.Clear();
                Board.Columns = _gamePreset.Columns;
                Board.Rows = _gamePreset.Rows;
                for (var row = 0; row < _gamePreset.Rows; row++)
                {
                    for (var column = 0; column < _gamePreset.Columns; column++)
                    {
                        var cell = new Image();
                        Board.Children.Add(cell);
                        SetBoardCell(row, column, CellType.Unknown);
                    }
                }
            }

            if (_gameProcess == null)
            {
                return;
            }

            BoardInfoField.Text = _gameProcess.BoardInfo;
            while (_gameProcess.UpdatedCells.Count > 0)
            {
                var updatedCell = _gameProcess.UpdatedCells.Dequeue();
                SetBoardCell(updatedCell.Row, updatedCell.Column, updatedCell.CellType);
            }
        }

        private void SetBoardCell(int row, int column, CellType cellType)
        {
            var cell = (Image)Board.Children[Board.Columns * row + column];
            switch (cellType)
            {
                case CellType.Unknown:
                    cell.Source = new BitmapImage(new Uri("/Resources/11.gif", UriKind.Relative));
                    break;
                case CellType.Free:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.FreeCellSprite);
                    break;
                case CellType.Bomb:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.BombCellSprite);
                    break;
                case CellType.Blast:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.BlastCellSprite);
                    break;
                case CellType.Bonus:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.BonusCellSprite);
                    break;
                case CellType.Empty:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.EmptyCellSprite);
                    break;
                case CellType.Flag:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.FlagCellSprite);
                    break;
                case CellType.Cell1:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell1Sprite);
                    break;
                case CellType.Cell2:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell2Sprite);
                    break;
                case CellType.Cell3:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell3Sprite);
                    break;
                case CellType.Cell4:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell4Sprite);
                    break;
                case CellType.Cell5:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell5Sprite);
                    break;
                case CellType.Cell6:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell6Sprite);
                    break;
                case CellType.Cell7:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell7Sprite);
                    break;
                case CellType.Cell8:
                    cell.Source = SearchHelper.BitmapToImageSource(_gamePreset.Cell8Sprite);
                    break;
            }
        }

        private void UpdateGameProcessState()
        {
            if (_gameProcess == null)
            { return; }
            if (!_gamePreset.IsReady())
            {
                TabControl.SelectedIndex = 0;
                _gameProcess.StopMonitoring();
                return;
            }

            UpdateBoard();
            if (GameProcessTab.IsSelected)
            {
                _gameProcess.RunMonitoring();
            }
            else
            {
                _gameProcess.StopMonitoring();
            }

            StartCommand.Visibility = _gameProcess.IsGameStarted ? Visibility.Collapsed : Visibility.Visible;
            StopCommand.Visibility = _gameProcess.IsGameStarted ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGameProcessState();
        }


        private void StartCommand_OnClick(object sender, RoutedEventArgs e)
        {
            StartCommand.Visibility = Visibility.Collapsed;
            StopCommand.Visibility = Visibility.Visible;
            _gameProcess.Start();
        }


        private void StopCommand_OnClick(object sender, RoutedEventArgs e)
        {
            StopCommand.Visibility = Visibility.Collapsed;
            StartCommand.Visibility = Visibility.Visible;
            _gameProcess.Stop();
        }

        private void DelayChanged(object sender, TextChangedEventArgs e)
        {
            if (DelayField.Text == "")
            {
                DelayField.Text = "0";
            }

            _gameProcess.Delay = int.Parse(DelayField.Text);
        }


        private void AccuracyChanged(object sender, TextChangedEventArgs e)
        {
            if (AccuracyField.Text == "")
            {
                AccuracyField.Text = "0";
            }

            _gameProcess.AnalysisAccuracy = int.Parse(AccuracyField.Text);

            if (_gameProcess.AnalysisAccuracy > 10)
            {
                _gameProcess.AnalysisAccuracy = 10;
                AccuracyField.Text = "10";
            }
        }


        private void DispersionChanged(object sender, TextChangedEventArgs e)
        {
            if (DispersionField.Text == "")
            {
                DispersionField.Text = "0";
            }

            _gameProcess.Dispersion = int.Parse(DispersionField.Text);
        }

        private void MouseSpeedChanged(object sender, TextChangedEventArgs e)
        {
            if (MouseSpeedField.Text == "")
            {
                MouseSpeedField.Text = "0";
            }

            _gameProcess.MouseSpeed = int.Parse(MouseSpeedField.Text);
        }


        private void MaxTurnDelayFieldChanged(object sender, TextChangedEventArgs e)
        {
            if (MaxTurnDelayField.Text == "")
            {
                MaxTurnDelayField.Text = "0";
            }

            _gameProcess.MaxTurnDelay = int.Parse(MaxTurnDelayField.Text);
        }

        #endregion

        #region Preset tab

        private void InitializePresetTab()
        {
            _gamePreset = GamePreset.Initialize();
            UpdatePresetControls();
        }

        private void UpdatePresetControls()
        {
            SavePresetCommand.IsEnabled = _gamePreset.HasChanges;
            CancelPresetCommand.IsEnabled = _gamePreset.HasChanges;
            BoardRowsField.Text = _gamePreset.Rows.ToString();
            BoardColumnsField.Text = _gamePreset.Columns.ToString();
            UpdateSpiteControlls(StartSprite, StartSpriteText, _gamePreset.StartSprite);
            UpdateSpiteControlls(RestartSprite, RestartSpriteText, _gamePreset.RestartSprite);
            UpdateSpiteControlls(FreeCellSprite, FreeCellSpriteText, _gamePreset.FreeCellSprite);
            UpdateSpiteControlls(BlastCellSprite, BlastCellSpriteText, _gamePreset.BlastCellSprite);
            UpdateSpiteControlls(BombCellSprite, BombCellSpriteText, _gamePreset.BombCellSprite);
            UpdateSpiteControlls(EmptyCellSprite, EmptyCellSpriteText, _gamePreset.EmptyCellSprite);
            UpdateSpiteControlls(FlagCellSprite, FlagCellSpriteText, _gamePreset.FlagCellSprite);
            UpdateSpiteControlls(BonusCellSprite, BonusCellSpriteText, _gamePreset.BonusCellSprite);
            UpdateSpiteControlls(Cell1Sprite, Cell1SpriteText, _gamePreset.Cell1Sprite);
            UpdateSpiteControlls(Cell2Sprite, Cell2SpriteText, _gamePreset.Cell2Sprite);
            UpdateSpiteControlls(Cell3Sprite, Cell3SpriteText, _gamePreset.Cell3Sprite);
            UpdateSpiteControlls(Cell4Sprite, Cell4SpriteText, _gamePreset.Cell4Sprite);
            UpdateSpiteControlls(Cell5Sprite, Cell5SpriteText, _gamePreset.Cell5Sprite);
            UpdateSpiteControlls(Cell6Sprite, Cell6SpriteText, _gamePreset.Cell6Sprite);
            UpdateSpiteControlls(Cell7Sprite, Cell7SpriteText, _gamePreset.Cell7Sprite);
            UpdateSpiteControlls(Cell8Sprite, Cell8SpriteText, _gamePreset.Cell8Sprite);
            GameProcessTab.IsEnabled = _gamePreset.IsReady();
            FirstCellField.Text = _gamePreset.FirstCell.ToString();
            LastCellField.Text = _gamePreset.LastCell.ToString();
        }

        private void UpdateSpiteControlls(Image spriteControl, TextBlock spriteTextControl, Bitmap bitmap)
        {
            spriteControl.Source = SearchHelper.BitmapToImageSource(bitmap);
            if (bitmap != null)
            {
                spriteControl.MaxHeight = bitmap.Height;
                spriteControl.MaxWidth = bitmap.Width;
            }

            spriteControl.Visibility = bitmap != null
                ? Visibility.Visible
                : Visibility.Collapsed;
            spriteTextControl.Visibility = bitmap != null
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void StartSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.StartSprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void FirstCellField_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.FirstCell = screener.Rectangle;
                UpdatePresetControls();
            }
        }


        private void RestartSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.RestartSprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void Cell1SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell1Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void Cell2SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell2Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void Cell3SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell3Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void Cell4SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell4Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void Cell5SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell5Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void Cell6SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell6Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }
        private void Cell7SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell7Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }
        private void Cell8SpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.Cell8Sprite = screener.Picture;
                UpdatePresetControls();
            }
        }


        private void FreeCellSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.FreeCellSprite = screener.Picture;
                UpdatePresetControls();
            }
        }


        private void EmptyCellSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.EmptyCellSprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void BombCellSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.BombCellSprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void BlastCellSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.BlastCellSprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void FlagCellSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.FlagCellSprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void BonusCellSpriteSelectionCommand_Click(object sender, RoutedEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.BonusCellSprite = screener.Picture;
                UpdatePresetControls();
            }
        }

        private void LastCellField_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ScreenshotRegion screener = new ScreenshotRegion(this);
            if (screener.ShowDialog() == true)
            {
                _gamePreset.LastCell = screener.Rectangle;
                UpdatePresetControls();
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                Regex regex = new Regex("[^0-9]+");
                if (text != null && regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void BoardRowsFieldChanged(object sender, TextChangedEventArgs e)
        {
            int rows;
            if (int.TryParse(BoardRowsField.Text, out rows))
            {
                _gamePreset.Rows = rows;
                UpdatePresetControls();
            }
        }

        private void BoardColumnsFieldChanged(object sender, TextChangedEventArgs e)
        {
            int columns;
            if (int.TryParse(BoardColumnsField.Text, out columns))
            {
                _gamePreset.Columns = columns;
                UpdatePresetControls();
            }
        }

        private void SavePresetCommand_Click(object sender, RoutedEventArgs e)
        {
            _gamePreset.Save();
            UpdatePresetControls();
        }

        private void CancelPresetCommand_Click(object sender, RoutedEventArgs e)
        {
            _gamePreset.Reload();
            UpdatePresetControls();
        }

        private void ResetPresetCommand_Click(object sender, RoutedEventArgs e)
        {
            _gamePreset.Reset();
            UpdatePresetControls();
        }

        private void OpenPresetDirCommand_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(_gamePreset.DirectoryPath))
            {
                Directory.CreateDirectory(_gamePreset.DirectoryPath);
            }

            Process.Start(_gamePreset.DirectoryPath);
        }

        #endregion


        private void SetOnTopFlag_Click(object sender, RoutedEventArgs e)
        {
            Topmost = SetOnTopFlag.IsChecked == true;
        }
    }
}
