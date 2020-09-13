using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.Class
{
  static  class Weighbridge
    {
        public static ScalesInterface scalesInterface;
        private static SerialPort serialPort;
        public static string readData;
        public static void Connect(string serialPortName, int serialBaudRate)
        {
            if (scalesInterface == ScalesInterface.Serial)
            {
                DisConnect();
                try
                {
                    if (serialPort == null || serialPort.IsOpen == false)
                    {
                        serialPort = new SerialPort(serialPortName, serialBaudRate);
                        serialPort.Open();
                        serialPort.DataReceived += SerialPort_DataReceived;
                    }
                }
                catch (Exception ex)
                {
                    StreamWriter sw = new StreamWriter("log.txt", true);
                    sw.WriteLine(ex.Message);
                    sw.Close();
                }
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            readData = serialPort.ReadLine();
        }

        public static void DisConnect()
        {
            if (scalesInterface == ScalesInterface.Serial && serialPort != null)
            {
                try
                {
                    serialPort.RtsEnable = false;
                    serialPort.DtrEnable = false;
                    serialPort.ReadExisting();
                    serialPort.Close();
                }
                catch (Exception ex)
                {
                    StreamWriter sw = new StreamWriter("log.txt", true);
                    sw.WriteLine(ex.Message);
                    sw.Close();
                }
            }
        }
    }
}
