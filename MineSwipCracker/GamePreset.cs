using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using MineSwipCracker.Properties;

namespace MineSwipCracker
{
    public class GamePreset
    {
        private const string GamePresetDir = "GamePreset";
        private const string ImgExt = ".bmp";
        private Bitmap _startSprite;
        private int _columns;
        private int _rows;
        private Bitmap _restartSprite;
        private Rectangle _firstCell;
        private Rectangle _lastCell;
        private Bitmap _flagCellSprite;
        private Bitmap _blastCellSprite;
        private Bitmap _bombCellSprite;
        private Bitmap _emptyCellSprite;
        private Bitmap _freeCellSprite;
        private Bitmap _cell8Sprite;
        private Bitmap _cell7Sprite;
        private Bitmap _cell6Sprite;
        private Bitmap _cell5Sprite;
        private Bitmap _cell4Sprite;
        private Bitmap _cell3Sprite;
        private Bitmap _cell2Sprite;
        private Bitmap _cell1Sprite;
        private Bitmap _bonusCellSprite;

        public bool HasChanges { get; set; }

        public int Columns
        {
            get { return _columns; }
            set
            {
                if (_columns != value)
                {
                    _columns = value;
                    HasChanges = true;
                }
            }
        }

        public int Rows
        {
            get { return _rows; }
            set
            {
                if (_rows != value)
                {
                    _rows = value;
                    HasChanges = true;
                }
            }
        }

        public string DirectoryPath => GamePresetDir;

        public Rectangle FirstCell
        {
            get { return _firstCell; }
            set
            {
                if (_firstCell != value)
                {
                    _firstCell = value;
                    HasChanges = true;
                }
            }
        }

        public Rectangle LastCell
        {
            get { return _lastCell; }
            set
            {
                if (_lastCell != value)
                {
                    _lastCell = value;
                    HasChanges = true;
                }
            }
        }


        public Bitmap StartSprite
        {
            get { return _startSprite; }
            set
            {
                if (_startSprite != value)
                {
                    _startSprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap RestartSprite
        {
            get { return _restartSprite; }
            set
            {
                if (_restartSprite != value)
                {
                    _restartSprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap Cell1Sprite
        {
            get { return _cell1Sprite; }
            set
            {
                if (_cell1Sprite != value)
                {
                    _cell1Sprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap Cell2Sprite
        {
            get { return _cell2Sprite; }
            set
            {
                if (_cell2Sprite != value)
                {
                    _cell2Sprite = value;
                    HasChanges = true;
                }
            }
        }
        public Bitmap Cell3Sprite
        {
            get { return _cell3Sprite; }
            set
            {
                if (_cell3Sprite != value)
                {
                    _cell3Sprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap Cell4Sprite
        {
            get { return _cell4Sprite; }
            set
            {
                if (_cell4Sprite != value)
                {
                    _cell4Sprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap Cell5Sprite
        {
            get { return _cell5Sprite; }
            set
            {
                if (_cell5Sprite != value)
                {
                    _cell5Sprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap Cell6Sprite
        {
            get { return _cell6Sprite; }
            set
            {
                if (_cell6Sprite != value)
                {
                    _cell6Sprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap Cell7Sprite
        {
            get { return _cell7Sprite; }
            set
            {
                if (_cell7Sprite != value)
                {
                    _cell7Sprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap Cell8Sprite
        {
            get { return _cell8Sprite; }
            set
            {
                if (_cell8Sprite != value)
                {
                    _cell8Sprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap FreeCellSprite
        {
            get { return _freeCellSprite; }
            set
            {
                if (_freeCellSprite != value)
                {
                    _freeCellSprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap EmptyCellSprite
        {
            get { return _emptyCellSprite; }
            set
            {
                if (_emptyCellSprite != value)
                {
                    _emptyCellSprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap BombCellSprite
        {
            get { return _bombCellSprite; }
            set
            {
                if (_bombCellSprite != value)
                {
                    _bombCellSprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap BlastCellSprite
        {
            get { return _blastCellSprite; }
            set
            {
                if (_blastCellSprite != value)
                {
                    _blastCellSprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap FlagCellSprite
        {
            get { return _flagCellSprite; }
            set
            {
                if (_flagCellSprite != value)
                {
                    _flagCellSprite = value;
                    HasChanges = true;
                }
            }
        }

        public Bitmap BonusCellSprite
        {
            get { return _bonusCellSprite; }
            set
            {
                if (_bonusCellSprite != value)
                {
                    _bonusCellSprite = value;
                    HasChanges = true;
                }
            }
        }

        public bool IsReady()
        {
            return StartSprite != null && RestartSprite != null && FreeCellSprite != null && BlastCellSprite != null
                && BombCellSprite != null && EmptyCellSprite != null && FlagCellSprite != null
                && Cell1Sprite != null && Cell2Sprite != null && Cell3Sprite != null && Cell4Sprite != null
                && Cell5Sprite != null && Cell6Sprite != null && Cell7Sprite != null && Cell8Sprite != null
                && Rows > 0 && Columns > 0 && FirstCell != Rectangle.Empty && LastCell != Rectangle.Empty;
        }

        public void Reset()
        {
            StartSprite = null;
            RestartSprite = null;
            FreeCellSprite = null;
            BlastCellSprite = null;
            BombCellSprite = null;
            EmptyCellSprite = null;
            FlagCellSprite = null;
            BonusCellSprite = null;
            Cell1Sprite = null;
            Cell2Sprite = null;
            Cell3Sprite = null;
            Cell4Sprite = null;
            Cell5Sprite = null;
            Cell6Sprite = null;
            Cell7Sprite = null;
            Cell8Sprite = null;
            Rows = 0;
            Columns = 0;
            LastCell = Rectangle.Empty;
            FirstCell = Rectangle.Empty;
        }

        public void Reload()
        {
            Rows = Settings.Default.Rows;
            FirstCell = Settings.Default.FirstCell;
            LastCell = Settings.Default.LastCell;
            Columns = Settings.Default.Columns;
            try
            {
                var presetDir = Path.Combine(Environment.CurrentDirectory, GamePresetDir);
                if (Directory.Exists(presetDir))
                {
                    StartSprite = FromFile(nameof(StartSprite), presetDir);
                    RestartSprite = FromFile(nameof(RestartSprite), presetDir);
                    FreeCellSprite = FromFile(nameof(FreeCellSprite), presetDir);
                    BlastCellSprite = FromFile(nameof(BlastCellSprite), presetDir);
                    BombCellSprite = FromFile(nameof(BombCellSprite), presetDir);
                    EmptyCellSprite = FromFile(nameof(EmptyCellSprite), presetDir);
                    FlagCellSprite = FromFile(nameof(FlagCellSprite), presetDir);
                    BonusCellSprite = FromFile(nameof(BonusCellSprite), presetDir);
                    Cell1Sprite = FromFile(nameof(Cell1Sprite), presetDir);
                    Cell2Sprite = FromFile(nameof(Cell2Sprite), presetDir);
                    Cell3Sprite = FromFile(nameof(Cell3Sprite), presetDir);
                    Cell4Sprite = FromFile(nameof(Cell4Sprite), presetDir);
                    Cell5Sprite = FromFile(nameof(Cell5Sprite), presetDir);
                    Cell6Sprite = FromFile(nameof(Cell6Sprite), presetDir);
                    Cell7Sprite = FromFile(nameof(Cell7Sprite), presetDir);
                    Cell8Sprite = FromFile(nameof(Cell8Sprite), presetDir);
                }
            }
            catch (Exception excpt)
            {
                MessageBox.Show(excpt.Message, "Data initialization error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            HasChanges = false;
        }

        public static GamePreset Initialize()
        {
            var gamePreset = new GamePreset();
            gamePreset.Reload();
            return gamePreset;
        }

        public void Save()
        {
            if (!HasChanges)
            {
                return;
            }

            Settings.Default.Rows = Rows;
            Settings.Default.Columns = Columns;
            Settings.Default.LastCell = LastCell;
            Settings.Default.FirstCell = FirstCell;
            Settings.Default.Save();
            SaveImage(StartSprite, nameof(StartSprite));
            SaveImage(RestartSprite, nameof(RestartSprite));
            SaveImage(FreeCellSprite, nameof(FreeCellSprite));
            SaveImage(BlastCellSprite, nameof(BlastCellSprite));
            SaveImage(BombCellSprite, nameof(BombCellSprite));
            SaveImage(EmptyCellSprite, nameof(EmptyCellSprite));
            SaveImage(FlagCellSprite, nameof(FlagCellSprite));
            SaveImage(BonusCellSprite, nameof(BonusCellSprite));
            SaveImage(Cell1Sprite, nameof(Cell1Sprite));
            SaveImage(Cell2Sprite, nameof(Cell2Sprite));
            SaveImage(Cell3Sprite, nameof(Cell3Sprite));
            SaveImage(Cell4Sprite, nameof(Cell4Sprite));
            SaveImage(Cell5Sprite, nameof(Cell5Sprite));
            SaveImage(Cell6Sprite, nameof(Cell6Sprite));
            SaveImage(Cell7Sprite, nameof(Cell7Sprite));
            SaveImage(Cell8Sprite, nameof(Cell8Sprite));
            HasChanges = false;
        }

        private void SaveImage(Bitmap sprite, string spriteName)
        {
            var presetDir = Path.Combine(Environment.CurrentDirectory, GamePresetDir);
            if (!Directory.Exists(presetDir))
            {
                Directory.CreateDirectory(presetDir);
            }

            var spriteFileName = Path.Combine(presetDir, spriteName + ImgExt);
            sprite?.Save(spriteFileName, ImageFormat.Bmp);
        }

        private Bitmap FromFile(string imgName, string dirName = null)
        {
            var fileName = dirName == null ? imgName : Path.Combine(dirName, imgName + ImgExt);
            if (fileName.EndsWith(ImgExt) && File.Exists(fileName))
            {
                var bytes = File.ReadAllBytes(fileName);
                var ms = new MemoryStream(bytes);
                var img = (Bitmap)Image.FromStream(ms);
                return img;
            }

            return null;
        }
    }
}
