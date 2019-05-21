using ConsoleApp1.Utils;
using OpenCvSharp;
using OpenCvSharp.XImgProc;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using ConsoleApp1.Objects;

namespace ConsoleApp1
{
    class Program
    {
        public static Mat imagen = new Mat();
        private static Mutex mutex = new Mutex();

        public static Mat FiltradoRuido(Mat imagen)
        {
            /* filtro de ruido mediante filtros de mediana y media */
            Mat imagenFiltroMediana = new Mat();
            Mat imagenSinRuidoGaussiano = new Mat();
            Cv2.MedianBlur(imagen, imagenFiltroMediana, 3);
            Cv2.FastNlMeansDenoisingColored(imagenFiltroMediana, imagenSinRuidoGaussiano, 5, 5, 5, 10);
            ///* liberar memoria */
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
            CLAHE ecualizadorHistograma = Cv2.CreateCLAHE(5, new Size(3, 3));
            ecualizadorHistograma.Apply(imagenGris, imagenGrisEqualizada);

            /* liberar memoria */
            imagenGris.Release();
            return imagenGrisEqualizada;
        }

        public static Mat FrecuenciasAltasPotenciadasContraste(Mat imagenGris, int tipoMat = MatType.CV_32F)
        {
            double alpha = 2;
            Mat imagenGris32F = new Mat();
            imagenGris.ConvertTo(imagenGris32F, tipoMat);
        
            Mat kernelFiltroPasabajo = new Mat(5, 5, tipoMat, 0.2);
            Mat imagenFrecuenciasBajas = new Mat();
            Cv2.Filter2D(imagenGris, imagenFrecuenciasBajas, tipoMat, kernelFiltroPasabajo);
            Mat kernelMorfologico = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(5, 5));

            imagenFrecuenciasBajas = imagenGris32F + 0.8 * imagenFrecuenciasBajas;
            imagenFrecuenciasBajas.ConvertTo(imagenFrecuenciasBajas, MatType.CV_8U);

            /* liberar memoria */
            imagenGris32F.Release();
            kernelFiltroPasabajo.Release();
            //imagenFrecuenciasBajas.Release();
            kernelMorfologico.Release();

            imagenFrecuenciasBajas.ConvertTo(imagenFrecuenciasBajas, MatType.CV_8U);
            return imagenFrecuenciasBajas;
        }

        public static void CalculoMatrizBinariaYRelleno(Mat imagen, out Mat imagenBinaria, out Mat imagenAberturaRelleno)
        {
            imagenBinaria = new Mat();
            imagenAberturaRelleno = new Mat();
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
            imagenTransfDistancia.ConvertTo(imagenTransfDistancia, MatType.CV_8U);
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
                    int areaMarcador = 0;
                    int m = marcadores.At<int>(i, j);
                    cantidadMarcador.TryGetValue(m, out areaMarcador);
                    if (areaMarcador > 0)
                    {
                        cantidadMarcador[m] += 1;
                    }
                    else
                    {
                        cantidadMarcador[m] = 1;
                    }
                    //try
                    //{
                    //    cantidadMarcador[m] += 1;
                    //}
                    //catch
                    //{
                    //    cantidadMarcador[m] = 1;
                    //}
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

        public static void CapturarVideo()
        {
            //VideoCapture captura = new VideoCapture("D:\\Dictuc\\out.avi");
            ////VideoWriter salida = new VideoWriter("D:\\Dictuc\\out.avi", FourCC.XVID, 25.0, new Size(captura.FrameWidth, captura.FrameHeight), true);
            //while (captura.IsOpened())
            //{
            //    mutex.WaitOne();
            //    if (captura.Read(imagen))
            //    {
            //        //salida.Write(imagen);
            //        //imagen = Cv2.ImRead("D:\\Dictuc\\20180807-00004500.png");
            //        captura.Read(imagen);
            //    }
            //    mutex.ReleaseMutex();
            //    System.Threading.Thread.Sleep(50);
            //}
            //captura.Release();
            ////salida.Release();
        }

        //public static void CapturarVideo()
        //{
        //    IPAddress direccionCamara = new IPAddress(new byte[] { 192, 168, 0, 72 });
        //    Network.ResultadoPing pingCamara = Network.LocalPing(direccionCamara);

        //    if (pingCamara.conectado)
        //    {
        //        ProcesoImagen.StartCamera(direccionCamara.ToString());
        //        VideoWriter salida = new VideoWriter();
        //        bool antesPrimerFrame = true;
        //        while (true)
        //        {
        //            mutex.WaitOne();
        //            try
        //            {
        //                ProcesoImagen.GetLastFrame();
        //                imagen = Cv2.ImRead("D:\\Dictuc\\input.png");
        //            }
        //            catch
        //            {

        //            }
        //            mutex.ReleaseMutex();
        //            if(imagen.Height > 0)
        //            {
        //                if (antesPrimerFrame)
        //                {
        //                    salida = new VideoWriter("D:\\Dictuc\\out.avi", FourCC.XVID, 25.0, new Size(imagen.Width, imagen.Height), true);
        //                    antesPrimerFrame = false;
        //                }
        //                else
        //                {
        //                    salida.Write(imagen);
        //                }
        //            }                
        //            System.Threading.Thread.Sleep(50);
        //        }
        //        ProcesoImagen.StopCamera();
        //    }
        //}

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
            //Thread capturaVideoThread = new Thread(new ThreadStart(Program.CapturarVideo));
            //capturaVideoThread.Start();

            VideoCapture captura = new VideoCapture("D:\\Dictuc\\out1.avi");
            VideoWriter salida = new VideoWriter("D:\\Dictuc\\outSegmentado.avi", FourCC.XVID, 10.0, new Size(captura.FrameWidth, captura.FrameHeight), true);

            Mat imagenProcesada = new Mat();
            int numImg = 0;

            while (true)
            {
                //captura.Read(imagen);
                imagen = Cv2.ImRead("D:\\uvas2.jpg");
                mutex.WaitOne();
                imagen.CopyTo(imagenProcesada);
                mutex.ReleaseMutex();
                Mat imagenRuidoFiltrado = FiltradoRuido(imagenProcesada);
                Mat imagenGrisContraste = EscalaGrisesEqualizada(imagenRuidoFiltrado);
                Mat imagenGrisFrecAltasProc = FrecuenciasAltasPotenciadasContraste(imagenGrisContraste);
                EdgeDetector edgeDetector = new EdgeDetector()
                {
                    Threshold = (byte)18,
                    SparseDistance = 3,
                    WeightPreviousPoint = (float)2.0,
                    WeightCurrentPoint = (float)1.0,
                    WeightAfterPoint = (float)2.0,
                };

                EdgeDetector edgeDetector2 = new EdgeDetector()
                {
                    Threshold = (byte)20,
                    SparseDistance = 5,
                    WeightPreviousPoint = (float)0.5,
                    WeightCurrentPoint = (float)1.0,
                    WeightAfterPoint = (float)0.5,
                };

                Mat imagenBordes = edgeDetector.EdgeImage(imagenGrisContraste);
                Mat imagenBordes2 = edgeDetector2.EdgeImage(imagenGrisContraste);
                Mat imagenBinaria, imagenAberturaRelleno;
                CalculoMatrizBinariaYRelleno(imagenBordes2, out imagenBinaria, out imagenAberturaRelleno);

                Mat mascaraInv = 255 - imagenAberturaRelleno;

                Mat DistSureFg = new Mat();
                Mat AreasSureFg = new Mat();
                Mat Unknown = new Mat();
                AreasSureFg += 1;
                Cv2.DistanceTransform(imagenAberturaRelleno, DistSureFg, DistanceTypes.L1, DistanceMaskSize.Mask5);
                int numAreas = Cv2.ConnectedComponents(imagenAberturaRelleno, AreasSureFg, PixelConnectivity.Connectivity8);

                float[,] distValues = new float[DistSureFg.Rows, DistSureFg.Cols];

                for (int i = 0; i < DistSureFg.Rows; i++)
                {
                    for (int j = 0; j < DistSureFg.Cols; j++)
                    {
                        distValues[i, j] = DistSureFg.At<float>(i, j);
                    }
                }

                Segment[] segments = new Segment[numAreas];

                for (int i = 0; i < AreasSureFg.Rows; i++)
                {
                    for (int j = 0; j < AreasSureFg.Cols; j++)
                    {
                        int m = AreasSureFg.At<Int32>(i, j);
                        byte pixelSurrounding = 0;
                        float distance = (float)0;

                        //if (i >= 1)
                        //{
                        //    distance = distValues[i - 1, j];
                        //    if (distance == 2)
                        //    {
                        //        pixelSurrounding |= Segment.PIXEL_SURROUNDED_LEFT;
                        //    }
                        //}
                        //if (i < AreasSureFg.Rows - 1)
                        //{
                        //    distance = distValues[i + 1, j];
                        //    if (distance == 2)
                        //    {
                        //        pixelSurrounding |= Segment.PIXEL_SURROUNDED_RIGHT;
                        //    }
                        //}
                        //if (j >= 1)
                        //{
                        //    distance = distValues[i, j - 1];
                        //    if (distance == 2)
                        //    {
                        //        pixelSurrounding |= Segment.PIXEL_SURROUNDED_DOWN;
                        //    }
                        //}
                        //if (j < AreasSureFg.Cols - 1)
                        //{
                        //    distance = distValues[i, j + 1];
                        //    if (distance == 2)
                        //    {
                        //        pixelSurrounding |= Segment.PIXEL_SURROUNDED_UP;
                        //    }
                        //}

                        SegmentPixelData newPixel = new SegmentPixelData()
                        {
                            Distance = distValues[i, j],
                            CoordsXY = new int[] { i, j },
                            Concave = 0,
                            Indexes = new int[] { -1, -1 },
                            PixelsSurrounding = pixelSurrounding,
                            SubsegmentLabel = 0,
                        };

                        if(segments[m] == null)
                        {
                            segments[m] = new Segment()
                            {
                                SegmentId = m,
                                PixelData = new List<SegmentPixelData>(),
                            };
                        }
                        else
                        {
                            segments[m].MaxDistance = (segments[m].MaxDistance > newPixel.Distance) ? (int)segments[m].MaxDistance : (int)newPixel.Distance;
                            segments[m].PixelData.Add(newPixel);
                        }
                    }
                }

                Mat Centroides = new Mat();
                imagenAberturaRelleno.CopyTo(Centroides);
                var indexadorCentroides = Centroides.GetGenericIndexer<byte>();
                var indexadorFiguras = AreasSureFg.GetGenericIndexer<Int32>();

                foreach (var s in segments.Where(s => s.Circularity <= 0.9))
                {
                    int distancia = 0;
                    if(s.Circularity > 0.7)
                    {
                        distancia = 5;
                    }
                    else if (s.Circularity > 0.5)
                    {
                        distancia = 5;
                    }
                    else if (s.Circularity > 0.25)
                    {
                        distancia = 6;
                    }
                    else
                    {
                        distancia = 6;
                    }

                    distancia = (distancia < s.MaxDistance) ? distancia : s.MaxDistance - 1;

                    foreach (var p in s.PixelData.Where(p => p.Distance <= distancia))
                    {
                        if (imagenAberturaRelleno.At<byte>(p.CoordsXY[0], p.CoordsXY[1]) != (byte)0)
                        {
                            indexadorCentroides[p.CoordsXY[0], p.CoordsXY[1]] = 0;
                        }
                    }
                }

                Cv2.Subtract(imagenAberturaRelleno + 255, Centroides, Unknown);

                #region segmentStuff
                //List<int> indexConcavos = segments.Where(s => s.Circularity > 1).Select(s => s.SegmentId).ToList();


                //foreach (var s in segments.Where(s => s.Circularity < 1.1 && s.Circularity > 0.9))
                //{
                //    foreach (var p in s.PixelData/*.Where(p => p.Distance == 1)*/)
                //    {
                //        if (imagenAberturaRelleno.At<byte>(p.CoordsXY[0], p.CoordsXY[1]) != (byte)0)
                //        {
                //            indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 255;
                //        }
                //    }
                //}

                //foreach (var s in segments.Where(s => s.Circularity >= 1.1))
                //{
                //    foreach (var p in s.PixelData/*.Where(p => p.Distance == 1)*/)
                //    {
                //        if (imagenAberturaRelleno.At<byte>(p.CoordsXY[0], p.CoordsXY[1]) != (byte)0)
                //        {
                //            indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 255;
                //        }
                //    }
                //}

                //foreach (var s in segments)
                //{
                //    s.SetPixelConcavity();
                //    s.Segmentation();
                //    foreach (var p in s.PixelData.Where(p => p.Distance == 1))
                //    {
                //        if (p.Concave == 1)
                //        {
                //            indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 255;
                //        }
                //        if (p.Concave == -1)
                //        {
                //            indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 255;
                //        }
                //    }
                //}

                //foreach (var s in segments)
                //{
                //    //s.SetPixelConcavity();
                //    //s.Segmentation();
                //    foreach (var p in s.PixelData.Where(p => p.Distance == 2))
                //    {
                //        indexadorFiguras[p.CoordsXY[0], p.CoordsXY[1]] = 230;
                //    }
                //}

                //imagenAberturaRelleno.CopyTo(SureFg);
                #endregion

                Mat colormap = new Mat();
                Mat Marcadores = new Mat();
                Cv2.ConnectedComponents(Centroides, Marcadores);
                Marcadores = Marcadores + 1;
                var indexador2 = Marcadores.GetGenericIndexer<Int32>();
                for (int i = 0; i < Unknown.Rows; i++)
                    for (int j = 0; j < Unknown.Cols; j++)
                        if (Unknown.At<byte>(i, j) == 255)
                            indexador2[i, j] = 0;

                Marcadores.CopyTo(colormap);
                colormap.ConvertTo(colormap, MatType.CV_8UC3);
                Cv2.ApplyColorMap(colormap, colormap, ColormapTypes.Rainbow);
                Cv2.ImWrite("D:\\Dictuc\\marcadores.png", Marcadores);

                //Mat img1 = new Mat();
                //imagen.CopyTo(img1);
                Mat DistColor = new Mat();
                //imagenGrisContraste = 255 - imagenGrisContraste;
                Cv2.CvtColor(imagenAberturaRelleno, DistColor, ColorConversionCodes.GRAY2BGR);
                DistColor.ConvertTo(DistColor, MatType.CV_8U);

                Cv2.Watershed(DistColor, Marcadores);


                Cv2.ImWrite("D:\\Dictuc\\watersheedIn.png", DistColor);

                var indexador4 = imagen.GetGenericIndexer<Vec3i>();
                //for (int i = 0; i < imagen.Rows; i++)
                //{
                //    for (int j = 0; j < imagen.Cols; j++)
                //    {
                //        //if (Centroides.At<byte>(i, j) > 0)
                //        //    indexador4[i, j] = new Vec3i(0, 0, 255);
                //        if (Marcadores.At<Int32>(i, j) == -1)
                //            indexador4[i, j] = new Vec3i(255, 20, 20);
                //    }
                //}


                for (int i = 0; i < imagen.Rows; i++)
                {
                    for (int j = 0; j < imagen.Cols; j++)
                    {
                        //if (Centroides.At<byte>(i, j) > 0)
                        //    indexador4[i, j] = new Vec3i(0, 0, 255);
                        if (imagenBordes.At<char>(i, j) > 0)
                            indexador4[i, j] = new Vec3i(255, 20, 20);
                    }
                }

                Mat seg = new Mat();
                Marcadores.CopyTo(seg);
                var indexador5 = seg.GetGenericIndexer<int>();
                for (int i = 0; i < Marcadores.Rows; i++)
                {
                    for (int j = 0; j < Marcadores.Cols; j++)
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

                SimpleBlobDetector.Params params1 = new SimpleBlobDetector.Params()
                {
                    MinThreshold = 0,
                    MaxThreshold = 255,
                    FilterByArea = true,
                    MinArea = 15,
                    FilterByCircularity = false,
                    MinCircularity = (float)0.01,
                    FilterByConvexity = false,
                    MinConvexity = (float)0.1,
                    FilterByInertia = false,
                    MinInertiaRatio = (float)0.01,
                };
                SimpleBlobDetector detectorBlobs = SimpleBlobDetector.Create(params1);
                KeyPoint[] segmentosBlob = detectorBlobs.Detect(edge1);

                Mat segmentosBlobMat = new Mat(1, segmentosBlob.Count(), MatType.CV_32FC1);
                var indexador6 = segmentosBlobMat.GetGenericIndexer<float>();
                for (int i = 0; i < segmentosBlob.Count(); i++)
                {
                    indexador6[0, i] = segmentosBlob[i].Size;
                }

                Mat hist = new Mat();
                Rangef[] ranges = { new Rangef(0, (float)segmentosBlob.Max(x => x.Size)) };
                Cv2.CalcHist(new Mat[] { segmentosBlobMat }, new int[] { 0 }, null, hist, 1, new int[] { 100 }, ranges, uniform:true, accumulate: true);
                float[] histAcumulado = new float[hist.Rows];
                float[] histAcumuladoPorcentaje = new float[11];

                histAcumulado[0] = hist.At<float>(0, 0);

                for (int i = 1; i < hist.Rows; i++)
                {
                    histAcumulado[i] = hist.At<float>(i, 0) + histAcumulado[i - 1];
                }

                int k = 1;
                for (int i = 1; i < histAcumuladoPorcentaje.Count(); i++)
                {
                    for (; k < hist.Rows; k++)
                    {
                        float porcentajeActual = histAcumulado[k] / segmentosBlob.Count() * 100;
                        float porcentajeAnterior = histAcumulado[k - 1] / segmentosBlob.Count() * 100;
                        float porcentajeRequerido = (float)((i < 10) ? i * 10 : 99.3); 
                        if (porcentajeRequerido <= porcentajeActual)
                        {
                            float tamañoPorcentajeActual = (float)(k * (float)segmentosBlob.Max(x => x.Size) / 100.0);
                            float tamañoPorcentajeAnterior = (float)((k - 1) * (float)segmentosBlob.Max(x => x.Size) / 100.0);
                            float tasaVariacionTamañoPorcentaje = (tamañoPorcentajeActual - tamañoPorcentajeAnterior) / (porcentajeActual - porcentajeAnterior);
                            histAcumuladoPorcentaje[i] = tamañoPorcentajeAnterior + tasaVariacionTamañoPorcentaje * (i * 10 - porcentajeAnterior);
                            break;
                        }
                    }
                }

                for (int i = 0; i< histAcumuladoPorcentaje.Count(); i ++)
                {
                    Console.Write(histAcumuladoPorcentaje[i] + ",");
                }
                Console.WriteLine("");

                //            data1 = [];

                //              for i in range(0, len(keypoints1)):

                //                data1.append(keypoints1[i].size * coefTamano)
                //                #tamano.write(str(i)+'\t'+str(keypoints1[i].size*2*0.3)+'\n')
                //  cv2.line(im_with_keypoints1, (int(float(keypoints1[i].pt[0] - keypoints1[i].size)), int(float(keypoints1[i].pt[1]))), (int(float(keypoints1[i].pt[0] + keypoints1[i].size)), int(float(keypoints1[i].pt[1]))), (255, 0, 0), 1)

                //                cv2.line(im_with_keypoints1, (int(float(keypoints1[i].pt[0])), int(float(keypoints1[i].pt[1] - keypoints1[i].size))), (int(float(keypoints1[i].pt[0])), int(float(keypoints1[i].pt[1] + keypoints1[i].size))), (255, 0, 0), 1)


                //# print(data1)
                //n1, bins1, patches1 = hist(data1, 200,[0, max(data1)], normed = 100, cumulative = True, bottom = True, histtype = 'stepfilled', align = 'mid', orientation = 'vertical', rwidth = 1, log = False, color = "r")

                //              tamano = open(temp + "instancia_" + instancia + ".txt", "w")


                //              x = np.array(bins1)

                //              y = np.append([0], n1)

                //                  xnew = [x[1], x[21], x[36], x[45], x[53], x[60], x[69], x[78], x[88], x[97], x[200]]
                //ynew = [y[1], y[21], y[36], y[45], y[53], y[60], y[69], y[78], y[88], y[97], y[200]]

                //tamano.write('INSERT INTO [dbo].[Granulometria](Cod_Instancia,Fecha,P_10,P_20,P_30,P_40,P_50,P_60,P_70,P_80,P_90,P_100, Filename) values (')
                //tamano.write(instancia + ",CONVERT(datetime, '" + sys.argv[1][0:4] + "-" + sys.argv[1][4:6] + "-" + sys.argv[1][6:8] + ' ' + sys.argv[1][9:11] + ':' + sys.argv[1][11:13] + ':' + sys.argv[1][13:15] + "', 120)")

                //for j in range(1, len(xnew)):
                //  #tamano.write (str(j)+'\t'+str(round(xnew[j],1))+'\t'+str(round(ynew[j]*100,2))+'\n')
                //  tamano.write(',' + str(round(xnew[j], 1)))

                //tamano.write(",'" + sys.argv[1] + " - Resultado.jpg'")
                //tamano.write(')')

                //CvXImgProc.Thinning(mascaraInv, mascaraInv, ThinningTypes.ZHANGSUEN);

                Mat imWithKeypoints1 = new Mat();
                Cv2.DrawKeypoints(imagen, segmentosBlob, imWithKeypoints1, new Scalar(0, 0, 255), DrawMatchesFlags.DrawRichKeypoints);


                var dataTamaños = segmentosBlob.Select(s => s.Size).ToArray();


                Cv2.ImWrite("D:\\Dictuc\\output0" + numImg + ".png", imagen);
                Cv2.ImWrite("D:\\Dictuc\\output1" + numImg++ + ".png", imWithKeypoints1);

                Cv2.ImShow("Segmentado", imagen);
                Cv2.ImShow("GrisContraste", imagenGrisContraste);
                Cv2.ImShow("bordes90", imagenBordes);
                Cv2.ImShow("bordes50", imagenBordes2);

                salida.Write(imagen);

                //System.Threading.Thread.Sleep(10);
                Cv2.WaitKey(10);

                imagenRuidoFiltrado.Release();
                imagenGrisContraste.Release();
                imagenGrisFrecAltasProc.Release();
                imagenBordes.Release();
                imagenBinaria.Release();
                imagenAberturaRelleno.Release();
            }
        }
    }
}