using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;

using AVT.VmbAPINET;
using System.Net.NetworkInformation;
using System.Net;

namespace ConsoleApp1.Utils
{
    class ProcesoImagen
    {
        private static Camera m_Camera;
        private static Frame rawFrame;
        public static Bitmap bitmap;

        public static void StartCamera(string ipCamara)
        {
            Vimba sys = new Vimba();
            FeatureCollection features = null;
            Feature feature = null;
            long payloadSize;
            Frame[] frameArray = new Frame[3];
            sys.Startup();
            m_Camera = sys.OpenCameraByID(ipCamara, VmbAccessModeType.VmbAccessModeFull);
            m_Camera.LoadCameraSettings("D:\\Dictuc\\camaras.xml");
            m_Camera.OnFrameReceived += new Camera.OnFrameReceivedHandler(OnFrameReceived);
            features = m_Camera.Features;
            feature = features["PayloadSize"];
            payloadSize = feature.IntValue;
            for (int index = 0; index < frameArray.Length; ++index)
            {
                frameArray[index] = new Frame(payloadSize);
                m_Camera.AnnounceFrame(frameArray[index]);
            }
            m_Camera.StartCapture();
            for (int index = 0; index < frameArray.Length; ++index)
            {
                m_Camera.QueueFrame(frameArray[index]);
            }
            feature = features["AcquisitionMode"];
            feature.EnumValue = "Continuous";
            feature = features["AcquisitionStart"];
            feature.RunCommand();
        }

        public static void StopCamera()
        {
            FeatureCollection features = m_Camera.Features;
            Feature feature = features["AcquisitionStop"];
            feature.RunCommand();
            m_Camera.EndCapture();
            m_Camera.FlushQueue();
            m_Camera.RevokeAllFrames();
            m_Camera.Close();
        }

        public static void OnFrameReceived(Frame frame)
        {
            if (VmbFrameStatusType.VmbFrameStatusComplete == frame.ReceiveStatus)
            {
                rawFrame = frame;
            }
            m_Camera.QueueFrame(frame);
        }

        public static void GetLastFrame()
        {
            //Mat output = new Mat((int)rawFrame.Width, (int)rawFrame.Height, MatType.CV_8UC3);
            bitmap = null;
            bitmap = new Bitmap((int)rawFrame.Width, (int)rawFrame.Height, PixelFormat.Format24bppRgb);
            try
            {
                rawFrame.Fill(ref bitmap);
                GuardarImagen(bitmap, "D:\\Dictuc\\", "input.png");
                //BitmapConverter.ToMat(bitmap, output);
            }
            catch /*(Exception e)*/
            {

            }
        }

        public static Mat Bitmap2Mat(Bitmap img)
        {
            Mat imgMat = new Mat();
            BitmapConverter.ToMat(img, imgMat);
            return imgMat;
        }

        public static void GuardarImagen(Bitmap img, string dir, string nombre)
        {
            DateTime t = DateTime.Now;

            System.Threading.Thread.Sleep(100);
            string archivo = dir + nombre;

            try
            {
                img.Save(archivo, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception e)
            {

            }
            
        }

    }
}
