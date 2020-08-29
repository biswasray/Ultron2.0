using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Management;
using System.Speech.Synthesis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
namespace Ultron2._0
{
    public partial class Service1 : ServiceBase
    {
        bool active = true;
        string debug = "smooth";
        SpeechSynthesizer speechSynthesizer;
        Thread thread,th;
        public Service1()
        {
            InitializeComponent();
            speechSynthesizer = new SpeechSynthesizer();
        }

        protected override void OnStart(string[] args)
        {
            speechSynthesizer.Speak("Start");
            //thread = new Thread(Fun);
            //thread.Start();
            th = new Thread(Capture);
            th.Start();
        }
        public static string GetIp()
        {
            string ip = "127.0.0.1";
            try
            {
                ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'");
                ManagementObjectCollection queryCollection = query.Get();
                foreach (ManagementObject mo in queryCollection)
                {

                    string[] defaultgateways = (string[])mo["DefaultIPGateway"];
                    string[] mys = new string[defaultgateways.Length];

                    int i = 0;
                    foreach (string defaultgateway in defaultgateways)
                    {
                        mys[i++] = defaultgateway;
                    }
                    ip = mys[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return (ip);
        }
        public string[] GetDefaultGateway()
        {
            /*return NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null).FirstOrDefault();*/
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled='TRUE'");
            ManagementObjectCollection col = query.Get();
            string[] st = null;
            foreach (ManagementObject mo in col)
            {
                st = (string[])mo["DefaultIPGateway"];

            }
            return st;
        }
        public void Capture()
        {
            try
            {
                //ScreenCapture screenCapture = new ScreenCapture();
                //Bitmap bit = new Bitmap(screenCapture.CaptureScreen());
                speechSynthesizer.Speak("In fun");
                SendKeys.SendWait("+{PRTSC}");
                SendKeys.SendWait("{PRTSC}");
                Bitmap bit = (Bitmap)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
                bit.Save(@"E:\Capture1.jpg", ImageFormat.Jpeg);
                speechSynthesizer.Speak("Capture");
            }
            catch (Exception ex)
            {
                //speechSynthesizer.Speak(ex.ToString());
                debug = ex.ToString();
            }
        }
        public void Fun()
        {
            speechSynthesizer.Speak("In fun");
            string ip = GetIp();
            IPAddress iPAddress = Dns.GetHostAddresses(ip)[0];
            IPEndPoint lo = new IPEndPoint(iPAddress, 1111);
            string preve = "";
            
            while (active)
            {
                Thread.Sleep(1000);

                try
                {
                    Socket sender = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(lo);
                    byte[] buffer = new byte[1024];
                    int s = sender.Receive(buffer);
                    char[] space = { '\n' };
                    string msg = Encoding.ASCII.GetString(buffer, 0, s).Split(space)[0];
                    if (!msg.Equals(preve))
                    {
                        speechSynthesizer.Speak("Hello " + msg);
                        preve = msg;
                        sender.Send(Encoding.ASCII.GetBytes(debug));
                    }
                    if(msg.Equals("exit")||msg.Contains("exit"))
                    {
                        active = false;
                        return;
                    }
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (Exception ex)
                {
                    speechSynthesizer.Speak(ex.ToString());
                    return;
                }
            }
            //sender.Shutdown(SocketShutdown.Both);
            //sender.Close();
        }

        protected override void OnStop()
        {
        }
    }
}
