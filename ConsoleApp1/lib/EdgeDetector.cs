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
            binImage.CopyTo(resultImage);
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

            byte[,] binImageValues = new byte[binImage.Rows, binImage.Cols];

            for (int i = 0; i < binImage.Rows; i++)
            {
                for (int j = 0; j < binImage.Cols; j++)
                {
                    binImageValues[i, j] = binImage.At<byte>(i, j);
                }
            }

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

                    horizontalGradientPrev = binImageValues[indexLftICoordPrev, j] - 2 * binImageValues[indexLftICoord, j];
                    verticalGradientPrev = binImageValues[i, indexLftJCoordPrev] - 2 * binImageValues[i, indexLftJCoord];
                    diagonalUpRightGradientPrev = binImageValues[indexLftICoordPrev, indexLftJCoordPrev] - 2 * binImageValues[indexLftICoord, indexLftJCoord];
                    diagonalDownRightGradientPrev = binImageValues[indexLftICoordPrev, indexRghtJCoordAfter] - 2 * binImageValues[indexLftICoord, indexRghtJCoord];

                    horizontalGradient = binImageValues[indexLftICoord, j] + binImageValues[indexRghtICoord, j];
                    verticalGradient = binImageValues[i, indexLftJCoord] + binImageValues[i, indexRghtJCoord];
                    diagonalUpRightGradient = binImageValues[indexLftICoord, indexLftJCoord] + binImageValues[indexRghtICoord, indexRghtJCoord];
                    diagonalDownRightGradient = binImageValues[indexLftICoord, indexRghtJCoord] + binImageValues[indexRghtICoord, indexLftJCoord];

                    horizontalGradientAfter = binImageValues[indexRghtICoordAfter, j] - 2 * binImageValues[indexRghtICoord, j];
                    verticalGradientAfter = binImageValues[i, indexRghtJCoordAfter] - 2 * binImageValues[i, indexRghtJCoord];
                    diagonalUpRightGradientAfter = binImageValues[indexRghtICoordAfter, indexRghtJCoordAfter] - 2 * binImageValues[indexRghtICoord, indexRghtJCoord];
                    diagonalDownRightGradientAfter = binImageValues[indexRghtICoordAfter, indexLftJCoordPrev] - 2 * binImageValues[indexRghtICoord, indexLftJCoord];

                    float maxValuePrev = Math.Max(Math.Max(horizontalGradientPrev, verticalGradientPrev), Math.Max(diagonalUpRightGradientPrev, diagonalDownRightGradientPrev)) + binImageValues[i, j];
                    float maxValueCurrent = Math.Max(Math.Max(horizontalGradient, verticalGradient), Math.Max(diagonalUpRightGradient, diagonalDownRightGradient)) - 2 * binImageValues[i, j];
                    float maxValueAfter = Math.Max(Math.Max(horizontalGradientAfter, verticalGradientAfter), Math.Max(diagonalUpRightGradientAfter, diagonalDownRightGradientAfter)) + binImageValues[i, j];
                    float maxValueWeightSum = (WeightPreviousPoint * maxValuePrev + WeightCurrentPoint * maxValueCurrent + WeightAfterPoint * maxValueAfter) / (WeightPreviousPoint + WeightCurrentPoint + WeightAfterPoint);
                    resultImgIndexer[i, j] = (maxValueWeightSum > Threshold) ? (byte)255 : (byte)0;
                }
            }
            return resultImage;
        }
    }
}