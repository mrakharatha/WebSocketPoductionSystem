using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketPoductionSystem.Class
{
	public static class WriteLog
	{
		// ثبت لاگ پیامک
		public static void Write(string log)
		{
            if (log == null)
				return;

			if (!Directory.Exists("log"))
				Directory.CreateDirectory("log");

			if (!File.Exists("log//log.txt"))
				File.Create("log//log.txt");
			

			try
			{
				StreamWriter sw = null;
				DateTime date = DateTime.Now;
				PersianCalendar persian = new PersianCalendar();
				sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"//log//log.txt", true);
				sw.WriteLine(persian.GetYear(date) + "/" + persian.GetMonth(date).ToString("00") + "/" + persian.GetDayOfMonth(date).ToString("00") + "  " + DateTime.Now.ToString("HH:mm:ss") + "  Log:" + log.ToString());
				sw.Flush();
				sw.Close();
			}
			catch
			{
				// ignored
			}
		}
	}
}