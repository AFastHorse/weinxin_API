using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace WeiXinLearn.CommonHelper
{
    public class Access_TokenDAL
    {
        private static string appid = "wx86ec4477de7d1e6f";
        private static string secret = "5280700ce37857f91b3e2f729e1c01d5";
        private static Access_TokenModel Access_Token;
        private static DateTime GetTime;
        public Access_TokenDAL() { }
        public static Access_TokenModel GetAccessToken()
        {
            if (Access_Token == null || (Access_Token != null && (DateTime.Now - GetTime).Minutes > 7200))
            {
                string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + secret;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "Get";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                string str = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                Access_Token = JsonConvert.DeserializeObject<Access_TokenModel>(str);
                GetTime = DateTime.Now;
            }
            return Access_Token;
        }
    }
    public class Access_TokenModel
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
}