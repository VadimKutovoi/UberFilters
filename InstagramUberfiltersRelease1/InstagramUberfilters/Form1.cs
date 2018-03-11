using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InstagramUberfilters
{
    public partial class Form1 : Form
    {
        public string strElem = "square";
        public Form1()
        {
            InitializeComponent();
            //this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
        }

        Bitmap image;
        Stack<Bitmap> previmg = new Stack<Bitmap>();

        //open and save
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All files (*.*) | *.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                picturebox.Image = image;               
                picturebox.Refresh();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();


            if (saveFileDialog1.FileName != "")
            {

                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();

                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        picturebox.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        picturebox.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        picturebox.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }

                fs.Close();
            }
        }

        //backgroundworker logic
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                picturebox.Image = image;
                picturebox.Refresh();
            }
            progressBar1.Value = 0;
        }

        //Filters
        private void invesionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InverFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            previmg.Push(image); 
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true) image = newImage; 
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }


        private void button_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void blurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void gaussianBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackColor = Color.SteelBlue;
        }

        private void lightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackColor = Color.White;
        }

        private void darkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackColor = Color.DimGray;
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void brightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BrightnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sharpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sobelFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void scharrFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new ScharrFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void prewittFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new PrewittFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void sharpnessVar2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sharpness2Filter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void glassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void wavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new WavesFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void motionBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MotionBlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void linearStretchingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new LinearStretching();
            filter.setMaxMinBrightness(image);    
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void grayWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayWorldFilter();
            filter.setAvgBrightness(image);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (previmg.DefaultIfEmpty() != null)
            {
                image = previmg.Pop();
                picturebox.Image = image;
                picturebox.Refresh();
            }
        }

        private void medianFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Dilation(strElem);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Erosion(strElem);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void openingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Opening(strElem);
            backgroundWorker1.RunWorkerAsync(filter);
 
        }

        private void closingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter2 = new Closing(strElem);
            backgroundWorker1.RunWorkerAsync(filter2);  
        }

        private void squareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            strElem = "square";
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            strElem = "diamond";
        }

        private void bigSquareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            strElem = "bigsquare";
        }

        private void bigDiamondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            strElem = "bigdiamond";
        }

        private void gradientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Gradient(strElem);
            backgroundWorker1.RunWorkerAsync(filter);
        }
    }
}
