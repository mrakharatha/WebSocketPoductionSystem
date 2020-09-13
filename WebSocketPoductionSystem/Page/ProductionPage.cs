using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using WebSocketPoductionSystem.Class;
using WebSocketPoductionSystem.ClassWebSocket;

namespace WebSocketPoductionSystem
{
    public partial class ProductionScales : Form
    {
        private WebSocketServer WebServer;
        public ProductionScales()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            ShowInTaskbar = false;
            WebServer = new WebSocketServer();
            int port = 8088;
            WebServer.Setup(port);
            WebServer.NewSessionConnected += WebServer_NewSessionConnected;
            WebServer.SessionClosed += WebServer_SessionClosed;
            WebServer.NewMessageReceived += WebServer_NewMessageReceived;
            WebServer.Start();
            notifyIcon.Visible = true;
        }
        private void WebServer_NewSessionConnected(WebSocketSession session)
        {
            MethodInvoker inv = delegate
            {
                lblStatus.Text = "SessionConnected";
                WriteLog.Write("SessionConnected");
            };
            this.Invoke(inv);
        }
        private void WebServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            MethodInvoker inv = delegate
            {
                lblStatus.Text = "SessionClosed";
                WriteLog.Write("SessionClosed");
            };
            this.Invoke(inv);
        }
        private void WebServer_NewMessageReceived(WebSocketSession session, string value)
        {
            dynamic Res = JsonConvert.DeserializeObject(value);
            string Command = Res.command.ToString();
            if (Command== "getscale")
            {
                Scales scale = new Scales();
                WriteLog.Write(value.ToString());
                BalanceClass balanceClass = JsonConvert.DeserializeObject<BalanceClass>(value);
                try
                {
                    if (balanceClass.data.port != null && balanceClass.data.transfer_rate != null && balanceClass.data.gateway != null)
                    {
                        scale.scalesInterface = (ScalesInterface)Enum.Parse(typeof(ScalesInterface), balanceClass.data.gateway.ToString());
                        if (scale.Connect(balanceClass.data.port, int.Parse(balanceClass.data.transfer_rate)))
                        {
                            var result = scale.Received();
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
                    scale.DisConnect();
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
