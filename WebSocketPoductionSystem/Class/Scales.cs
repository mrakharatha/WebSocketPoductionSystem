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
                    //// خواندن اطلاعات
                    //string data = SerialPort.ReadLine();


                    ////منفی بودن مقدار ترازو
                    //if (data.Contains("-"))
                    //{
                    //    //حذف دیتای اضافی
                    //    string result = RemoveData(data);
                    //    if (result.StartsWith("."))
                    //    {
                    //        return "-0" + result;
                    //    }
                    //    return "-" + result;
                    //}
                    //else
                    //{
                    //    //حذف دیتای اضافی
                    //    string result = RemoveData(data);
                    //    if (result.StartsWith("."))
                    //    {
                    //        return "0" + result;
                    //    }
                    //    return result;
                    //}



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


                            //var result = line.Replace("p", "");
                            //result = result.Replace("P", "");
                            //result = result.Replace("@", "");
                            //if (result.StartsWith("+"))
                            //{
                            //    result = result.Replace("+", "");
                            //    result = result.TrimStart(new char[] { '0' });
                            //    if (result.StartsWith("."))
                            //        return "0" + result;

                            //    return result;
                            //}

                            //if (result.StartsWith("-"))
                            //{
                            //    result = result.Replace("-", "");
                            //    result = result.TrimStart(new char[] { '0' });
                            //    if (result.StartsWith("."))
                            //        return "0" + result;

                            //    return "-" + result;
                            //}
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
    }
    public enum ScalesInterface
    {
        Weighbridge,
        Scales,
        Zarbaf,
        YazdTaraz
    }
}
