using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.ClassWebSocket
{
    class BalanceClass
    {
        //دستور
        public string command { get; set; }
        //اطلاعات
        public  data data { get; set; }
    }

    public class data
    {
        //آیدی
        public int id { get; set; }
        //عنوان
        public string title { get; set; }
        //مدل
        public string model { get; set; }
        //پروتکل 
        public string protocol { get; set; }
        //دروازه
        public string gateway { get; set; }
        //آی پی
        public string ip { get; set; }
        //پورت
        public string port { get; set; }
        //نرخ انتقال
        public string transfer_rate { get; set; }
        //فعال
        public int active { get; set; }
        //تاریخ ثبت
        public string created_at { get; set; }
        //تاریخ ویرایش
        public string updated_at { get; set; }
        //تاریخ حذف
        public string deleted_at { get; set; }
    }
}
