﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using SuperWebSocket;
using WebSocketPoductionSystem.Class;
using WebSocketPoductionSystem.ClassWebSocket;

namespace WebSocketPoductionSystem.Page
{
    public partial class ProductionScales : Form
    {
        private WebSocketServer _webServer;
        private bool _weighbridge = false;
        public ProductionScales()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //اطلاعات باسکول
            string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) + @"\Weighbridge.txt";

            if (File.Exists(path))
            {
                //وضعیت نرم افزار ترازو به باسکول
                _weighbridge = true;
                StreamReader reader = new StreamReader(path);
                BalanceClass balanceClass = JsonConvert.DeserializeObject<BalanceClass>(reader.ReadLine());

                Weighbridge.ScalesInterface = (ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.gateway.ToString());

                //متصل به ترازو
                Weighbridge.Connect(balanceClass.data.port, int.Parse(balanceClass.data.transfer_rate));
            }

            WriteLog.Write(_weighbridge ? "باسکول" : "ترازو");


            this.Hide();
            ShowInTaskbar = false;

            //تنظیمات وب سوکت
            _webServer = new WebSocketServer();
            int port = 8088;
            _webServer.Setup(port);
            _webServer.NewSessionConnected += WebServer_NewSessionConnected;
            _webServer.SessionClosed += WebServer_SessionClosed;
            _webServer.NewMessageReceived += WebServer_NewMessageReceived;
            _webServer.Start();
            notifyIcon.Visible = true;
        }
        private void WebServer_NewSessionConnected(WebSocketSession session)
        {
            //متصل به وب سوکت
            MethodInvoker inv = delegate
            {
                lblStatus.Text = "SessionConnected";
                WriteLog.Write("SessionConnected");
            };
            this.Invoke(inv);
        }
        private void WebServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            //بستن وب سوکت
            MethodInvoker inv = delegate
            {
                lblStatus.Text = "SessionClosed";
                WriteLog.Write("SessionClosed");
            };
            this.Invoke(inv);
        }
        private void WebServer_NewMessageReceived(WebSocketSession session, string value)
        {
            dynamic res = JsonConvert.DeserializeObject(value);
            string command = res.command.ToString();
            string protocol = res.data.protocol.ToString();
            WriteLog.Write($"Received : {value}");

            ////حالت باسکول
            //if (protocol== "Weighbridge")
            //{
            //    //خواندن اطلاعات
            //    string result = Weighbridge.ReadData;

            //    if (result==null)
            //    {
            //        session.Send("0");
            //        WriteLog.Write(result);
            //        MethodInvoker inv = delegate { listScales.Items.Add($"وزن: 0       {ConvertDate()} "); };
            //        this.Invoke(inv);
            //    }
            //    else
            //    {
            //        session.Send(result);
            //        WriteLog.Write(result);
            //        MethodInvoker inv = delegate { listScales.Items.Add($"وزن: {result}        {ConvertDate()} "); };
            //        this.Invoke(inv);
            //    }

            //}
            //else
            //{
            // dynamic res = JsonConvert.DeserializeObject(value);
            //دستور سیستم
            //string Command = res.command.ToString();
            //دریافت وزن ترازو
            if (command == "getscale")
            {

                Scales scale = new Scales();
                BalanceClass balanceClass = JsonConvert.DeserializeObject<BalanceClass>(value);




                try
                {
                    if ((ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.gateway) == ScalesInterface.Wifi)
                    {

                        if ((ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.protocol) == ScalesInterface.Udp)
                        {
                            var result = scale.UcpServerReceived(balanceClass.data.ip,balanceClass.data.port,balanceClass.data.scale_number);


                            session.Send(result);
                            MethodInvoker inv = delegate { listScales.Items.Add($"وزن: {result}        {ConvertDate()} "); };
                            this.Invoke(inv);
                        }
                        else if ((ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.protocol) == ScalesInterface.Tcp)
                        {
                            var result = scale.TcpServerReceived(balanceClass.data.ip, balanceClass.data.port);

                            session.Send(result);
                            MethodInvoker inv = delegate { listScales.Items.Add($"وزن: {result}        {ConvertDate()} "); };
                            this.Invoke(inv);
                        }
                    }
                    else
                    {
                        var serialPortName = balanceClass.data.port;
                        
                     var serialBaudRate=   int.Parse(balanceClass.data.transfer_rate);

                        //دریافت اطلاعات ترازو
                        if (balanceClass.data.port != null && balanceClass.data.transfer_rate != null && balanceClass.data.gateway != null)
                        {
                            var scalesInterface = (ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.protocol.ToString());

                            if (scalesInterface==ScalesInterface.Mahak)
                            {

                                var result = MahakReceivedData.ReceivedData(serialPortName, serialBaudRate);
                                session.Send(result);
                                MethodInvoker inv = delegate { listScales.Items.Add($"وزن: {result}        {ConvertDate()} "); };
                                this.Invoke(inv);
                            }
                            else
                            {
                                if (scale.Connect(serialPortName,serialBaudRate))
                                {

                                    var result = scale.Received(scalesInterface);

                                    session.Send(result);
                                    MethodInvoker inv = delegate { listScales.Items.Add($"وزن: {result}        {ConvertDate()} "); };
                                    this.Invoke(inv);
                                    scale.DisConnect();
                                }
                                else
                                {
                                    session.Send("0");
                                }
                            }

                            
                        }
                        else
                        {
                            session.Send("0");
                        }
                    }



                }
                catch (Exception e)
                {
                    WriteLog.Write("Step Error" + e.Message);

                    scale.DisConnect();
                }
            }
            //}

        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon.Visible = false;
            ShowInTaskbar = true;
            this.Show();
        }
        private void ProductionPage_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            notifyIcon.Visible = true;
            e.Cancel = true;
        }
        private void ProductionPage_SizeChanged(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon.Visible = true;
        }
        public string ConvertDate()
        {
            DateTime date = DateTime.Now;
            PersianCalendar persian = new PersianCalendar();
            return persian.GetYear(date) + "/" + persian.GetMonth(date).ToString("00") + "/" + persian.GetDayOfMonth(date).ToString("00") + "  " + DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
