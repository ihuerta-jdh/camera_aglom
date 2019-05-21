using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using AVT.VmbAPINET;
using System.Net.NetworkInformation;
using System.Net;

namespace ConsoleApp1.Utils
{
    class Network
    {
        public class ResultadoPing
        {
            public bool conectado { get; set; }
            public string direccion { get; set; }
            public long roundTripTime { get; set; }
            public string tiempoVivir { get; set; }
            public string sinFragmentacion { get; set; }
            public string tamañoBuffer { get; set; }
            public string status { get; set; }

        }

        public static ResultadoPing LocalPing(IPAddress address)
        {
            // Ping's a una maquina o dispositivo Local
            ResultadoPing res = new ResultadoPing();
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(address, 1000);

            if (reply.Status == IPStatus.Success)
            {
                res.conectado = true;
                res.direccion = reply.Address.ToString();
                res.roundTripTime = reply.RoundtripTime;
                res.tiempoVivir = reply.Options.Ttl.ToString();
                res.sinFragmentacion = reply.Options.DontFragment.ToString();
                res.tamañoBuffer = reply.Buffer.Length.ToString();
                res.status = reply.Status.ToString();
                return res;
            }
            else
            {
                res.conectado = false;
                res.direccion = address.ToString();
                res.roundTripTime = reply.RoundtripTime;
                res.tiempoVivir = null;
                res.sinFragmentacion = null;
                res.tamañoBuffer = null;
                res.status = reply.Status.ToString();
                return res;
            }
        }
    }
}
