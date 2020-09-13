using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.ClassWebSocket
{
    class BalanceClass
    {
        public string command { get; set; }

        public  data data { get; set; }
    }

    public class data
    {
        public int id { get; set; }
        public string title { get; set; }
        public string model { get; set; }
        public string protocol { get; set; }
        public string gateway { get; set; }
        public string ip { get; set; }
        public string port { get; set; }
        public string transfer_rate { get; set; }
        public int active { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string deleted_at { get; set; }
    }
}
