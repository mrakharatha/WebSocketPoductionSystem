using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.Class
{
   public  class Porotocol
    {
        
            public string command { get; set; }
            public string[] data { get; set; }

        
    }

    public class PorotocolScale
    {
        public string serialPortName { get; set; }
        public  int serialBaudRate { get; set; }

        public ScalesInterface ScalesInterface { get; set; }
    }
}
