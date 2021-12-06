using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.Class
{
    public class Porotocol
    {
        //دستورات
        public string command { get; set; }
        //اطلاعات
        public string[] data { get; set; }
    }

    public class PorotocolScale
    {
        //نام پورت سریال
        public string serialPortName { get; set; }
        //سریال Baud Rate
        public int serialBaudRate { get; set; }
        //نوع ترازو
        public ScalesInterface ScalesInterface { get; set; }
    }
}
