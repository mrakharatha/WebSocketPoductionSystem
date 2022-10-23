using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.Class
{
    public class Scales
    {
        public ScalesInterface ScalesInterface;
        public SerialPort SerialPort;
        //متصل به ترازو
        public bool Connect(string serialPortName, int serialBaudRate)
        {
            // از نوع سریال
            if (ScalesInterface == ScalesInterface.Serial)
            {
                DisConnect();
                try
                {
                    if (SerialPort == null || SerialPort.IsOpen == false)
                    {
                        SerialPort = new SerialPort(serialPortName, serialBaudRate);
                        SerialPort.Open();
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
            //از نوع سریال
            if (ScalesInterface == ScalesInterface.Serial && SerialPort != null)
            {
                try
                {
                    SerialPort.RtsEnable = false;
                    SerialPort.DtrEnable = false;
                    SerialPort.ReadExisting();
                    SerialPort.Close();
                }
                catch (Exception ex)
                {
                    StreamWriter sw = new StreamWriter("log.txt", true);
                    sw.WriteLine(ex.Message);
                    sw.Close();
                }
            }
        }
        public string Received(ScalesInterface scalesInterface)
        {
            try
            {

                //از نوع ترازو
                if (scalesInterface==ScalesInterface.Scales)
                {
                    // خواندن اطلاعات
                    string data = SerialPort.ReadLine();

                    
                    //منفی بودن مقدار ترازو
                    if (data.Contains("-"))
                    {
                        //حذف دیتای اضافی
                        string result = RemoveData(data);
                        if (result.StartsWith("."))
                        {
                            return "-0" + result;
                        }
                        return "-" + result;
                    }
                    else
                    {
                        //حذف دیتای اضافی
                        string result = RemoveData(data);
                        if (result.StartsWith("."))
                        {
                            return "0" + result;
                        }
                        return result;
                    }
                }

                //ازنوع باسکول
                if (scalesInterface == ScalesInterface.Weighbridge)
                {
                    //تاخیر 1 ثانیه ای
                    Thread.Sleep(1000);
                    // خواندن اطلاعات
                    var data = SerialPort.ReadExisting();
                    //شکاف بر اساس ؟
                    string[] res = data.Split('?');
                    var result = res.Last();
                    string removeZero = result.TrimStart(new char[] { '0' });
                    if (removeZero=="")
                        return "0";
                    
                    else
                        return removeZero;
                    
                }

                return "0";

            }
            catch
            {
                return "0";
            }
        }
        public string RemoveData(string data)
        {
            data = data.Replace('(','k');
            //حذف دیتای اضافی 
            if (data.Contains("-"))
            {
                data = data.Replace('-', '0');
            }
            string removeEqual = data.Replace('=', '0');
            string removeW = removeEqual.Replace('w', '0');
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
        Serial,
        Weighbridge,
        Scales
    }
}
