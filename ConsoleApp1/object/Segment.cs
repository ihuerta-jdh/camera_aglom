using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Objects
{
    class SegmentPixelData
    {
        public int[] CoordsXY { get; set; }
        public float Distance { get; set; }
        public int Concave { get; set; }
        public int[] Indexes { get; set; }
        public byte PixelsSurrounding { get; set; }
        public byte SubsegmentLabel { get; set; }
    }

    class Segment
    {
        static public byte PIXEL_SURROUNDED_LEFT = 0b0001;
        static public byte PIXEL_SURROUNDED_RIGHT = 0b0010;
        static public byte PIXEL_SURROUNDED_UP = 0b010;
        static public byte PIXEL_SURROUNDED_DOWN = 0b1000;

        public int SegmentId { get; set; }
        public List<SegmentPixelData> PixelData { get; set; }
        public float[] Center
        {
            get
            {
                float[] centerEstimator = new float[2] { 0, 0 };
                foreach (var coord in PixelData.Select(d => d.CoordsXY))
                {
                    centerEstimator[0] += (float)coord[0];
                    centerEstimator[1] += (float)coord[1];
                }
                centerEstimator[0] /= PixelData.Count;
                centerEstimator[1] /= PixelData.Count;
                return centerEstimator;
            }
        }

        public float Area
        {
            get
            {
                return PixelData.Count;
            }
        }

        public float Perimeter
        {
            get
            {
                return PixelData.Where(d => d.Distance == 1).Count();
            }
        }

        public double Circularity
        {
            get
            {
                return 4 * Math.PI * Area / Math.Pow(Perimeter, 2);
            }
        }

        public void SetPixelConcavity()
        {
            List<List<SegmentPixelData>> prev2HLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> prevHLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> hLines = new List<List<SegmentPixelData>>();

            List<List<SegmentPixelData>> prev2VLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> prevVLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> vLines = new List<List<SegmentPixelData>>();

            List<List<SegmentPixelData>> prevHPrevPixelOfLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> prevVPrevPixelOfLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> hPrevPixelOfLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> prevHNextPixelOfLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> hNextPixelOfLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> vPrevPixelOfLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> prevVNextPixelOfLines = new List<List<SegmentPixelData>>();
            List<List<SegmentPixelData>> vNextPixelOfLines = new List<List<SegmentPixelData>>();

            List<SegmentPixelData> hOrderer = PixelData.Where(p => p.Distance == 1).ToList();
            List<SegmentPixelData> vOrderer = hOrderer.OrderBy(p => p.CoordsXY[1]).ToList();

            for (int i = 0; i < hOrderer.Count; i++)
            {
                hOrderer[i].Indexes[0] = i;
                vOrderer[i].Indexes[1] = i;
            }

            int prevXCoord = -1;
            int prevYCoord = -1;

            List<SegmentPixelData> line = new List<SegmentPixelData>();
            List<SegmentPixelData> vPrevPixelOfLine = new List<SegmentPixelData>();
            List<SegmentPixelData> vNextPixelOfLine = new List<SegmentPixelData>();
            List<SegmentPixelData> hPrevPixelOfLine = new List<SegmentPixelData>();
            List<SegmentPixelData> hNextPixelOfLine = new List<SegmentPixelData>();

            for (int n = 0; n < hOrderer.Count; n++)
            {
                // ver si cambio en la coordenada x
                if (prevXCoord != hOrderer[n].CoordsXY[0])
                {
                    if (prevHLines.Count > 2)
                    {
                        if (prev2HLines.Count >= 2)
                        {
                            for (int i = 0; i < prev2HLines.Count - 1; i++)
                            {
                                int firstItem = prev2HLines[i].Last().CoordsXY[1];
                                int lastItem = prev2HLines[i + 1].First().CoordsXY[1];
                                for (int j = 1; j < prevHLines.Count - 1; j++)
                                {
                                    if (prevHLines[j].First().CoordsXY[1] > firstItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevHLines[j].Last().CoordsXY[1] > lastItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevHLines[j].First().CoordsXY[1] < firstItem - 1)
                                    {
                                        continue;
                                    }

                                    if (prevHLines[j].Last().CoordsXY[1] < lastItem - 1)
                                    {
                                        continue;
                                    }

                                    for (int k = 0; k < prevHLines[j].Count; k++)
                                    {
                                        SegmentPixelData pixel = prevHLines[j].ElementAt(k);
                                        SegmentPixelData pVPixel = prevVPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nVPixel = prevVNextPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData pHPixel = prevHPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nHPixel = prevHNextPixelOfLines[j].ElementAt(k);
                                        pixel.Concave = 1;
                                        if (pVPixel.Concave != 1)
                                            pVPixel.Concave = -1;
                                        if (nVPixel.Concave != 1)
                                            nVPixel.Concave = -1;
                                        if (pHPixel.Concave != 1)
                                            pHPixel.Concave = -1;
                                        if (nHPixel.Concave != 1)
                                            nHPixel.Concave = -1;
                                    }
                                }
                            }
                        }

                        if (hLines.Count >= 2)
                        {
                            for (int i = 0; i < hLines.Count - 1; i++)
                            {
                                if (hLines[i].First().Concave == 1 || hLines[i + 1].First().Concave == 1)
                                    continue;
                                int firstItem = hLines[i].Last().CoordsXY[1];
                                int lastItem = hLines[i + 1].First().CoordsXY[1];
                                for (int j = 1; j < prevHLines.Count - 1; j++)
                                {
                                    if (prevHLines[j].First().CoordsXY[1] > firstItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevHLines[j].Last().CoordsXY[1] > lastItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevHLines[j].First().CoordsXY[1] < firstItem - 1)
                                    {
                                        continue;
                                    }

                                    if (prevHLines[j].Last().CoordsXY[1] < lastItem - 1)
                                    {
                                        continue;
                                    }

                                    for (int k = 0; k < prevHLines[j].Count; k++)
                                    {
                                        SegmentPixelData pixel = prevHLines[j].ElementAt(k);
                                        SegmentPixelData pVPixel = prevVPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nVPixel = prevVNextPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData pHPixel = prevHPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nHPixel = prevHNextPixelOfLines[j].ElementAt(k);
                                        pixel.Concave = 1;
                                        if (pVPixel.Concave != 1)
                                            pVPixel.Concave = -1;
                                        if (nVPixel.Concave != 1)
                                            nVPixel.Concave = -1;
                                        if (pHPixel.Concave != 1)
                                            pHPixel.Concave = -1;
                                        if (nHPixel.Concave != 1)
                                            nHPixel.Concave = -1;
                                    }
                                }
                            }
                        }
                    }

                    prev2HLines = prevHLines;
                    prevHLines = hLines;
                    prevVPrevPixelOfLines = vPrevPixelOfLines;
                    prevVNextPixelOfLines = vNextPixelOfLines;
                    prevHPrevPixelOfLines = hPrevPixelOfLines;
                    prevHNextPixelOfLines = hNextPixelOfLines;

                    hLines = new List<List<SegmentPixelData>>();
                    vPrevPixelOfLines = new List<List<SegmentPixelData>>();
                    vNextPixelOfLines = new List<List<SegmentPixelData>>();
                    hPrevPixelOfLines = new List<List<SegmentPixelData>>();
                    hNextPixelOfLines = new List<List<SegmentPixelData>>();

                    line = new List<SegmentPixelData>();
                    vPrevPixelOfLine = new List<SegmentPixelData>();
                    vNextPixelOfLine = new List<SegmentPixelData>();
                    hPrevPixelOfLine = new List<SegmentPixelData>();
                    hNextPixelOfLine = new List<SegmentPixelData>();

                    hLines.Add(line);
                    vPrevPixelOfLines.Add(vPrevPixelOfLine);
                    vNextPixelOfLines.Add(vNextPixelOfLine);
                    hPrevPixelOfLines.Add(hPrevPixelOfLine);
                    hNextPixelOfLines.Add(hNextPixelOfLine);
                }
                else
                {
                    int lastYCoord = line.Last().CoordsXY[1];
                    // ver si es la misma línea
                    if (hOrderer[n].CoordsXY[1] != lastYCoord + 1)
                    {
                        line = new List<SegmentPixelData>();
                        vPrevPixelOfLine = new List<SegmentPixelData>();
                        vNextPixelOfLine = new List<SegmentPixelData>();
                        hPrevPixelOfLine = new List<SegmentPixelData>();
                        hNextPixelOfLine = new List<SegmentPixelData>();

                        hLines.Add(line);
                        vPrevPixelOfLines.Add(vPrevPixelOfLine);
                        vNextPixelOfLines.Add(vNextPixelOfLine);
                        hPrevPixelOfLines.Add(hPrevPixelOfLine);
                        hNextPixelOfLines.Add(hNextPixelOfLine);
                    }
                }

                #region AddLineElements
                line.Add(hOrderer[n]);
                if (hOrderer[n].Indexes[1] >= 1)
                    if (vOrderer[hOrderer[n].Indexes[1] - 1].CoordsXY[1] == hOrderer[n].CoordsXY[1])
                        vPrevPixelOfLine.Add(vOrderer[hOrderer[n].Indexes[1] - 1]);
                    else
                        vPrevPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                if (hOrderer[n].Indexes[1] < vOrderer.Count - 1)
                    if (vOrderer[hOrderer[n].Indexes[1] + 1].CoordsXY[1] == hOrderer[n].CoordsXY[1])
                        vNextPixelOfLine.Add(vOrderer[hOrderer[n].Indexes[1] + 1]);
                    else
                        vNextPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                if (n >= 1)
                    if (hOrderer[n - 1].CoordsXY[0] == hOrderer[n].CoordsXY[0])
                        hPrevPixelOfLine.Add(hOrderer[n - 1]);
                    else
                        hPrevPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                if (n < hOrderer.Count - 1)
                    if (hOrderer[n + 1].CoordsXY[0] == hOrderer[n].CoordsXY[0])
                        hNextPixelOfLine.Add(hOrderer[n + 1]);
                    else
                        hNextPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                #endregion

                prevXCoord = hOrderer[n].CoordsXY[0];
            }

            prevXCoord = -1;
            prevYCoord = -1;
            line = new List<SegmentPixelData>();
            vPrevPixelOfLine = new List<SegmentPixelData>();
            vNextPixelOfLine = new List<SegmentPixelData>();
            hPrevPixelOfLine = new List<SegmentPixelData>();
            hNextPixelOfLine = new List<SegmentPixelData>();

            prevHPrevPixelOfLines = new List<List<SegmentPixelData>>();
            prevVPrevPixelOfLines = new List<List<SegmentPixelData>>();
            hPrevPixelOfLines = new List<List<SegmentPixelData>>();
            prevHNextPixelOfLines = new List<List<SegmentPixelData>>();
            hNextPixelOfLines = new List<List<SegmentPixelData>>();
            vPrevPixelOfLines = new List<List<SegmentPixelData>>();
            prevVNextPixelOfLines = new List<List<SegmentPixelData>>();
            vNextPixelOfLines = new List<List<SegmentPixelData>>();

            for (int n = 0; n < hOrderer.Count; n++)
            {
                // ver si cambio en la coordenada x
                if (prevYCoord != vOrderer[n].CoordsXY[1])
                {
                    if (prevVLines.Count > 2)
                    {
                        if (prev2VLines.Count >= 2)
                        {
                            for (int i = 0; i < prev2VLines.Count - 1; i++)
                            {
                                int firstItem = prev2VLines[i].Last().CoordsXY[0];
                                int lastItem = prev2VLines[i + 1].First().CoordsXY[0];
                                for (int j = 1; j < prevVLines.Count - 1; j++)
                                {
                                    if (prevVLines[j].First().CoordsXY[0] > firstItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevVLines[j].Last().CoordsXY[0] > lastItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevVLines[j].First().CoordsXY[0] < firstItem - 1)
                                    {
                                        continue;
                                    }

                                    if (prevVLines[j].Last().CoordsXY[0] < lastItem - 1)
                                    {
                                        continue;
                                    }

                                    for (int k = 0; k < prevVLines[j].Count; k++)
                                    {
                                        SegmentPixelData pixel = prevVLines[j].ElementAt(k);
                                        SegmentPixelData pVPixel = prevVPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nVPixel = prevVNextPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData pHPixel = prevHPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nHPixel = prevHNextPixelOfLines[j].ElementAt(k);
                                        pixel.Concave = 1;
                                        if (pVPixel.Concave != 1)
                                            pVPixel.Concave = -1;
                                        if (nVPixel.Concave != 1)
                                            nVPixel.Concave = -1;
                                        if (pHPixel.Concave != 1)
                                            pHPixel.Concave = -1;
                                        if (nHPixel.Concave != 1)
                                            nHPixel.Concave = -1;
                                    }
                                }
                            }
                        }

                        if (vLines.Count >= 2)
                        {
                            for (int i = 0; i < vLines.Count - 1; i++)
                            {
                                if (vLines[i].First().Concave == 1 || vLines[i + 1].First().Concave == 1)
                                    continue;
                                int firstItem = vLines[i].Last().CoordsXY[0];
                                int lastItem = vLines[i + 1].First().CoordsXY[0];
                                for (int j = 1; j < prevVLines.Count - 1; j++)
                                {
                                    if (prevVLines[j].First().CoordsXY[0] > firstItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevVLines[j].Last().CoordsXY[0] > lastItem + 1)
                                    {
                                        break;
                                    }

                                    if (prevVLines[j].First().CoordsXY[0] < firstItem - 1)
                                    {
                                        continue;
                                    }

                                    if (prevVLines[j].Last().CoordsXY[0] < lastItem - 1)
                                    {
                                        continue;
                                    }

                                    for (int k = 0; k < prevVLines[j].Count; k++)
                                    {
                                        SegmentPixelData pixel = prevVLines[j].ElementAt(k);
                                        SegmentPixelData pVPixel = prevVPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nVPixel = prevVNextPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData pHPixel = prevHPrevPixelOfLines[j].ElementAt(k);
                                        SegmentPixelData nHPixel = prevHNextPixelOfLines[j].ElementAt(k);
                                        pixel.Concave = 1;
                                        if (pVPixel.Concave != 1)
                                            pVPixel.Concave = -1;
                                        if (nVPixel.Concave != 1)
                                            nVPixel.Concave = -1;
                                        if (pHPixel.Concave != 1)
                                            pHPixel.Concave = -1;
                                        if (nHPixel.Concave != 1)
                                            nHPixel.Concave = -1;
                                    }
                                }
                            }
                        }
                    }

                    prev2VLines = prevVLines;
                    prevVLines = vLines;
                    prevVPrevPixelOfLines = vPrevPixelOfLines;
                    prevVNextPixelOfLines = vNextPixelOfLines;
                    prevHPrevPixelOfLines = hPrevPixelOfLines;
                    prevHNextPixelOfLines = hNextPixelOfLines;

                    vLines = new List<List<SegmentPixelData>>();
                    vPrevPixelOfLines = new List<List<SegmentPixelData>>();
                    vNextPixelOfLines = new List<List<SegmentPixelData>>();
                    hPrevPixelOfLines = new List<List<SegmentPixelData>>();
                    hNextPixelOfLines = new List<List<SegmentPixelData>>();

                    line = new List<SegmentPixelData>();
                    vPrevPixelOfLine = new List<SegmentPixelData>();
                    vNextPixelOfLine = new List<SegmentPixelData>();
                    hPrevPixelOfLine = new List<SegmentPixelData>();
                    hNextPixelOfLine = new List<SegmentPixelData>();

                    vLines.Add(line);
                    vPrevPixelOfLines.Add(vPrevPixelOfLine);
                    vNextPixelOfLines.Add(vNextPixelOfLine);
                    hPrevPixelOfLines.Add(hPrevPixelOfLine);
                    hNextPixelOfLines.Add(hNextPixelOfLine);
                }
                else
                {
                    int lastXCoord = line.Last().CoordsXY[0];
                    // ver si es la misma línea
                    if (vOrderer[n].CoordsXY[0] != lastXCoord + 1)
                    {
                        line = new List<SegmentPixelData>();
                        vPrevPixelOfLine = new List<SegmentPixelData>();
                        vNextPixelOfLine = new List<SegmentPixelData>();
                        hPrevPixelOfLine = new List<SegmentPixelData>();
                        hNextPixelOfLine = new List<SegmentPixelData>();

                        vLines.Add(line);
                        vPrevPixelOfLines.Add(vPrevPixelOfLine);
                        vNextPixelOfLines.Add(vNextPixelOfLine);
                        hPrevPixelOfLines.Add(hPrevPixelOfLine);
                        hNextPixelOfLines.Add(hNextPixelOfLine);
                    }
                }

                #region AddLineElements
                line.Add(vOrderer[n]);
                if (n >= 1)
                    if (vOrderer[n - 1].CoordsXY[1] == vOrderer[n].CoordsXY[1])
                        vPrevPixelOfLine.Add(vOrderer[n - 1]);
                    else
                        vPrevPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                if (n < vOrderer.Count - 1)
                    if (vOrderer[n + 1].CoordsXY[1] == vOrderer[n].CoordsXY[1])
                        vNextPixelOfLine.Add(vOrderer[n + 1]);
                    else
                        vNextPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                if (vOrderer[n].Indexes[0] >= 1)
                    if (hOrderer[vOrderer[n].Indexes[0] - 1].CoordsXY[0] == vOrderer[n].CoordsXY[0])
                        hPrevPixelOfLine.Add(hOrderer[vOrderer[n].Indexes[0] - 1]);
                    else
                        hPrevPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                if (vOrderer[n].Indexes[0] < hOrderer.Count - 1)
                    if (hOrderer[vOrderer[n].Indexes[0] + 1].CoordsXY[0] == vOrderer[n].CoordsXY[0])
                        hNextPixelOfLine.Add(hOrderer[vOrderer[n].Indexes[0] + 1]);
                    else
                        hNextPixelOfLine.Add(new SegmentPixelData() { CoordsXY = new int[] { -1, -1 } });
                #endregion

                prevYCoord = vOrderer[n].CoordsXY[1];
            }
        }

        public List<Segment> Segmentation()
        {
            List<Segment> segments = new List<Segment>();

            if (Circularity >= 1.1)
            {
                double currentCircularity = Circularity;

                List<SegmentPixelData> possibleHCutPixels = PixelData.Where(p => p.Distance == 1 && (p.Concave == -1 || p.Concave == 1)).ToList();
                List<SegmentPixelData> possibleVCutPixels = possibleHCutPixels.OrderBy(p => p.CoordsXY[1]).ToList();

                for (int i = 0; i < possibleHCutPixels.Count; i++)
                {
                    possibleHCutPixels[i].Indexes[0] = i;
                    possibleVCutPixels[i].Indexes[1] = i;
                }

                int minLength = int.MaxValue;
                SegmentPixelData cutPixel1;
                SegmentPixelData cutPixel2;

                for (int n = 0; n < possibleHCutPixels.Count; n++)
                {
                    int m = possibleHCutPixels[n].Indexes[1];
                    if (possibleHCutPixels[n].Concave == -1)
                        continue;
                    List<SegmentPixelData> cutPixels = new List<SegmentPixelData>();
                    if (n > 0 && (possibleHCutPixels[n].PixelsSurrounding & PIXEL_SURROUNDED_LEFT) > 0)
                    {
                        if (possibleHCutPixels[n].CoordsXY[0] - possibleHCutPixels[n - 1].CoordsXY[0] > 1)
                        {

                        }
                    }
                    if (n < possibleHCutPixels.Count - 1 && (possibleHCutPixels[n].PixelsSurrounding & PIXEL_SURROUNDED_RIGHT) > 0)
                    {
                        if (possibleHCutPixels[n + 1].CoordsXY[0] - possibleHCutPixels[n].CoordsXY[0] > 1)
                        {

                        }
                    }
                    if (m > 0 && (possibleHCutPixels[n].PixelsSurrounding & PIXEL_SURROUNDED_DOWN) > 0)
                    {
                        if (possibleHCutPixels[n].CoordsXY[0] - possibleVCutPixels[m - 1].CoordsXY[0] > 1)
                        {

                        }
                    }
                    if (m < possibleHCutPixels.Count - 1 && (possibleHCutPixels[n].PixelsSurrounding & PIXEL_SURROUNDED_UP) > 0)
                    {
                        if (possibleVCutPixels[m + 1].CoordsXY[0] - possibleHCutPixels[n].CoordsXY[0] > 1)
                        {

                        }
                    }
                }
            }
            return segments;
        }

        public void LabelSubSegments(byte labelSegment1, byte labelSegment2, List<SegmentPixelData> cutLine)
        {

        }
    }
}
