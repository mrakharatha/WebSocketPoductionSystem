using System;
using System.Globalization;
using NewDLL;

namespace WebSocketPoductionSystem.Class
{
    public static class MahakReceivedData
    {
        public static NewMahakScale MahakScale { get; set; }

        public static string ReceivedData(string serialPortName, int baudRate)
        {

            var portComValue = int.Parse(serialPortName.Split('M')[1]);

            MahakScale = new NewMahakScale(false, string.Empty, 0, serialPortName, baudRate, false, false, true, false);

            //بررسی فعال بودن ترازوی دریافت شده
            if (MahakScale.mahakScales[portComValue] != null && MahakScale.mahakScales[portComValue].isActive)
            {

                var getWeightScale = GetWeight(portComValue, MahakScale);
                var result = getWeightScale.GetResult;
                Disconnect();

                return result.Success ? result.Message : "0";
            }
            else
            {
                return "0";
            }
        }



        public static GetWeightResponseViewModel GetWeight(int hostId, NewMahakScale mahakScale)
        {
            var result = new GetWeightResponseViewModel();
            int tryRunSuccess = 0;
            try
            {
                //بررسی فعال بودن ترازو
                result.GetResult = IsActiveScale(hostId, mahakScale);
                if (result.GetResult.Success)
                {
                    var getMahakScale = mahakScale.mahakScales[hostId];
                    result.GetResult.Success = false;

                    //ارسال دستور به ترازو
                    while (!result.GetResult.Success && tryRunSuccess < 3)
                    {
                        if (getMahakScale.isNewProtocol)
                            result.GetResult.Success = getMahakScale._GetWeight(); //ترازوهای پلاس
                        else
                            result.GetResult.Success = getMahakScale.ReadWeight(); //ترازوهای معمولی
                        tryRunSuccess++;
                    }

                    //بررسی نتیجه جواب ترازو
                    result.GetResult = AnswerProcess(hostId, "دریافت وزن", mahakScale);
                    if (result.GetResult.Success)
                    {
                        var getScaleWeight = getMahakScale.Answer[0].ToString();

                        //وزن ترازو را بر حسب واحد (گرم/کیلوگرم) محاسبه می نماییم 
                        var getCalculateWeight = ConvertGoodWeightValue(getScaleWeight, true);
                        result.ScaleWeight = getCalculateWeight.ScaleWeight;
                        result.WeightDouble = getCalculateWeight.WeightDouble;
                        result.WeightUnit = getCalculateWeight.WeightUnit;

                        //مقدار نهایی وزن را نیز در این متغیر ذخیره می کنیم
                        result.GetResult.Message = getCalculateWeight.GetFinalWeightShow;
                    }
                }
            }
            catch (Exception ex)
            {
                result.GetResult = AnswerProcess(hostId, "دریافت وزن", mahakScale, ex);
            }
            return result;
        }

        private static BaseResponseViewModel AnswerProcess(int hostId, string messageErrorInstruction, NewMahakScale mahakScale, Exception exception = null)
        {
            var result = new BaseResponseViewModel() { Message = string.Empty, Success = true };
            var getScaleHost = mahakScale.mahakScales[hostId];

            if (getScaleHost.SAnswered)
            {
                result.ExMessage = getScaleHost.ErrComment;
                if (getScaleHost.ErrComment.Contains("مشغول"))
                {
                    result.Message = "ترازو مشغول می باشد";
                    result.Success = false;
                }
                else if (getScaleHost.ErrComment.Contains("اطلاعات ورودی در سیستم تعریف نشده است"))
                {
                    result.Message = "خطا در " + messageErrorInstruction + "";
                    result.Success = false;
                }
                else if (exception != null)
                {
                    if (string.IsNullOrEmpty(getScaleHost.ErrComment) || string.IsNullOrWhiteSpace(getScaleHost.ErrComment))
                    {
                        result.ExMessage = exception.Message;
                        result.Message = "خطا در اجرای " + messageErrorInstruction + "";
                    }
                    else
                        result.Message = getScaleHost.ErrComment + " . اما در انجام عملیات خطایی رخ داده است";
                    result.ExError = exception;
                    result.Success = false;
                }
                else
                {
                    result.Message = messageErrorInstruction + " با موفقیت اجرا شد";
                    result.Success = true;
                }
            }
            else
            {
                result.Success = false;
                result.ExMessage = getScaleHost.ErrComment;
                if (result.ExMessage.Contains("WaitForResponse4"))
                    result.Message = "اطلاعاتی  از ترازو دریافت نشد";
                else if (result.ExMessage.Contains("مشغول"))
                    result.Message = "ترازو مشغول می باشد";
                else
                    result.Message = "خطایی در هنگام ارسال دستور به ترازو رخ داده است";
            }
            return result;
        }

        private static BaseResponseViewModel IsActiveScale(int hostId, NewMahakScale mahakScale)
        {
            var result = new BaseResponseViewModel();
            try
            {
                if (mahakScale != null && mahakScale.mahakScales[hostId] != null)
                {
                    var IsActive = mahakScale.mahakScales[hostId].isActive;
                    if (IsActive)
                    {
                        var IsBusy = mahakScale.mahakScales[hostId].OCXBusy;
                        if (!IsBusy)
                            result.Success = true;
                        else
                        {
                            result.Success = false;
                            result.Message = "در حال حاضر ترازو مشغول می باشد";
                        }

                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "در حال حاضر ارتباط با ترازو قطع می باشد";
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = "در حال حاضر ترازوی مورد نظر در سیستم وجود ندارد";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "خطا در تشخیص اتصال ترازو";
            }
            return result;
        }

        public static GetWeightResponseViewModel ConvertGoodWeightValue(string weight, bool handleGr = false)
        {
            var result = new GetWeightResponseViewModel();
            result.ScaleWeight = int.Parse(weight);

            if (result.ScaleWeight == 0)
            {
                result.WeightDouble = 0;
                return result;
            }

            result.WeightDouble = Convert.ToDouble(weight) / 1000;
            var getWeightLength = weight.Length;
            if (getWeightLength < 4)
            {
                result.WeightUnit = "گرم";
                return result;
            }
            else
            {
                if (handleGr)
                {
                    if (Convert.ToDouble(weight) % 1000 == 0)
                    {
                        result.WeightUnit = "گرم";
                        return result;
                    }
                }

                result.WeightUnit = " کیلو";
                return result;
            }
        }




        private static void Disconnect()
        {
            try
            {
                if (MahakScale == null) return;
                foreach (var item in MahakScale.mahakScales)
                {
                    if (item == null) continue;
                    item.Dispose();
                    item.CloseConnection();
                }
                MahakScale.Dispose();
                MahakScale = null;
            }
            catch (Exception)
            {
                // ignored
            }
        }

    }



    public class BaseResponseViewModel
    {
        public BaseResponseViewModel()
        {
            Message = string.Empty;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
        public string ExMessage { get; set; }
        public Exception ExError { get; set; }
    }

    public class GetWeightResponseViewModel
    {
        public BaseResponseViewModel GetResult { get; set; }
        public GetWeightResponseViewModel()
        {
            GetResult = new BaseResponseViewModel();
        }

        //وزنی که ترازو ارسال می کند
        public int ScaleWeight { get; set; }

        //وزن ارسالی ترازو را تقسیم بر 1000 می کنیم تا مقدار واقعی وزن ترازو به دست آید که این مقدار در صفحه نمایش ترازو نیز قابل مشاهده است
        //در محاسبات خود همواره این متغیر را در نظر داشته باشید
        public double WeightDouble { get; set; }

        //آیا وزن ارسالی ترازو بر حسب گرم است یا کیلوگرم ؟
        public string WeightUnit { get; set; }

        //public string GetFinalWeightShow => WeightDouble.ToString(CultureInfo.InvariantCulture) + " " + WeightUnit;
        public string GetFinalWeightShow => WeightDouble.ToString(CultureInfo.InvariantCulture);
    }

}