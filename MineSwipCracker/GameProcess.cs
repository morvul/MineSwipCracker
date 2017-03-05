using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MineSwipCracker.Enums;
using MineSwipCracker.Models;
using MineSwipCracker.Properties;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace MineSwipCracker
{
    public class GameProcess
    {
        private const string ScreensDir = "Screenshots";
        private static readonly Random Rd = new Random();
        private readonly GamePreset _gamePreset;
        private readonly Window _appWindow;
        private IntPtr _winHandle;
        private Rectangle _winParams;
        private Task _monitoringProcess;
        private bool _isMonitoringRinning;
        private bool _isGameOver;
        private Stopwatch _timer;

        private GameProcess()
        {
            _isGameOver = false;
            _isMonitoringRinning = false;
            _monitoringProcess = new Task(Monitoring);
            UpdatedCells = new Queue<Cell>();
        }

        private GameProcess(GamePreset gamePreset, Window appWindow)
            : this()
        {
            _gamePreset = gamePreset;
            _appWindow = appWindow;
            _appWindow.LocationChanged += WindowChanged;
            _appWindow.SizeChanged += WindowChanged;
            Board = new CellType[_gamePreset.Rows, _gamePreset.Columns];
        }

        private void WindowChanged(object sender, EventArgs e)
        {
            lock (this)
            {
                _winParams = new Rectangle((int)_appWindow.Left, (int)_appWindow.Top,
                 (int)_appWindow.Width, (int)_appWindow.Height);
                if (_winHandle.ToInt32() == 0)
                {
                    _winHandle = new WindowInteropHelper(_appWindow).Handle;
                }
            }
        }

        public bool IsGameStarted { get; private set; }

        public CellType[,] Board { get; set; }

        private List<KeyValuePair<ImageContainer, CellType>> PossibleCells { get; set; } = new List<KeyValuePair<ImageContainer, CellType>>();

        public Queue<Cell> UpdatedCells { get; set; }

        public int Delay { get; set; }

        public int AnalysisAccuracy { get; set; }

        public int Dispersion { get; set; }

        public int MouseSpeed { get; set; }

        public string BoardInfo { get; set; }

        public int MaxTurnDelay { get; set; }

        public event Action OnGameStateUpdated;

        private void Update()
        {
            PossibleCells = new List<KeyValuePair<ImageContainer, CellType>>
            {
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.BombCellSprite), CellType.Bomb),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.BlastCellSprite), CellType.Blast),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.BonusCellSprite), CellType.Bonus),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.EmptyCellSprite), CellType.Empty),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.FlagCellSprite), CellType.Flag),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.FreeCellSprite), CellType.Free),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell1Sprite), CellType.Cell1),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell2Sprite), CellType.Cell2),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell3Sprite), CellType.Cell3),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell4Sprite), CellType.Cell4),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell5Sprite), CellType.Cell5),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell6Sprite), CellType.Cell6),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell7Sprite), CellType.Cell7),
                new KeyValuePair<ImageContainer, CellType>(new ImageContainer(_gamePreset.Cell8Sprite), CellType.Cell8)
            };
            TurnImage = new ImageContainer(_gamePreset.RestartSprite);
            StartImage = new ImageContainer(_gamePreset.StartSprite);
            OnGameStateUpdated?.Invoke();
        }

        public ImageContainer StartImage { get; set; }

        public ImageContainer TurnImage { get; set; }

        internal static GameProcess Initialize(GamePreset gamePreset, Window appWindow)
        {
            var gameProcess = new GameProcess(gamePreset, appWindow);
            gameProcess.Load();
            return gameProcess;
        }

        private void Load()
        {
            Delay = Settings.Default.Delay;
            AnalysisAccuracy = Settings.Default.Accuracy;
            Dispersion = Settings.Default.Dispersion;
            MouseSpeed = Settings.Default.MouseSpeed;
            MaxTurnDelay = Settings.Default.MaxTurnDelay;
        }

        public void Save()
        {
            Settings.Default.Delay = Delay;
            Settings.Default.Accuracy = AnalysisAccuracy;
            Settings.Default.Dispersion = Dispersion;
            Settings.Default.MouseSpeed = MouseSpeed;
            Settings.Default.MaxTurnDelay = MaxTurnDelay;
            Settings.Default.Save();
        }

        public void Stop()
        {
            IsGameStarted = false;
        }

        public void StopMonitoring()
        {
            _isMonitoringRinning = false;
            Stop();
        }

        private void Monitoring()
        {
            Update();
            while (_isMonitoringRinning)
            {
                var searchScreenObj = GetScreenShot();
                var isBoardUpdated = UpdateBoard(searchScreenObj);
                if (IsGameStarted)
                {
                    MakeTurn();
                }

                if (isBoardUpdated)
                {
                    OnGameStateUpdated?.Invoke();
                }
                Thread.Sleep(Delay);
            }
        }

        private ImageContainer GetScreenShot()
        {
            var screen = Screen.FromHandle(_winHandle);
            var screenShot = SearchHelper.CaptureScreen(SearchHelper.MagicShift, SearchHelper.MagicShift,
                screen.Bounds.Width, screen.Bounds.Height, _winHandle);

            #region Hide Application window on the screenshot

            Rectangle winParams;
            lock (this)
            {
                winParams = _winParams;
            }

            using (var graphics = Graphics.FromImage(screenShot))
            {
                graphics.FillRectangle(Brushes.Black, winParams.X, winParams.Y, winParams.Width,
                    winParams.Height);
            }

            #endregion
            //
            return new ImageContainer(screenShot);
        }

        private void MakeTurn()
        {
            if (_isGameOver)
            {
                Thread.Sleep(Rd.Next(MaxTurnDelay));
                StopWatch();
                return;
            }


            if (_timer == null || !_timer.IsRunning)
            {
                _timer = Stopwatch.StartNew();
                BoardInfo = "Game in progres...";
            }

            var isNoTurn = true;
            var steps = Logic.GetSteps(Board);
            foreach (var keyValuePair in steps)
            {
                foreach (var point in keyValuePair.Value)
                {
                    Thread.Sleep(Rd.Next(MaxTurnDelay));
                    ClickCell(point, keyValuePair.Key);
                    isNoTurn = false;
                }
            }

            if (isNoTurn)
            {
                StopWatch();
            }
        }

        private void StopWatch()
        {
            if (_timer != null && _timer.IsRunning)
            {
                _timer.Stop();
                BoardInfo = $"Time lapsed: {_timer.ElapsedMilliseconds / 1000.0} sec.";
            }
        }

        private void ClickCell(Point goodCell, Mouse.Buttons button)
        {
            var screenPoint = CellPosToPoint(goodCell);
            Mouse.ClickIt(screenPoint, MouseSpeed, button);
        }

        private Point CellPosToPoint(Point goodCell)
        {
            var firstCellPos = _gamePreset.FirstCell;
            var lastCellPos = _gamePreset.LastCell;
            var horCellDistance = (lastCellPos.X - firstCellPos.X) / (_gamePreset.Columns - 1);
            var verCellDistance = (lastCellPos.Y - firstCellPos.Y) / (_gamePreset.Rows - 1);
            var x = firstCellPos.X + horCellDistance * goodCell.X + horCellDistance / 2;
            var y = firstCellPos.Y + verCellDistance * goodCell.Y + verCellDistance / 2;
            return new Point(x, y);
        }

        private bool UpdateBoard(ImageContainer screen)
        {
            bool isBoardUpdated = false;
            _isGameOver = false;
            if (Board.GetLength(0) != _gamePreset.Rows || Board.GetLength(1) != _gamePreset.Columns)
            {
                Board = new CellType[_gamePreset.Rows, _gamePreset.Columns];
            }

            for (int row = 0; row < _gamePreset.Rows; row++)
            {
                for (int column = 0; column < _gamePreset.Columns; column++)
                {
                    var cell = UpdateCellType(row, column, screen);
                    _isGameOver |= cell.CellType == CellType.Bomb || cell.CellType == CellType.Blast;
                    isBoardUpdated |= SetBoardCell(cell);
                }
            }

            return isBoardUpdated;
        }

        private bool SetBoardCell(Cell cell)
        {
            if (Board[cell.Row, cell.Column] != cell.CellType)
            {
                Board[cell.Row, cell.Column] = cell.CellType;
                UpdatedCells.Enqueue(cell);
                return true;
            }

            return false;
        }

        private Cell UpdateCellType(int row, int column, ImageContainer screen)
        {
            var horCellDistance = (_gamePreset.LastCell.X - _gamePreset.FirstCell.X) / (_gamePreset.Columns - 1);
            var verCellDistance = (_gamePreset.LastCell.Y - _gamePreset.FirstCell.Y) / (_gamePreset.Rows - 1);
            var x = _gamePreset.FirstCell.X + horCellDistance * column;
            var y = _gamePreset.FirstCell.Y + verCellDistance * row;
            var width = _gamePreset.FirstCell.Width;
            var height = _gamePreset.FirstCell.Height;
            var cell = new Cell(CellType.Unknown, row, column, x, y, height, width);
            if (width == 0 || height == 0)
            {
                return cell;
            }

            var cellCenterX = cell.X + cell.Width / 2;
            var cellCenterY = cell.Y + cell.Height / 2;
            int? minDiff = null;
            foreach (var possibleCell in PossibleCells)
            {
                var image = possibleCell.Key;
                x = cellCenterX - image.Width / 2;
                y = cellCenterY - image.Height / 2;
                width = image.Width - 1;
                height = image.Height - 1;
                var diff = screen.Difference(ref image, x, y, x + width, y + height, new Size(width, height), AnalysisAccuracy);
                if (minDiff == null || minDiff.Value > diff)
                {
                    cell.X = x;
                    cell.Y = y;
                    cell.Width = possibleCell.Key.Width;
                    cell.Height = possibleCell.Key.Height;
                    cell.CellType = diff <= Dispersion ? possibleCell.Value : CellType.Unknown;
                    minDiff = diff;
                }
            }

            return cell;
        }

        public void RunMonitoring()
        {
            _isMonitoringRinning = true;
            _winHandle = new WindowInteropHelper(_appWindow).Handle;
            _winParams = new Rectangle((int)_appWindow.Left, (int)_appWindow.Top,
                (int)_appWindow.ActualWidth, (int)_appWindow.ActualHeight);
            Board = new CellType[_gamePreset.Rows, _gamePreset.Columns];
            if (_monitoringProcess.Status != TaskStatus.Running)
            {
                _monitoringProcess = Task.Factory.StartNew(Monitoring);
            }
        }

        public void Start()
        {
            IsGameStarted = true;
            RunMonitoring();
            Update();
        }
    }
}
