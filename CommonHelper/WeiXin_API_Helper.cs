using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using WeiXinLearn.Models;

namespace WeiXinLearn.CommonHelper
{
    public class WeiXin_API_Helper
    {
        public static QrCode InitQrCode(HttpContext context)
        {
            int expire_seconds;
            if (context.Request["expire_seconds"] != null)
            {
                if (!int.TryParse(context.Request["expire_seconds"].ToString(), out expire_seconds))
                {
                    expire_seconds = 604800;
                }
            }
            else
            {
                expire_seconds = 604800;
            }

            string action_name = "QR_SCENE";
            if (context.Request["action_name"] != null)
            {
                action_name = context.Request["action_name"].ToString();
            }
            Scene scene = new Scene { scene_str = "888" };
            Action_Info action_info = new Action_Info { scene = scene };
            QrCode model = new QrCode { expire_seconds = expire_seconds, action_name = action_name, action_info = action_info };
            return model;
        }
        public static string GetQrCode(HttpContext context, QrCode model)
        {
            string url = "https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token=";
            string postDataStr = JsonConvert.SerializeObject(model);           
            return PostRequest(context, url, postDataStr);
        }

        public static string GenerateQrCode(HttpContext context)
        {
            string permanentOrTemp = "media/upload";
            if (context.Request["permanentOrTemp"] != null)
            {
                permanentOrTemp = context.Request["permanentOrTemp"].ToString();
            }
            string result = GetQrCode(context, WeiXin_API_Helper.InitQrCode(context));
            QrCodeResPonse model = JsonConvert.DeserializeObject<QrCodeResPonse>(result);
            Bitmap bitMap = GetQrCodeBitMap(model.url, 1, 7);
            byte[] bArr = BitmapByte(bitMap);
            return Post_Media(context, bArr, "code.jpg", permanentOrTemp);
        }

        public static string GenerateQrCode(HttpContext context, string userOpenId)
        {
            int expire_seconds = 10800;
            string action_name = "QR_LIMIT_STR_SCENE";

            Scene scene = new Scene { scene_str = userOpenId };
            Action_Info action_info = new Action_Info { scene = scene };
            QrCode qrCode = new QrCode { expire_seconds = expire_seconds, action_name = action_name, action_info = action_info };

            string result = GetQrCode(context, qrCode);
            QrCodeResPonse model = JsonConvert.DeserializeObject<QrCodeResPonse>(result);
            Bitmap bitMap = GetQrCodeBitMap(model.url, 1, 7);
            byte[] bArr = BitmapByte(bitMap);
            return Post_Media(context, bArr, DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg", "media/upload");
        }

        public static Bitmap GetQrCodeBitMap(string content, int version, int pixel)
        {
            QRCoder.QRCodeGenerator coder = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeData codeData = coder.CreateQrCode(content, QRCoder.QRCodeGenerator.ECCLevel.M, true, true);
            QRCoder.QRCode code = new QRCoder.QRCode(codeData);
            Bitmap bitMap = code.GetGraphic(pixel, Color.Black, Color.White, true);
            return bitMap;
        }

        public static byte[] BitmapByte(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                return data;
            }
        }

        public static string SaveFile(HttpContext context)
        {
            var file = context.Request.Files[0];
            string newFile = DateTime.Now.ToString("yyyyMMddHHmmss");
            string path = context.Server.MapPath("/images/" + file.FileName);
            file.SaveAs(path);
            return path;
        }

        public static string PostMedia(HttpContext context)
        {
            string permanetOrTemp = "media/upload";
            if (context.Request.Params["permanetOrTemp"] != null)
            {
                permanetOrTemp = context.Request.Params["permanetOrTemp"].ToString();
            }
            string path = SaveFile(context);
            int pos = path.LastIndexOf("\\");
            string fileName = path.Substring(pos + 1);
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] bArr = new byte[fs.Length];
            fs.Read(bArr, 0, bArr.Length);
            fs.Close();
            return Post_Media(context, bArr, fileName, permanetOrTemp);

        }

        public static string Post_Media(HttpContext context, byte[] bArr, string fileName, string permanetOrTemp)
        {
            Access_TokenModel model = Access_TokenDAL.GetAccessToken();
            string url = "https://api.weixin.qq.com/cgi-bin/" + permanetOrTemp + "?type=image&access_token=" + model.access_token;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "Post";
            string boundary = DateTime.Now.Ticks.ToString("-");
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;

            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            //请求头部信息
            StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"{1}\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName, permanetOrTemp.Contains("add_material") ? "media" : "file"));
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());

            Stream postStream = request.GetRequestStream();
            postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            postStream.Write(bArr, 0, bArr.Length);
            postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            postStream.Close();

            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            //返回结果网页（html）代码
            string content = sr.ReadToEnd();
            return content;
        }

        public static string Get_TempMedia(HttpContext context)
        {
            string url = "https://api.weixin.qq.com/cgi-bin/media/get?media_id=pe0YPmkPwa4NWr0H1Q7Jqqt3e7BPb8jl0MEbbGL30VpPlgiqNJva0K4YtovJiWi5&access_token=";
            return GetRequest(context, url);
        }

        public static string CreateMenu(HttpContext context)
        {
            string url = "https://api.weixin.qq.com/cgi-bin/menu/create?access_token=";
            List<MButton> list = new List<MButton>();

            List<MButton> child1 = new List<MButton>();
            child1.Add(new MButton { type = "view", name = "登录学习", url = "http://m.kjjl100.com/586/" });
            child1.Add(new MButton { type = "view", name = "免费直播", url = "http://m.kjjl100.com/lives/k586/" });
            child1.Add(new MButton { type = "view", name = "免费题库", url = "http://t.kjjl100.com/" });
            child1.Add(new MButton { type = "media_id", name = "系统使用指南", media_id = "fshB0k541FCSJfq1O2Mevaet4M7mA_VzjZJsRFFjPGrrYdIE1_7vnN2E1PNE1XhK" });
            child1.Add(new MButton { type = "media_id", name = "在线答疑", media_id = "fshB0k541FCSJfq1O2Mevaet4M7mA_VzjZJsRFFjPGrrYdIE1_7vnN2E1PNE1XhK" });
            list.Add(new MButton { name = "学习中心", sub_button = child1 });

            list.Add(new MButton { name = "选课中心", sub_button = child1 });
            list.Add(new MButton { name = "福利中心", sub_button = child1 });
            string postDataStr = JsonConvert.SerializeObject(new { button = list });

            return PostRequest(context, url, postDataStr);
        }

        public static string Get_CheckResult(HttpContext context)
        {
            string url = "https://api.weixin.qq.com/cgi-bin/callback/check?access_token=";
            string postDataStr = JsonConvert.SerializeObject(new
            {
                action = "all",
                check_operator = "DEFAULT"
            });
            return PostRequest(context, url, postDataStr);
        }

        public static string Get_IPList(HttpContext context)
        {
            string url = "https://api.weixin.qq.com/cgi-bin/getcallbackip?access_token=";
            return GetRequest(context, url);
        }

        public static string PostRequest(HttpContext context, string url, string data)
        {
            Access_TokenModel model = Access_TokenDAL.GetAccessToken();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + model.access_token);
            request.Method = "POST";

            Stream postStream = request.GetRequestStream();
            byte[] postDataByte = Encoding.UTF8.GetBytes(data);
            postStream.Write(postDataByte, 0, postDataByte.Length);
            postStream.Dispose();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
            string str = reader.ReadToEnd();
            return str;
        }

        public static string GetRequest(HttpContext context, string url)
        {
            Access_TokenModel model = Access_TokenDAL.GetAccessToken();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + model.access_token);
            request.Method = "Get";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
            string str = reader.ReadToEnd();
            return str;
        }

    }
}