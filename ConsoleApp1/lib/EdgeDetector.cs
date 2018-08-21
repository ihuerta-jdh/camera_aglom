using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ConsoleApp1.Utils
{
    class EdgeDetector
    {
        public int SparseDistance { get; set; }
        public byte Threshold { get; set; }
        public float WeightCurrentPoint { get; set; }
        public float WeightPreviousPoint { get; set; }
        public float WeightAfterPoint { get; set; }

        public Mat EdgeImage(Mat binImage)
        {
            Mat resultImage = new Mat();
            try
            {
                binImage.CopyTo(resultImage);
                var binImgIndexer = binImage.GetGenericIndexer<byte>();
                var resultImgIndexer = resultImage.GetGenericIndexer<byte>();

                float horizontalGradientPrev = 0;
                float verticalGradientPrev = 0;
                float diagonalUpRightGradientPrev = 0;
                float diagonalDownRightGradientPrev = 0;
                float horizontalGradient = 0;
                float verticalGradient = 0;
                float diagonalUpRightGradient = 0;
                float diagonalDownRightGradient = 0;
                float diagonalUpRightGradientAfter = 0;
                float diagonalDownRightGradientAfter = 0;
                float horizontalGradientAfter = 0;
                float verticalGradientAfter = 0;

                int sd = SparseDistance;

                for (int i = 0; i < binImage.Rows; i++)
                {
                    for (int j = 0; j < binImage.Cols; j++)
                    {
                        int indexLftICoordPrev = (i > 2 * sd) ? (i - 2 * sd) : 0;
                        int indexLftJCoordPrev = (j > 2 * sd) ? (j - 2 * sd) : 0;
                        int indexLftICoord = (i > sd) ? (i - sd) : 0;
                        int indexRghtICoord = (i + sd < binImage.Rows) ? (i + sd) : binImage.Rows - 1;
                        int indexLftJCoord = (j > sd) ? (j - sd) : 0;
                        int indexRghtJCoord = (j + sd < binImage.Cols) ? (j + sd) : binImage.Cols - 1;
                        int indexRghtICoordAfter = (i + 2 * sd < binImage.Rows) ? (i + 2 * sd) : binImage.Rows - 1;
                        int indexRghtJCoordAfter = (j + 2 * sd < binImage.Cols) ? (j + 2 * sd) : binImage.Cols - 1;

                        horizontalGradientPrev = binImgIndexer[indexLftICoordPrev, j] + binImgIndexer[i, j] - 2 * binImgIndexer[indexLftICoord, j];
                        verticalGradientPrev = binImgIndexer[i, indexLftJCoordPrev] + binImgIndexer[i, j] - 2 * binImgIndexer[i, indexLftJCoord];
                        diagonalUpRightGradientPrev = binImgIndexer[indexLftICoordPrev, indexLftJCoordPrev] + binImgIndexer[i, j] - 2 * binImgIndexer[indexLftICoord, indexLftJCoord];
                        diagonalDownRightGradientPrev = binImgIndexer[indexLftICoordPrev, indexRghtJCoordAfter] + binImgIndexer[i, j] - 2 * binImgIndexer[indexLftICoord, indexRghtJCoord];

                        horizontalGradient = binImgIndexer[indexLftICoord, j] + binImgIndexer[indexRghtICoord, j] - 2 * binImgIndexer[i, j];
                        verticalGradient = binImgIndexer[i, indexLftJCoord] + binImgIndexer[i, indexRghtJCoord] - 2 * binImgIndexer[i, j];
                        diagonalUpRightGradient = binImgIndexer[indexLftICoord, indexLftJCoord] + binImgIndexer[indexRghtICoord, indexRghtJCoord] - 2 * binImgIndexer[i, j];
                        diagonalDownRightGradient = binImgIndexer[indexLftICoord, indexRghtJCoord] + binImgIndexer[indexRghtICoord, indexLftJCoord] - 2 * binImgIndexer[i, j];

                        horizontalGradientAfter = binImgIndexer[indexRghtICoordAfter, j] + binImgIndexer[i, j] - 2 * binImgIndexer[indexRghtICoord, j];
                        verticalGradientAfter = binImgIndexer[i, indexRghtJCoordAfter] + binImgIndexer[i, j] - 2 * binImgIndexer[i, indexRghtJCoord];
                        diagonalUpRightGradientAfter = binImgIndexer[indexRghtICoordAfter, indexRghtJCoordAfter] + binImgIndexer[i, j] - 2 * binImgIndexer[indexRghtICoord, indexRghtJCoord];
                        diagonalDownRightGradientAfter = binImgIndexer[indexRghtICoordAfter, indexLftJCoordPrev] + binImgIndexer[i, j] - 2 * binImgIndexer[indexRghtICoord, indexLftJCoord];

                        float maxValuePrev = Math.Max(Math.Max(horizontalGradientPrev, verticalGradientPrev), Math.Max(diagonalUpRightGradientPrev, diagonalDownRightGradientPrev));
                        float maxValueCurrent = Math.Max(Math.Max(horizontalGradient, verticalGradient), Math.Max(diagonalUpRightGradient, diagonalDownRightGradient));
                        float maxValueAfter = Math.Max(Math.Max(horizontalGradientAfter, verticalGradientAfter), Math.Max(diagonalUpRightGradientAfter, diagonalDownRightGradientAfter));
                        float maxValueWeightSum = (WeightPreviousPoint * maxValuePrev + WeightCurrentPoint * maxValueCurrent + WeightAfterPoint * maxValueAfter) / (WeightPreviousPoint + WeightCurrentPoint + WeightAfterPoint);
                        resultImgIndexer[i, j] = (maxValueWeightSum > Threshold) ? (byte)255 : (byte)0;
                    }
                }
            }
            catch
            {

            }
            return resultImage;
        }
    }
}