using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.Class
{
    public class Weighbridge
    {
        public static ScalesInterface ScalesInterface;
        private static SerialPort _serialPort;
        public static string ReadData;

        //متصل به ترازو
        public static void Connect(string serialPortName, int serialBaudRate)
        {
            // از نوع سریال
           
                DisConnect();
                try
                {
                    if (_serialPort == null || _serialPort.IsOpen == false)
                    {
                        _serialPort = new SerialPort(serialPortName, serialBaudRate);
                        _serialPort.Open();
                        _serialPort.DataReceived += SerialPort_DataReceived;
                    }
                }
                catch (Exception ex)
                {
                    StreamWriter sw = new StreamWriter("log.txt", true);
                    sw.WriteLine(ex.Message);
                    sw.Close();
                
            }
        }

        //دریافت اطلاعات
        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            ReadData = _serialPort.ReadExisting();
            string[] res = ReadData.Split(' ');
            res = res.Last().Split('\r');
            ReadData = res.First();
        }

        public static void DisConnect()
        {
            //قطع شدن
            if ( _serialPort != null)
            {
                try
                {
                    _serialPort.RtsEnable = false;
                    _serialPort.DtrEnable = false;
                    _serialPort.ReadExisting();
                    _serialPort.Close();
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
