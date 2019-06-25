using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace StatsGraph
{
    public partial class Form1 : Form
    {
        Bitmap pic = new Bitmap(900,600);
        private int height = 250*2;
        private int width = 400*2;
        List<int> v = new List<int>(); 
        Regex rgx = new Regex(@"^(\d*)$", RegexOptions.Compiled);
        Stopwatch sw = new Stopwatch();
        string fileToMonitor;
        private double xRatio;
        private double yRatio;
        private int Color1R = 0;
        private int Color1G = 0;
        private int Color1B = 0;
        private int Color2R = 0;
        private int Color2G = 100;
        private int Color2B = 0;
        public Form1()
        {
            InitializeComponent();
        }


        private void point(Point p, Color c)
        {
            if (p.X > 0 && p.X < pic.Width && p.Y > 0 && p.Y < pic.Height)
            {
                pic.SetPixel(p.X, p.Y, c);
            }
        }

        private List<Point> line(Point p1, Point p2, Color c)
        {
            List<Point> pts = new List<Point>();
            int dx = Math.Abs(p1.X - p2.X);
            int dy = Math.Abs(p1.Y - p2.Y);
            int length = (int)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            double m = ((double)p2.Y - p1.Y) / ((double)p2.X - p1.X);
            double b = p1.Y - m * p1.X;
            for (int x = (p1.X > p2.X ? p2.X : p1.X); x < dx + (p1.X > p2.X ? p2.X : p1.X); x += 1)
            {
                int y = (int)(m * x + b);
                Point cur = new Point(x, y);
                pts.Add(cur);
                point(cur, c);
            }
            return pts;
        }

        private void shadedLine(Point p1, Point p2, Color c)
        {
            List<Point> pts = line(p1, p2, c);
            foreach (var pt in pts)
            {
                int curX = pt.X;
                for (int i = 0; i < pt.Y; i++)
                {
                    point(new Point(curX,i), c);   
                }
            }
        }

        private void updataData()
        {
            //FileStream fileStream = new FileStream(
            //    @"c:\words.txt", FileMode.OpenOrCreate,
            //    FileAccess.ReadWrite, FileShare.None);
            v = new List<int>();
            using ( StreamReader sr = new StreamReader(fileToMonitor))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    Match m = rgx.Match(line);
                    v.Add(Int32.Parse(m.Groups[1].Value));
                }
                sr.Close();
                drawData();
            }
        }

        private void drawData()
        {
            richTextBox1.Text = "";
            int max = 0;
            for (var i = 0; i < v.Count; i++)
            {
                richTextBox1.Text += $"\nel[{i}]: {v[i]}";
                if (v[i] > max)
                {
                    max = v[i];
                }
            }

            // do actuall drawing

            int elements = v.Count;
            double wiprt = (int)numericUpDown8.Value / (double)elements;
            double hiprt = (int)numericUpDown7.Value / (double)max;
            List<Point> points = new List<Point>();
            double avg = 0;
            for (var i = 0; i < v.Count; i++)
            {
                double xp = (wiprt * i + 30);
                double yp = (hiprt*v[i]+30);
                Point p = new Point((int)xp, (int)yp);
                points.Add(p);
                avg += v[i];
            }
            avg = avg / (double)v.Count;
            this.richTextBox1.Text += "\n======\n"+avg;
            pic = new Bitmap(900, 600);
            int j = 0;
            // linker ==========
            Color aColor = Color.FromArgb(Color1R, Color1G, Color1B);
            Color bColor = Color.FromArgb(Color2R, Color2G, Color2B);
            
            List<Point> numCenters = new List<Point>();
            foreach (var p in points)
            {
                //point(p, Color.Black);
                numCenters.Add(new Point((int)(p.X+wiprt/2),p.Y));
                if (j % 2 == 1)
                {
                    if (!checkBox1.Checked)
                    {
                        drawBar(p, (int)wiprt, p.Y, aColor);
                    }
                }
                else
                {
                    if (!checkBox1.Checked)
                    {
                        drawBar(p, (int)wiprt, p.Y, bColor);
                    }
                }
                j++;
            }
            if (checkBox1.Checked)
            {
                for (var i = 0; i < numCenters.Count-1; i++)
                {
                    Point cur = numCenters[i];
                    Point next = numCenters[i + 1];
                    shadedLine(cur,next,i%2==0? aColor : bColor);
                }
            }
            line(new Point(0, (int) avg+50), new Point(500, (int) avg+50), Color.Black);
            
            setImage();
        }

        private void drawBar(Point st, int width, int height, Color c)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    point(new Point(st.X + i, st.Y - j), c);
                }
            }
        }

        private void setImage()
        {
            pictureBox1.Image = pic;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; i <= 10; i++)
            {
                System.Threading.Thread.Sleep(500);
                worker.ReportProgress(i*10);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (FileToMonitor != "")
            {
                updataData();
            }
            backgroundWorker1.RunWorkerAsync();
        }
        
        public string FileToMonitor
        {
            get => fileToMonitor;
            set => fileToMonitor = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FileToMonitor = @"";
            backgroundWorker1.WorkerReportsProgress = true;
            sw.Start();
            backgroundWorker1.RunWorkerAsync();
            updateColor1();
            updateColor2();
            updatePreview();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileToMonitor = textBox1.Text;
        }

        private void updateColor1()
        {
            Bitmap bp = new Bitmap(32,32);
            for (int i = 0; i < 32; i++)
            {
                for (int i1 = 0; i1 < 32; i1++)
                {
                    bp.SetPixel(i,i1,Color.FromArgb(Color1R, Color1G, Color1B));
                }
            }
            pictureBox2.Image = bp;
            updatePreview();
        }

        private void updateColor2()
        {
            Bitmap bp = new Bitmap(32, 32);
            for (int i = 0; i < 32; i++)
            {
                for (int i1 = 0; i1 < 32; i1++)
                {
                    bp.SetPixel(i, i1, Color.FromArgb(Color2R, Color2G, Color2B));
                }
            }
            pictureBox3.Image = bp;
            updatePreview();
        }

        private void updatePreview()
        {
            Bitmap bp = new Bitmap(320, 32);
            for (int i = 0; i < 320; i++)
            {
                for (int i1 = 0; i1 < 32; i1++)
                {
                    if ((i / 10) % 2 == 0)
                    {
                        bp.SetPixel(i, i1, Color.FromArgb(Color1R, Color1G, Color1B));
                    }
                    else
                    {
                        bp.SetPixel(i, i1, Color.FromArgb(Color2R, Color2G, Color2B));
                    }
                }
            }
            pictureBox4.Image = bp;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Color1R = (int) numericUpDown1.Value;
            updateColor1();
        }
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Color1G = (int) numericUpDown2.Value;
            updateColor1();
        }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Color1B = (int) numericUpDown3.Value;
            updateColor1();
        }
        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            Color2R = (int) numericUpDown6.Value;
            updateColor2();
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Color2G = (int) numericUpDown5.Value;
            updateColor2();
        }
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Color2B = (int) numericUpDown4.Value;
            updateColor2();
        }
    }
}