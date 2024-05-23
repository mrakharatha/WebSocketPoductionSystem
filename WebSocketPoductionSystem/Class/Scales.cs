using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

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
            return false;
        }
        public void DisConnect()
        {
            //از نوع سریال
            if (SerialPort != null)
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
                if (scalesInterface == ScalesInterface.Scales)
                {
                    while (true)
                    {
                        string[] stringSeparators = new string[] { "\r" };
                        Thread.Sleep(100);
                        string[] lines = SerialPort.ReadExisting().Split(stringSeparators, StringSplitOptions.None);

                        var positiveState = "=";
                        var negativeState = "-=";
                        var end = "(kg)";

                        foreach (var line in lines)
                        {

                            if (line.StartsWith(positiveState))
                            {
                                if (line.EndsWith(end))
                                {
                                    string result = RemoveData(line);
                                    if (result.StartsWith("."))
                                    {
                                        return "0" + result;
                                    }
                                    return result;
                                }
                            }

                            if (line.StartsWith(negativeState))
                            {
                                if (line.EndsWith(end))
                                {

                                    //حذف دیتای اضافی
                                    string result = RemoveData(line);
                                    if (result.StartsWith("."))
                                    {
                                        return "-0" + result;
                                    }
                                    return "-" + result;
                                }
                            }
                        }
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
                    string[] res = data.Split('$');
                    var result = res.FirstOrDefault() ?? "0";

                    result = result.Split('?').FirstOrDefault() ?? "0";

                    var response = result.Split('\r');

                    result = response.FirstOrDefault() ?? "0";

                    var r = result.Split('\0');

                    result = r.FirstOrDefault() ?? "0";

                    result = result.Replace(" ", "");

                    string removeZero = result.TrimStart(new char[] { '0' });

                    if (removeZero == "")
                        return "0";

                    else
                        return removeZero;

                }

                if (scalesInterface == ScalesInterface.Zarbaf)
                {

                    while (true)
                    {
                        string[] stringSeparators = new string[] { "\r" };
                        Thread.Sleep(100);
                        string[] lines = SerialPort.ReadExisting().Split(stringSeparators, StringSplitOptions.None);



                        foreach (var line in lines)
                        {
                            var result = line.Replace("p", "");
                            result = result.Replace("P", "");
                            result = result.Replace("@", "");
                            if (result.StartsWith("+"))
                            {
                                result = result.Replace("+", "");
                                result = result.TrimStart(new char[] { '0' });
                                if (result.StartsWith("."))
                                    return "0" + result;

                                return result;
                            }

                            if (result.StartsWith("-"))
                            {
                                result = result.Replace("-", "");
                                result = result.TrimStart(new char[] { '0' });
                                if (result.StartsWith("."))
                                    return "0" + result;

                                return "-" + result;
                            }
                        }
                    }


                }

                if (scalesInterface == ScalesInterface.YazdTaraz)
                {
                    while (true)
                    {
                        string[] stringSeparators = new string[] { "\r\n" };
                        Thread.Sleep(100);
                        string[] lines = SerialPort.ReadExisting().Split(stringSeparators, StringSplitOptions.None);

                        var positiveState = "ST,NT,+";
                        var negativeState = "ST,NT,-";
                        var end = "kg";

                        foreach (var line in lines)
                        {
                            if (line.StartsWith(positiveState) && line.EndsWith(end))
                            {
                                var result = line.Replace(positiveState, "").Replace(end, "").Trim();
                                DisConnect();

                                return result;

                            }
                            if (line.StartsWith(negativeState) && line.EndsWith(end))
                            {
                                var result = line.Replace(negativeState, "").Replace(end, "").Trim();
                                DisConnect();

                                return "-" + result;

                            }
                        }

                    }
                }


                if(scalesInterface== ScalesInterface.A12E)
                {
                    while (true)
                    {
                        string[] stringSeparators = new string[] { "\r" };
                        Thread.Sleep(100);
                        string[] lines = SerialPort.ReadExisting().Split(stringSeparators, StringSplitOptions.None);

                        var positiveState = "wn";
                        var negativeState = "wn-";
                        var end = "kg";

                        foreach (var line in lines)
                        {
                            if (line.Trim().StartsWith(negativeState))
                            {
                                if (line.EndsWith(end))
                                {

                                    //حذف دیتای اضافی
                                    string result = RemoveData(line);
                                    if (result.StartsWith("."))
                                    {
                                        return "-0" + result;
                                    }
                                    return "-" + result;
                                }
                            }

                            if (line.Trim().StartsWith(positiveState))
                            {
                                if (line.EndsWith(end))
                                {
                                    string result = RemoveData(line);
                                    if (result.StartsWith("."))
                                    {
                                        return "0" + result;
                                    }
                                    return result;
                                }
                            }

                           
                        }
                    }
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
            data = data.Replace('(', 'k');
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
        public string TcpServerReceived(string ip, string port)
        {
            TcpListener server = null;
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);

                server = new TcpListener(ipAddress,int.Parse( port));
                server.Start();

                Byte[] bytes = new Byte[256];

                while (true)
                {
                    using (TcpClient client = server.AcceptTcpClient())
                    {
                        NetworkStream stream = client.GetStream();

                        int i;
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var data = System.Text.Encoding.ASCII.GetString(bytes, 0, i).Trim();
                            server.Stop();
                            if (!data.EndsWith("-") && !data.EndsWith("_"))
                                return data;

                            data = data.Replace("-", "");
                            data = data.Replace("_", "");
                            data = "-" + data;

                            return data;
                        }
                    }
                }
            }
            catch
            {
                server?.Stop();
                return "0";
            }
        }


        public string UcpServerReceived(string ip, string port, int scaleNumber)
        {
            var udpServer = new System.Net.Sockets.UdpClient(int.Parse(port));
            var clientEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));

            try
            {

                while (true)
                {
                    // دریافت داده از کلاینت
                    var data = udpServer.Receive(ref clientEndPoint);
                    var result = Encoding.UTF8.GetString(data);

                    result = result.Replace("Scale,", "");
                    result = result.Replace("kg", "");
                    result = result.Replace("\r\n", "");

                    var split = result.Split(':');

                    if (split.Length > 1&& int.Parse(split[0]) == scaleNumber)
                    {
                        var getScale = split[1];

                        if (getScale.StartsWith("-"))
                        {

                            getScale = getScale.Replace("-", "");

                            getScale = getScale.TrimStart(new char[] { '0' });

                            if (string.IsNullOrEmpty(getScale))
                                return "-0";

                            return $"-{getScale}";
                        }
                        else
                        {
                            getScale = getScale.TrimStart(new char[] { '0' });

                            if (string.IsNullOrEmpty(getScale))
                                return "0";

                            return $"{getScale}";
                        }


                    }

                }
            }
            catch (Exception ex)
            {
                return "0";
            }
            finally
            {
                udpServer.Close();
            }
        }

    }
    public enum ScalesInterface
    {
        Udp,
        Tcp,
        Serial,
        Wifi,
        Weighbridge,
        Scales,
        Zarbaf,
        Mahak,
        YazdTaraz,
        A12E
    }
}
