using ConsoleApp1.Utils;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp1.Objects;

namespace ConsoleApp1
{
    class Program
    {
        public static Mat FiltradoRuido(Mat imagen)
        {
            /* filtro de ruido mediante filtros de mediana y media */
            Mat imagenFiltroMediana = new Mat();
            Mat imagenSinRuidoGaussiano = new Mat();
            Cv2.MedianBlur(imagen, imagenFiltroMediana, 3);
            Cv2.FastNlMeansDenoisingColored(imagenFiltroMediana, imagenSinRuidoGaussiano, 5, 5, 5, 10);
            /* liberar memoria */
            imagenFiltroMediana.Release();

            return imagenSinRuidoGaussiano;
        }

        public static Mat EscalaGrisesEqualizada(Mat imagen)
        {
            /* bgr a grayscale */
            Mat imagenGris = new Mat();
            Cv2.CvtColor(imagen, imagenGris, ColorConversionCodes.BGR2GRAY);

            /* ecualización por histograma */
            Mat imagenGrisEqualizada = new Mat();
            CLAHE ecualizadorHistograma = Cv2.CreateCLAHE(20, new Size(13, 13));
            ecualizadorHistograma.Apply(imagenGris, imagenGrisEqualizada);

            /* liberar memoria */
            imagenGris.Release();

            imagenGrisEqualizada.Normalize(0, 1, NormTypes.MinMax);
            return imagenGrisEqualizada;
        }

        public static Mat edgeDetection(Mat imagenBinaria)
        {

            return null;
        }

        public static Mat FrecuenciasAltasPotenciadasContraste(Mat imagenGris, int tipoMat = MatType.CV_32F)
        {
            double alpha = 2;
            Mat imagenGris32F = new Mat();
            imagenGris.ConvertTo(imagenGris32F, tipoMat);

            Mat kernelFiltroPasabajo = new Mat(4, 4, tipoMat, 0.08);
            Mat imagenFrecuenciasBajas = new Mat();
            Cv2.Filter2D(imagenGris, imagenFrecuenciasBajas, tipoMat, kernelFiltroPasabajo);
            Mat kernelMorfologico = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3));
            //Cv2.Erode(imagenFrecuenciasBajas, imagenFrecuenciasBajas, kernelMorfologico);

            imagenFrecuenciasBajas = imagenGris32F + 0.4 * imagenFrecuenciasBajas;
            Cv2.ImWrite("D:\\Dictuc\\output1.png", imagenFrecuenciasBajas);
            ///* filtro pasa alto */
            //Mat imagenFrecuenciasAltas = imagenGris32F - imagenFrecuenciasBajas;

            ///* potenciar frecuencias altas */0
            //Mat imagenFrecuenciasAltaPotenciada = imagenFrecuenciasAltas + (alpha - 1) * imagenGris32F;

            /* Procesamiento morfológico y ecualización por histograma */
            //Mat imagenErode = new Mat();
            //Mat kernelMorfologico = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(4, 4));
            //Cv2.Erode(imagenFrecuenciasAltaPotenciada, imagenErode, kernelMorfologico);
            //imagenErode.ConvertTo(imagenErode, MatType.CV_8UC3);
            //CLAHE ecualizadorHistograma = Cv2.CreateCLAHE(5, new Size(10, 10));
            //ecualizadorHistograma.Apply(imagenErode, imagenFrecuenciasAltaPotenciada);
            imagenFrecuenciasBajas.ConvertTo(imagenFrecuenciasBajas, MatType.CV_8U);
            //Cv2.Canny(imagenFrecuenciasBajas, imagenFrecuenciasBajas, 10, 20, apertureSize: 7);
            Cv2.ImWrite("D:\\Dictuc\\output2.png", imagenFrecuenciasBajas);
            Cv2.ImWrite("D:\\Dictuc\\output3.png", imagenGris);

            /* liberar memoria */
            //imagenGris32F.Release();
            //kernelFiltroPasabajo.Release();
            //imagenFrecuenciasBajas.Release();
            //imagenFrecuenciasAltas.Release();
            //imagenErode.Release();
            //kernelMorfologico.Release();

            //return imagenFrecuenciasAltaPotenciada;
            imagenFrecuenciasBajas.ConvertTo(imagenFrecuenciasBajas, MatType.CV_8U);
            return imagenFrecuenciasBajas;
        }

        public static void CalculoMatrizBinariaYRelleno(Mat imagen, out Mat imagenBinaria, out Mat imagenAberturaRelleno)
        {
            imagenBinaria = new Mat();
            imagenAberturaRelleno = new Mat();
            //Cv2.AdaptiveThreshold(imagen, imagenBinaria, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 3);
            imagenBinaria = 255 - imagen;
            Mat imagenBinariaProc = new Mat();
            imagenBinaria.CopyTo(imagenBinariaProc);
            Mat kernelMorfologicoElipse = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
            Mat kernelMorfologicoCruz = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3));
            Cv2.Erode(imagenBinariaProc, imagenBinariaProc, kernelMorfologicoElipse, iterations: 3);
            Cv2.Dilate(imagenBinariaProc, imagenBinariaProc, kernelMorfologicoCruz, iterations: 2);
            imagenBinariaProc = 255 - imagenBinariaProc;
            Mat imagenTransfDistancia = new Mat();
            Mat imagenTransfDistanciaBinaria = new Mat();
            Cv2.DistanceTransform(imagenBinariaProc, imagenTransfDistancia, DistanceTypes.C, DistanceMaskSize.Mask5);
            //Cv2.Threshold(imagenTransfDistancia, imagenTransfDistanciaBinaria, (byte)1, (byte)255, ThresholdTypes.Binary);
            imagenTransfDistancia.ConvertTo(imagenTransfDistancia, MatType.CV_8U);
            //Cv2.AdaptiveThreshold(imagenTransfDistancia*50, imagenTransfDistancia, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 11, 3);
            //imagenTransfDistanciaBinaria.ConvertTo(imagenTransfDistanciaBinaria, MatType.CV_8U);
            imagenTransfDistancia.CopyTo(imagenTransfDistanciaBinaria);
            double maxDist, minDist, rangoDist;
            imagenTransfDistanciaBinaria.MinMaxIdx(out minDist, out maxDist);
            rangoDist = maxDist - minDist;
            var indexadorImgDistancia = imagenTransfDistanciaBinaria.GetGenericIndexer<byte>();
            for (int i = 0; i < imagenTransfDistancia.Rows; i++)
            {
                for (int j = 0; j < imagenTransfDistancia.Cols; j++)
                {
                    if (indexadorImgDistancia[i, j] >= maxDist - rangoDist * 0.35
                        || indexadorImgDistancia[i, j] <= minDist + rangoDist * 0.15)
                        indexadorImgDistancia[i, j] = (byte)255;
                    else
                        indexadorImgDistancia[i, j] = (byte)0;
                }
            }
            Mat marcadores = new Mat();
            int numMarcadores = Cv2.ConnectedComponents(imagenTransfDistanciaBinaria, marcadores, PixelConnectivity.Connectivity8, MatType.CV_16UC1);
            Dictionary<int, int> cantidadMarcador = new Dictionary<int, int>();

            for (int i = 0; i < marcadores.Rows; i++)
                for (int j = 0; j < marcadores.Cols; j++)
                {
                    int m = marcadores.At<int>(i, j);
                    try
                    {
                        cantidadMarcador[m] += 1;
                    }
                    catch
                    {
                        cantidadMarcador[m] = 1;
                    }
                }

            int indiceMarcadorMax = cantidadMarcador.Select(v => new Tuple<int, int>(v.Key, v.Value))
                .OrderByDescending(x => x.Item1).First().Item2;
            Mat fondoMarcadores = new Mat();
            marcadores.CopyTo(fondoMarcadores);
            var indexador = fondoMarcadores.GetGenericIndexer<int>();
            for (int i = 0; i < fondoMarcadores.Rows; i++)
                for (int j = 0; j < fondoMarcadores.Cols; j++)
                    indexador[i, j] = (indexador[i, j] == indiceMarcadorMax) ? 255 : 0;
            fondoMarcadores.ConvertTo(fondoMarcadores, imagenBinaria.Type());
            Mat imagenBinariaSinFondo = new Mat();
            Mat imagenRellenoHuecos = new Mat();
            Cv2.Subtract(imagenBinaria, fondoMarcadores, imagenBinariaSinFondo);
            Cv2.Add(imagenBinaria, imagenBinariaSinFondo, imagenRellenoHuecos);

            Mat kernelMorfologicoApertura = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
            Cv2.MorphologyEx(imagenRellenoHuecos, imagenAberturaRelleno, MorphTypes.Open, kernelMorfologicoApertura, iterations: 2);

            /* liberar memoria */
            imagenBinariaProc.Release();
            imagenBinariaSinFondo.Release();
            imagenRellenoHuecos.Release();
            kernelMorfologicoElipse.Release();
            kernelMorfologicoCruz.Release();
            kernelMorfologicoApertura.Release();
        }

        static public void printMat(Mat m)
        {
            for (int i = 0; i < m.Rows; i++)
            {
                string fila = "";
                for (int j = 0; j < m.Cols; j++)
                {
                    fila += m.At<byte>(i, j) + ",";
                }
                if (m.Cols > 0)
                    fila = fila.Remove(fila.Length - 1);
                Console.WriteLine("[" + fila + "]");
            }
        }

        static void Main(string[] args)
        {
            Mat imagen = new Mat();
            imagen = Cv2.ImRead("D:\\Dictuc\\20180807-00004500.png");
            int alto = imagen.Height;
            int ancho = imagen.Width;
            Mat[] bgr = imagen.Split();
            Cv2.ImWrite("D:\\Dictuc\\output1.png", bgr[0]);
            Cv2.ImWrite("D:\\Dictuc\\output2.png", bgr[1]);
            Cv2.ImWrite("D:\\Dictuc\\output3.png", bgr[2]);

            Mat imagenRuidoFiltrado = FiltradoRuido(imagen);
            Mat imagenGrisContraste = EscalaGrisesEqualizada(imagenRuidoFiltrado);
            Mat imagenGrisFrecAltasProc = FrecuenciasAltasPotenciadasContraste(imagenGrisContraste);
            EdgeDetector edgeDetector = new EdgeDetector()
            {
                Threshold = (byte)90,
                SparseDistance = 3,
                WeightPreviousPoint = (float)0.1,
                WeightCurrentPoint = (float)1.0,
                WeightAfterPoint = (float)0.1,
            };
            Mat imagenBordes = edgeDetector.EdgeImage(imagenGrisContraste);
            Cv2.ImWrite("D:\\Dictuc\\output3.png", imagenGrisContraste);
            Cv2.ImWrite("D:\\Dictuc\\output4.png", imagenBordes);
            Mat imagenBinaria, imagenAberturaRelleno;
            CalculoMatrizBinariaYRelleno(imagenBordes, out imagenBinaria, out imagenAberturaRelleno);

            /* Nueva idea */

            Mat mascaraInv = 255 - imagenAberturaRelleno;


            ///* REVISAR NOMBRES DE VARIABLES */
            Mat kernelMorfologico = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(2, 2));
            Mat sureBg = new Mat();
            Mat sureFg = new Mat();
            Cv2.Dilate(imagenAberturaRelleno, sureBg, kernelMorfologico, iterations: 2);

            Mat DistSureFg = new Mat();
            Mat AreasSureFg = new Mat();
            Mat SureFg = new Mat();
            Cv2.DistanceTransform(imagenAberturaRelleno, DistSureFg, DistanceTypes.L1, DistanceMaskSize.Mask5);
            int numAreas = Cv2.ConnectedComponents(imagenAberturaRelleno, AreasSureFg, PixelConnectivity.Connectivity8);

            Segment[] segments = new Segment[numAreas];

            Dictionary<int, float> maxArea = new Dictionary<int, float>();
            Dictionary<int, float> minArea = new Dictionary<int, float>();
            for (int i = 0; i < AreasSureFg.Rows; i++)
            {
                for (int j = 0; j < AreasSureFg.Cols; j++)
                {
                    int m = AreasSureFg.At<Int32>(i, j);
                    byte pixelSurrounding = 0;
                    float distance = (float)0;

                    if (i >= 1)
                    {
                        distance = DistSureFg.At<float>(i - 1, j);
                        if(distance == 2)
                        {
                            pixelSurrounding |= Segment.PIXEL_SURROUNDED_LEFT;
                        }
                    }
                    if (i < AreasSureFg.Rows - 1)
                    {
                        distance = DistSureFg.At<float>(i + 1, j);
                        if (distance == 2)
                        {
                            pixelSurrounding |= Segment.PIXEL_SURROUNDED_RIGHT;
                        }
                    }
                    if (j >= 1)
                    {
                        distance = DistSureFg.At<float>(i, j - 1);
                        if (distance == 2)
                        {
                            pixelSurrounding |= Segment.PIXEL_SURROUNDED_DOWN;
                        }
                    }
                    if (j < AreasSureFg.Cols - 1)
                    {
                        distance = DistSureFg.At<float>(i, j + 1);
                        if (distance == 2)
                        {
                            pixelSurrounding |= Segment.PIXEL_SURROUNDED_UP;
                        }
                    }

                    SegmentPixelData newPixel = new SegmentPixelData()
                    {
                        Distance = DistSureFg.At<float>(i, j),
                        CoordsXY = new int[] { i, j },
                        Concave = 0,
                        Indexes = new int[] { i, -1 },
                        PixelsSurrounding = pixelSurrounding,
                    };

                    try
                    {
                        segments[m].PixelData.Add(newPixel);
                    }
                    catch
                    {
                        segments[m] = new Segment()
                        {
                            SegmentId = m,
                            PixelData = new List<SegmentPixelData>(),
                        };
                        segments[m].PixelData.Add(newPixel);
                    }
                }
            }

            List<int> indexConcavos = segments.Where(s => s.Circularity > 1).Select(s => s.SegmentId).ToList();

            Mat figurasConcavas = new Mat();
            imagenBinaria.CopyTo(figurasConcavas);
            figurasConcavas = figurasConcavas * 0;
            var indexadorFiguras = figurasConcavas.GetGenericIndexer<byte>();
            foreach(var s in segments.Where(s => s.Circularity < 1.1 && s.Circularity > 0.9))
            {
                foreach(var p in s.PixelData.Where(p => p.Distance == 1))
                {
                    if(imagenAberturaRelleno.At<byte>(p.CoordsXY[0], p.CoordsXY[1]) != (byte)0)
                    {
                        indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 255;
                    }
                }
            }

            foreach (var s in segments.Where(s => s.Circularity >= 1.1))
            {
                foreach (var p in s.PixelData.Where(p => p.Distance == 1))
                {
                    if (imagenAberturaRelleno.At<byte>(p.CoordsXY[0], p.CoordsXY[1]) != (byte)0)
                    {
                        indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 175;
                    }
                }
            }

            foreach (var s in segments.Where(s => s.Circularity <= 0.9))
            {
                foreach (var p in s.PixelData.Where(p => p.Distance == 1))
                {
                    if (imagenAberturaRelleno.At<byte>(p.CoordsXY[0], p.CoordsXY[1]) != (byte)0)
                    {
                        indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 75;
                    }
                }
            }

            foreach (var s in segments)
            {
                s.SetPixelConcavity();
                s.Segmentation();
                foreach (var p in s.PixelData.Where(p => p.Distance == 1))
                {
                    if (p.Concave == 1)
                    {
                        indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 200;
                    }
                    if (p.Concave == -1)
                    {
                        indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 100;
                    }
                }
            }

            foreach (var s in segments)
            {
                s.SetPixelConcavity();
                s.Segmentation();
                foreach (var p in s.PixelData.Where(p => p.Distance == 2))
                {
                    indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 230;
                }
            }

            Cv2.ImWrite("D:\\Dictuc\\output4.png", figurasConcavas);


            imagenAberturaRelleno.CopyTo(SureFg);

            Mat colormap = new Mat();
            figurasConcavas.CopyTo(colormap);
            colormap.ConvertTo(colormap, MatType.CV_8UC1);
            Cv2.ApplyColorMap(colormap, colormap, ColormapTypes.Jet);
            Cv2.ImWrite("D:\\Dictuc\\sureFG.png", colormap);

            Mat kernel2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
            Cv2.MorphologyEx(imagenAberturaRelleno, sureFg, MorphTypes.Open, kernel2, iterations: 2);

            Mat unknown = new Mat();
            //Cv2.Subtract(sureBg, sureFg, unknown);
            Cv2.Subtract(sureBg, SureFg, unknown);
            unknown = 255 - SureFg;
            Mat markers = new Mat();
            Cv2.ConnectedComponents(SureFg, markers);
            markers = markers + 1;
            var indexador2 = markers.GetGenericIndexer<Int32>();
            for (int i = 0; i < unknown.Rows; i++)
                for (int j = 0; j < unknown.Cols; j++)
                    if (unknown.At<byte>(i, j) == 255)
                        indexador2[i, j] = 0;
            markers.CopyTo(colormap);
            colormap.ConvertTo(colormap, MatType.CV_8UC3);
            Cv2.ApplyColorMap(colormap, colormap, ColormapTypes.Rainbow);

            Mat img1 = new Mat();
            imagen.CopyTo(img1);
            //Cv2.ImWrite("D:\\Dictuc\\output3.png", img1);
            DistSureFg = 0 - DistSureFg*0;
            Mat DistColor = new Mat();
            imagenGrisContraste = bgr[2]*1.2;
            imagenGrisContraste = 255 - imagenGrisContraste;
            Cv2.CvtColor(mascaraInv, DistColor, ColorConversionCodes.GRAY2BGR);
            DistColor.ConvertTo(DistColor, MatType.CV_8U);
            markers.CopyTo(colormap);
            colormap.ConvertTo(colormap, MatType.CV_8U);
            Cv2.ApplyColorMap(colormap, colormap, ColormapTypes.Jet);
            Cv2.ImWrite("D:\\Dictuc\\colormap1.png", colormap);
            Cv2.Watershed(DistColor, markers);
            markers.CopyTo(colormap);
            colormap.ConvertTo(colormap, MatType.CV_8U);
            Cv2.ApplyColorMap(colormap, colormap, ColormapTypes.Jet);
            Cv2.ImWrite("D:\\Dictuc\\colormap2.png", colormap);
            Cv2.ImWrite("D:\\Dictuc\\watersheedIn.png", DistColor);
            var indexador4 = img1.GetGenericIndexer<Vec3i>();
            for (int i = 0; i < markers.Rows; i++)
            {
                for (int j = 0; j < markers.Cols; j++)
                {
                    if (markers.At<int>(i, j) == -1)
                        indexador4[i, j] = new Vec3i(0, 255, 0);
                }
            }

            Mat seg = new Mat();
            markers.CopyTo(seg);
            var indexador5 = seg.GetGenericIndexer<int>();
            for (int i = 0; i < markers.Rows; i++)
            {
                for (int j = 0; j < markers.Cols; j++)
                {
                    indexador5[i, j] = (Math.Abs(indexador5[i, j]) > 1) ? 255 : 0;
                }
            }
            Mat kE1 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 1));
            Cv2.Erode(seg, seg, kE1, iterations: 3);
            int thrs1 = 1500;
            int thrs2 = 1800;
            Mat edge1 = new Mat();
            seg.ConvertTo(seg, MatType.CV_8U);
            Cv2.Canny(seg, edge1, thrs1, thrs2, apertureSize: 5);

            SimpleBlobDetector.Params params1 = new SimpleBlobDetector.Params() {
                MinThreshold = 0,
                MaxThreshold = 255,
                FilterByArea = true,
                MinArea = 15,
                FilterByCircularity = false,
                MinCircularity = (float)0.01,
                FilterByConvexity = false,
                MinConvexity = (float)0.1,
                FilterByInertia =  false,
                MinInertiaRatio = (float)0.01,
            };
            SimpleBlobDetector detectorBlobs = SimpleBlobDetector.Create(params1);
            KeyPoint[] segmentos = detectorBlobs.Detect(edge1);
            Mat vis11 = new Mat();
            imagen.CopyTo(vis11);
            var indexador6 = vis11.GetGenericIndexer<Vec3i>();
            for (int i = 0; i < vis11.Rows; i++)
            {
                for (int j = 0; j < vis11.Cols; j++)
                {
                    if(edge1.At<int>(i,j) != 0) {
                        indexador6[i, j] = new Vec3i(0, 255, 0);
                    }
                }
            }
            Mat imWithKeypoints1 = new Mat();
            Cv2.DrawKeypoints(vis11, segmentos, imWithKeypoints1, new Scalar(0, 0, 255), DrawMatchesFlags.DrawRichKeypoints);

            Cv2.ImWrite("D:\\Dictuc\\output1.png", edge1);
            Cv2.ImWrite("D:\\Dictuc\\output2.png", imWithKeypoints1);
            Console.WriteLine("Hola");
        }
    }
}