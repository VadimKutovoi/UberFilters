using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramUberfilters
{
    abstract class Filters
    {
        public float minBrightness = 1.0f;
        public float maxBrightness = 0.0f;
        public float avgBrightnessR = 0;
        public float avgBrightnessG = 0;
        public float avgBrightnessB = 0;
        public float avgBrightness = 0;

        public void setMaxMinBrightness(Bitmap sourceImage)
        {
            for (int i = 0; i < sourceImage.Width; i++)
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    if (minBrightness > sourceImage.GetPixel(i, j).GetBrightness())
                        minBrightness = sourceImage.GetPixel(i, j).GetBrightness();
                    if (maxBrightness < sourceImage.GetPixel(i, j).GetBrightness())
                        maxBrightness = sourceImage.GetPixel(i, j).GetBrightness();
                }
        }

        public void setAvgBrightness(Bitmap sourceImage)
        {
            for (int i = 0; i < sourceImage.Width; i++)
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    avgBrightnessR += sourceImage.GetPixel(i,j).R;
                    avgBrightnessG += sourceImage.GetPixel(i, j).G;
                    avgBrightnessB += sourceImage.GetPixel(i, j).B;
                }
            avgBrightnessR /= sourceImage.Width * sourceImage.Height;
            avgBrightnessG /= sourceImage.Width * sourceImage.Height;
            avgBrightnessB /= sourceImage.Width * sourceImage.Height;

            avgBrightness = (avgBrightnessR + avgBrightnessG + avgBrightnessB) / 3;
        }

        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending) return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

    }

    class InverFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(
                255 - sourceColor.R,
                255 - sourceColor.G,
                255 - sourceColor.B);

            return resultColor;
        }
    }
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(intensity, intensity, intensity);

            return resultColor;
        }
    }
    class LinearStretching : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float brightness;
            float resbrightness = 0;

            brightness = sourceImage.GetPixel(x, y).GetBrightness();
            resbrightness = (brightness - minBrightness) * 255 / (maxBrightness - minBrightness);

            Color sourceColor = sourceImage.GetPixel(x, y);

            Color resultColor = Color.FromArgb(Clamp((int)resbrightness, 0, 255),
               Clamp((int)resbrightness, 0, 255),
               Clamp((int)resbrightness, 0, 255));

            return resultColor;
        }
    }
    class GrayWorldFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            Color resultColor = Color.FromArgb(Clamp((int)(sourceColor.R * avgBrightness/avgBrightnessR), 0, 255),
               Clamp((int)(sourceColor.G * avgBrightness / avgBrightnessG), 0, 255),
               Clamp((int)(sourceColor.B * avgBrightness / avgBrightnessB), 0, 255));

            return resultColor;
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 10;
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(
                Clamp(intensity + 2 * k, 0, 255),
                Clamp((int)(intensity + 0.5), 0, 255),
                Clamp(intensity - 1, 0, 255)
                );

            return resultColor;
        }
    }

    class BrightnessFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int c = 15;

            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(
                Clamp(sourceColor.R + c, 0, 255),
                Clamp(sourceColor.G + c, 0, 255),
                Clamp(sourceColor.B + c, 0, 255)
                );

            return resultColor;
        }
    }

    class GlassFilter : Filters
    {
        Random rand = new Random();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double k = x + (rand.NextDouble() - 0.5) * 10, l = y + (rand.NextDouble() - 0.5) * 10;

            Color sourceColor = sourceImage.GetPixel(
                Clamp((int)k, 0, sourceImage.Width - 1), 
                Clamp((int)l, 0, sourceImage.Height - 1));
       
            return sourceColor;
        }
    }

    class WavesFilter : Filters
    {
        Random rand = new Random();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double l = y, k = x + 20 * Math.Sin((2 * 3.14 * l) / 60);

            Color sourceColor = sourceImage.GetPixel(
                Clamp((int)k, 0, sourceImage.Width - 1),
                Clamp((int)l, 0, sourceImage.Height - 1));

            return sourceColor;
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255), 
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;

            kernel = new float[sizeX, sizeY];

            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (sizeX * sizeY);
        }   
    }

    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            int size = 3;

            kernel = new float[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    if(i != j ) kernel[i, j] = 0.0f;
                    if(i == j) kernel[i, j] = 1.0f / size;
                }
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;

            for(int i = -radius; i <=radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / sigma * sigma));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }

        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            kernel = new float[3, 3] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } }; 
        }
    }

    class Sharpness2Filter : MatrixFilter
    {
        public Sharpness2Filter()
        {
            kernel = new float[3, 3] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
        }
    }

    

    class SobelFilter : MatrixFilter
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float R1 = 0, G1 = 0, B1 = 0, R2 = 0, G2 = 0, B2 = 0;

            kernel = new float[3, 3] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    R1 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    G1 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    B1 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            kernel = new float[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    R2 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    G2 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    B2 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            return Color.FromArgb(
                Clamp((int)Math.Sqrt(R1 * R1  + R2 * R2), 0, 255),
                Clamp((int)Math.Sqrt(G1 * G1 + G2 * G2), 0, 255),
                Clamp((int)Math.Sqrt(B1 * B1 + B2 * B2), 0, 255)
                );

        }
    }
    
    class ScharrFilter : MatrixFilter
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float R1 = 0, G1 = 0, B1 = 0, R2 = 0, G2 = 0, B2 = 0;

            kernel = new float[3, 3] { { 3, 10, 3 }, { 0, 0, 0 }, { -3, -10, -3 } };

            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    R1 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    G1 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    B1 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            kernel = new float[3, 3] { { 3, 0, -3 }, { 10, 0, -10 }, { 3, 0, -3 } };

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    R2 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    G2 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    B2 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            return Color.FromArgb(
                Clamp((int)Math.Sqrt(R1 * R1 + R2 * R2), 0, 255),
                Clamp((int)Math.Sqrt(G1 * G1 + G2 * G2), 0, 255),
                Clamp((int)Math.Sqrt(B1 * B1 + B2 * B2), 0, 255)
                );

        }
    }

    class PrewittFilter : MatrixFilter
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float R1 = 0, G1 = 0, B1 = 0, R2 = 0, G2 = 0, B2 = 0;

            kernel = new float[3, 3] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };

            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    R1 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    G1 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    B1 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            kernel = new float[3, 3] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    R2 += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    G2 += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    B2 += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            return Color.FromArgb(
                Clamp((int)Math.Sqrt(R1 * R1 + R2 * R2), 0, 255),
                Clamp((int)Math.Sqrt(G1 * G1 + G2 * G2), 0, 255),
                Clamp((int)Math.Sqrt(B1 * B1 + B2 * B2), 0, 255)
                );

        }
    }


    class MedianFilter : MatrixFilter
    {
        int kernelSize;
        int kernelRadius;
        int[] ColorsR;
        int[] ColorsG;
        int[] ColorsB;

        public MedianFilter(int size = 30)
        {
            kernelSize = size;
            kernelRadius = ((int)Math.Sqrt(size) - 1) / 2;
            ColorsR = new int[kernelSize];
            ColorsG = new int[kernelSize];
            ColorsB = new int[kernelSize];
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int i = 0;

            for (int l = -kernelRadius; l < kernelRadius + 1; l++)
                for (int k = -kernelRadius; k < kernelRadius + 1; k++)
                {
                    Color sourceColor = sourceImage.GetPixel(Clamp(x + l, 0, sourceImage.Width - 1), Clamp(y + k, 0, sourceImage.Height - 1));
                    ColorsR[i] = sourceColor.R;
                    ColorsG[i] = sourceColor.G;
                    ColorsB[i] = sourceColor.B;
                    i++;
                }
            Array.Sort(ColorsR);
            Array.Sort(ColorsG);
            Array.Sort(ColorsB);

            return Color.FromArgb(
                ColorsR[kernelSize / 2],                           
                ColorsG[kernelSize / 2],                                 
                ColorsB[kernelSize / 2]);
        }

           
    }
    
    class Dilation : MatrixFilter
    {
        float max;
        int maxX, maxY;

        public Dilation(string strElem)
        {
            if(strElem == "square")
                kernel = new float[3, 3] { { 1, 1, 1 },
                                           { 1, 1, 1 }, 
                                           { 1, 1, 1 } };
            if(strElem == "diamond")
                kernel = new float[3, 3] { { 0, 1, 0 },
                                           { 1, 1, 1 }, 
                                           { 0, 1, 0 } };
            if(strElem == "bigsquare")
                kernel = new float[5, 5] { { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1}};
            if (strElem == "bigdiamond")
                kernel = new float[5, 5] { { 0, 0, 1, 0, 0},
                                           { 0, 1, 1, 1, 0},
                                           { 1, 1, 1, 1, 1},
                                           { 0, 1, 1, 1, 0},
                                           { 0, 0, 1, 0, 0}};
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            maxX = x;
            maxY = y;
            max = sourceImage.GetPixel(x, y).GetBrightness();

            if (x - (kernel.GetLength(0) - 1) / 2 > 0 && y - (kernel.GetLength(0) - 1) / 2 > 0)
            {

                for (int i = 0; i <= (kernel.GetLength(0) - 1) / 2; i++)
                    for (int j = 0; j <= (kernel.GetLength(1) - 1) / 2; j++)
                    {
                        if (max < (sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness()))
                        {
                            if (kernel[i,j] == 1)
                            {
                                max = sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness();
                                maxX = x + i - (kernel.GetLength(0) - 1) / 2;
                                maxY = y + j - (kernel.GetLength(1) - 1) / 2;
                            }
                        }
                    }
            }

            return sourceImage.GetPixel(maxX,maxY);
        }
    }

    class Erosion : MatrixFilter
    {
        float min;
        int minX, minY;

        public Erosion(string strElem)
        {
            if (strElem == "square")
                kernel = new float[3, 3] { { 1, 1, 1 },
                                           { 1, 1, 1 },
                                           { 1, 1, 1 } };
            if (strElem == "diamond")
                kernel = new float[3, 3] { { 0, 1, 0 },
                                           { 1, 1, 1 },
                                           { 0, 1, 0 } };
            if (strElem == "bigsquare")
                kernel = new float[5, 5] { { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1}};
            if (strElem == "bigdiamond")
                kernel = new float[5, 5] { { 0, 0, 1, 0, 0},
                                           { 0, 1, 1, 1, 0},
                                           { 1, 1, 1, 1, 1},
                                           { 0, 1, 1, 1, 0},
                                           { 0, 0, 1, 0, 0}};
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            minX = x;
            minY = y;
            min = sourceImage.GetPixel(x, y).GetBrightness();

            if (x - (kernel.GetLength(0) - 1) / 2 > 0 && y - (kernel.GetLength(0) - 1) / 2 > 0)
            {

                for (int i = 0; i <= (kernel.GetLength(0) - 1) / 2; i++)
                    for (int j = 0; j <= (kernel.GetLength(1) - 1) / 2; j++)
                    {
                        if (min > (sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness()))
                        {
                            if (kernel[i, j] == 1)
                            {
                                min = sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness();
                                minX = x + i - (kernel.GetLength(0) - 1) / 2;
                                minY = y + j - (kernel.GetLength(1) - 1) / 2;
                            }
                        }
                    }
            }

            return sourceImage.GetPixel(minX, minY);
        }
    }

    class Opening : MatrixFilter
    {
        float max;
        int maxX, maxY;

        float min;
        int minX, minY;

        public Opening(string strElem)
        {
            if (strElem == "square")
                kernel = new float[3, 3] { { 1, 1, 1 },
                                           { 1, 1, 1 },
                                           { 1, 1, 1 } };
            if (strElem == "diamond")
                kernel = new float[3, 3] { { 0, 1, 0 },
                                           { 1, 1, 1 },
                                           { 0, 1, 0 } };
            if (strElem == "bigsquare")
                kernel = new float[5, 5] { { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1}};
            if (strElem == "bigdiamond")
                kernel = new float[5, 5] { { 0, 0, 1, 0, 0},
                                           { 0, 1, 1, 1, 0},
                                           { 1, 1, 1, 1, 1},
                                           { 0, 1, 1, 1, 0},
                                           { 0, 0, 1, 0, 0}};
        }


        protected Color calculateNewPixelColorDilation(Bitmap sourceImage, int x, int y)
        {
            maxX = x;
            maxY = y;
            max = sourceImage.GetPixel(x, y).GetBrightness();

            if (x - (kernel.GetLength(0) - 1) / 2 > 0 && y - (kernel.GetLength(0) - 1) / 2 > 0)
            {

                for (int i = 0; i <= (kernel.GetLength(0) - 1) / 2; i++)
                    for (int j = 0; j <= (kernel.GetLength(1) - 1) / 2; j++)
                    {
                        if (max < (sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness()))
                        {
                            if (kernel[i, j] == 1)
                            {
                                max = sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness();
                                maxX = x + i - (kernel.GetLength(0) - 1) / 2;
                                maxY = y + j - (kernel.GetLength(1) - 1) / 2;
                            }
                        }
                    }
            }

            return sourceImage.GetPixel(maxX, maxY);
        }

        protected Color calculateNewPixelColorErosion(Bitmap sourceImage, int x, int y)
        {
            minX = x;
            minY = y;
            min = sourceImage.GetPixel(x, y).GetBrightness();

            if (x - (kernel.GetLength(0) - 1) / 2 > 0 && y - (kernel.GetLength(0) - 1) / 2 > 0)
            {

                for (int i = 0; i <= (kernel.GetLength(0) - 1) / 2; i++)
                    for (int j = 0; j <= (kernel.GetLength(1) - 1) / 2; j++)
                    {
                        if (min > (sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness()))
                        {
                            if (kernel[i, j] == 1)
                            {
                                min = sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness();
                                minX = x + i - (kernel.GetLength(0) - 1) / 2;
                                minY = y + j - (kernel.GetLength(1) - 1) / 2;
                            }
                        }
                    }
            }

            return sourceImage.GetPixel(minX, minY);
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap tmp = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending) return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    tmp.SetPixel(i, j, calculateNewPixelColorErosion(sourceImage, i, j));
                }
            }

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending) return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColorDilation(tmp, i, j));
                }
            }

            return resultImage;
        }
    }

    class Closing : MatrixFilter
    {
        float max;
        int maxX, maxY;

        float min;
        int minX, minY;

        public Closing(string strElem)
        {
            if (strElem == "square")
                kernel = new float[3, 3] { { 1, 1, 1 },
                                           { 1, 1, 1 },
                                           { 1, 1, 1 } };
            if (strElem == "diamond")
                kernel = new float[3, 3] { { 0, 1, 0 },
                                           { 1, 1, 1 },
                                           { 0, 1, 0 } };
            if (strElem == "bigsquare")
                kernel = new float[5, 5] { { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1}};
            if (strElem == "bigdiamond")
                kernel = new float[5, 5] { { 0, 0, 1, 0, 0},
                                           { 0, 1, 1, 1, 0},
                                           { 1, 1, 1, 1, 1},
                                           { 0, 1, 1, 1, 0},
                                           { 0, 0, 1, 0, 0}};
        }

        protected Color calculateNewPixelColorDilation(Bitmap sourceImage, int x, int y)
        {
            maxX = x;
            maxY = y;
            max = sourceImage.GetPixel(x, y).GetBrightness();

            if (x - (kernel.GetLength(0) - 1) / 2 > 0 && y - (kernel.GetLength(0) - 1) / 2 > 0)
            {

                for (int i = 0; i <= (kernel.GetLength(0) - 1) / 2; i++)
                    for (int j = 0; j <= (kernel.GetLength(1) - 1) / 2; j++)
                    {
                        if (max < (sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness()))
                        {
                            if (kernel[i, j] == 1)
                            {
                                max = sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness();
                                maxX = x + i - (kernel.GetLength(0) - 1) / 2;
                                maxY = y + j - (kernel.GetLength(1) - 1) / 2;
                            }
                        }
                    }
            }

            return sourceImage.GetPixel(maxX, maxY);
        }

        protected Color calculateNewPixelColorErosion(Bitmap sourceImage, int x, int y)
        {
            minX = x;
            minY = y;
            min = sourceImage.GetPixel(x, y).GetBrightness();

            if (x - (kernel.GetLength(0) - 1) / 2 > 0 && y - (kernel.GetLength(0) - 1) / 2 > 0)
            {

                for (int i = 0; i <= (kernel.GetLength(0) - 1) / 2; i++)
                    for (int j = 0; j <= (kernel.GetLength(1) - 1) / 2; j++)
                    {
                        if (min > (sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness()))
                        {
                            if (kernel[i, j] == 1)
                            {
                                min = sourceImage.GetPixel((x + i - (kernel.GetLength(0) - 1) / 2), (y + j - (kernel.GetLength(1) - 1) / 2)).GetBrightness();
                                minX = x + i - (kernel.GetLength(0) - 1) / 2;
                                minY = y + j - (kernel.GetLength(1) - 1) / 2;
                            }
                        }
                    }
            }

            return sourceImage.GetPixel(minX, minY);
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap tmp = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending) return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    tmp.SetPixel(i, j, calculateNewPixelColorDilation(sourceImage, i, j));
                }
            }

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending) return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColorErosion(tmp, i, j));
                }
            }

            return resultImage;
        }
    }

    class Gradient : MatrixFilter
    {  
        string Elem = "square";

        public Gradient(string strElem)
        {
            Elem = strElem;
            if (strElem == "square") 
                kernel = new float[3, 3] { { 1, 1, 1 },
                                           { 1, 1, 1 },
                                           { 1, 1, 1 } };
            if (strElem == "diamond")
                kernel = new float[3, 3] { { 0, 1, 0 },
                                           { 1, 1, 1 },
                                           { 0, 1, 0 } };
            if (strElem == "bigsquare")
                kernel = new float[5, 5] { { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1},
                                           { 1, 1, 1, 1, 1}};
            if (strElem == "bigdiamond")
                kernel = new float[5, 5] { { 0, 0, 1, 0, 0},
                                           { 0, 1, 1, 1, 0},
                                           { 1, 1, 1, 1, 1},
                                           { 0, 1, 1, 1, 0},
                                           { 0, 0, 1, 0, 0}};
        }

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Filters dilate = new Dilation(Elem);
            Filters erode = new Erosion(Elem);

            Bitmap resD = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap resE = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap res = new Bitmap(sourceImage.Width, sourceImage.Height);

            resD = dilate.processImage(sourceImage, worker);
            resE = erode.processImage(sourceImage, worker);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / res.Width * 100));
                if (worker.CancellationPending) return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    res.SetPixel(i, j, Color.FromArgb(Clamp(resD.GetPixel(i, j).R - resE.GetPixel(i, j).R, 0, 255),
                                                                  Clamp(resD.GetPixel(i, j).G - resE.GetPixel(i, j).G, 0, 255),
                                                                  Clamp(resD.GetPixel(i, j).B - resE.GetPixel(i, j).B, 0, 255)));
                }
            }

            return res;
        }
    }
}
    
