using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlideShow
{
    public partial class SlideShow : Form
    {
        public static bool Debugging = false;
        public string SourceDirectory = @"C:\Shirley\Pictures\Wedding\Combined";
        public bool Repeat = true;
        public int PerImageSleep = 4000;
        public static Font LabelFont = new Font("Arial", 20, FontStyle.Regular);

        bool ExitRequest = false;
        bool Paused = false;

        public Brush BackgroundBrush = Brushes.Black;

        List<string> Filenames = new List<string>();
        Bitmap CurrentBitmap = null;
        int CurrentNumber = 0;
        int TotalNumber = 0;

        public SlideShow()
        {
            InitializeComponent();

            Task.Factory.StartNew(RunSlideshow, TaskCreationOptions.LongRunning);

            Select();
            if (!Debugging)
                Cursor.Hide();
        }

        private void SlideShow_Load(object sender, EventArgs e)
        {
            if (!Debugging)
            {
                TopMost = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            DoubleBuffered = true;
        }

        private void SlideShow_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.FillRectangle(BackgroundBrush, new Rectangle(0,0,10000,10000));

            var b = CurrentBitmap;
            if (b != null)
            {
                var bitmapAspectRatio = (float)b.Height / b.Width;
                var windowAspectRatio = (float)Height / Width;
                if (bitmapAspectRatio < windowAspectRatio)
                {   // image will extend across width
                    var width = Width;
                    var height = (int)(width * bitmapAspectRatio);
                    var rect = new Rectangle(0, 0, width, height);
                    rect = CenterRectange(rect);
                    g.DrawImage(CurrentBitmap, rect);
                }
                else
                {   // image will extend across height
                    var height = Height;
                    var width = (int)(height / bitmapAspectRatio);
                    var rect = new Rectangle(0, 0, width, height);
                    rect = CenterRectange(rect);
                    g.DrawImage(CurrentBitmap, rect);
                }
            }

            var str = $"{CurrentNumber+1} / {TotalNumber}" + (Paused ? " Paused" : "");
            g.DrawString(str, LabelFont, Brushes.White, 10, Height - 80);
        }

        private Rectangle CenterRectange(Rectangle r)
        {
            var offsetx = (int)((Bounds.Width - r.Width) / 2f);
            var offsety = (int)((Bounds.Height - r.Height) / 2f);
            r.Offset(offsetx, offsety);
            return r;
        }

        private void RunSlideshow()
        {
            Filenames = Directory.EnumerateFiles(SourceDirectory).OrderBy(f => f).ToList();
            TotalNumber = Filenames.Count;
            CurrentNumber = -1;

            while (!ExitRequest)
            {
                if (!Paused)
                {
                    SetCurrentNumber(CurrentNumber + 1);
                }
                Thread.Sleep(PerImageSleep);
            }
        }

        void SetCurrentNumber(int n)
        {
            if (n < 0)
                n = n + TotalNumber * (1 + Math.Abs(n / TotalNumber));
            var j = n / TotalNumber;
            CurrentNumber = n - TotalNumber * j;
            var filename = Filenames[CurrentNumber];
            SetBitmap(Image.FromFile(filename) as Bitmap);
        }

        void SetBitmap(Bitmap b)
        {
            var lastBitmap = CurrentBitmap;
            CurrentBitmap = b;
            if (lastBitmap != null)
                lastBitmap.Dispose();
            Invalidate();
        }

        private void SlideShow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Paused = !Paused;
                SetCurrentNumber(CurrentNumber);
            }
            else if (e.KeyCode == Keys.Left)
            {
                Paused = true;
                SetCurrentNumber(CurrentNumber - 1);
            }
            else if (e.KeyCode == Keys.Right)
            {
                Paused = true;
                SetCurrentNumber(CurrentNumber + 1);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ExitRequest = true;
                Close();
            }
        }

        private void SlideShow_Resize(object sender, EventArgs e)
        {
            if (Debugging)
                Invalidate();
        }
    }
}
