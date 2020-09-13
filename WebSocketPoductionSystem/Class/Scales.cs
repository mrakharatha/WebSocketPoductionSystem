using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.Class
{
    public class Scales
    {
        public ScalesInterface scalesInterface;
        public SerialPort serialPort;
        public bool Connect(string serialPortName, int serialBaudRate)
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
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    StreamWriter sw = new StreamWriter("log.txt", true);
                    sw.WriteLine(ex.Message);
                    sw.Close();
                    return false;
                }
            }
            return false;
        }
        public void DisConnect()
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
        public string Received()
        {
            try
            {
                string data = serialPort.ReadLine();
                if (data.Contains("-"))
                {
                    string result = RemoveData(data);
                    if (result.StartsWith("."))
                    {
                        return "-0" + result;
                    }
                    return "-"+result;
                }
                else
                {
                    string result =RemoveData(data);
                    if (result.StartsWith("."))
                    {
                        return "0" + result;
                    }
                    return result;
                }
            }
            catch
            {
                return "0";
            }
        }
        public string RemoveData(string data)
        {
            if (data.Contains("-"))
            {
                data = data.Replace('-', '0');
            }
            string removeW = data.Replace('w', '0');
            string removeN = removeW.Replace('n', '0');
            string removeZero = removeN.TrimStart(new char[] { '0' });
            var send = removeZero.Split('k');
            return send[0];
        }
    }
    public enum ScalesInterface
    {
        Wifi = 1,
        Ethernet,
        Serial
    }
}
