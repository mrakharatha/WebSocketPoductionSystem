using SuperWebSocket;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using WebSocketPoductionSystem.Class;
using WebSocketPoductionSystem.ClassWebSocket;

namespace WebSocketPoductionSystem
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
                StreamReader reader=new StreamReader(path);
                BalanceClass balanceClass = JsonConvert.DeserializeObject<BalanceClass>(reader.ReadLine());

                Weighbridge.ScalesInterface = (ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.gateway.ToString());
             
                //متصل به ترازو
                Weighbridge.Connect(balanceClass.data.port, int.Parse(balanceClass.data.transfer_rate));
            }

            WriteLog.Write(_weighbridge?"باسکول":"ترازو");


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

            WriteLog.Write("Step 1");

            //حالت باسکول
            if (_weighbridge)
            {
                //خواندن اطلاعات
                string result = Weighbridge.ReadData;
                if (result==null)
                {
                    session.Send("0");
                    WriteLog.Write(result);
                    MethodInvoker inv = delegate { listScales.Items.Add($"وزن: 0       {ConvertDate()} "); };
                    this.Invoke(inv);
                }
                else
                {
                    session.Send(result);
                    WriteLog.Write(result);
                    MethodInvoker inv = delegate { listScales.Items.Add($"وزن: {result}        {ConvertDate()} "); };
                    this.Invoke(inv);
                }
           
            }
            else
            {
                WriteLog.Write("Step 2");
                dynamic res = JsonConvert.DeserializeObject(value);
                //دستور سیستم
                string Command = res.command.ToString();
                //دریافت وزن ترازو
                if (Command == "getscale")
                {
                    WriteLog.Write("Step 3");

                    Scales scale = new Scales();
                    WriteLog.Write(value.ToString());
                    BalanceClass balanceClass = JsonConvert.DeserializeObject<BalanceClass>(value);

                    WriteLog.Write("balance" + balanceClass.data.port);


                    try
                    {
                        WriteLog.Write("Step 4");

                        //دریافت اطلاعات ترازو
                        if (balanceClass.data.port != null && balanceClass.data.transfer_rate != null && balanceClass.data.gateway != null)
                        {
                            WriteLog.Write("Step 5");

                            scale.ScalesInterface = (ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.gateway.ToString());
                            if (scale.Connect(balanceClass.data.port, int.Parse(balanceClass.data.transfer_rate)))
                            {
                                WriteLog.Write("Step 6");

                                var result = scale.Received((ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.protocol.ToString()));

                                session.Send(result);
                                MethodInvoker inv = delegate { listScales.Items.Add($"وزن: {result}        {ConvertDate()} "); };
                                this.Invoke(inv);
                                WriteLog.Write(result);
                                scale.DisConnect();
                            }
                            else
                            {
                                session.Send("0");
                            }
                        }
                        else
                        {
                            session.Send("0");
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLog.Write("Step Error"+ e.Message);

                        scale.DisConnect();
                    }
                }
            }
           
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
