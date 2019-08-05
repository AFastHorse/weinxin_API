using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml;
using WeiXinLearn.BLL;
using WeiXinLearn.CommonHelper;
using WeiXinLearn.Models;

namespace WeiXinLearn.handle
{
    /// <summary>
    /// WeiXinHandler 的摘要说明
    /// </summary>
    public class WeiXinHandler : IHttpHandler
    {
        public readonly string Welcome = @"终于等到你，亲！
                                            ---感谢关注会计教练！
                                            会计教练-用最短的时间、最少的精力、最低的成本提升会计技能！
                                            无论您是想学习会计实操或者考证，还是想领取免费资料
                                            直接在公众号对话框回复消息即可！老师会在线给您回复！
                                            【温馨提示】
                                            老师上班时间为周一到周六上午8:30到12点，下午2点到6点30分
                                            其他时间回复，老师上班时间看到会第一时间回复大家，谢谢理解
                                            如有公众号资料发放活动，您根据提示将自己的专属海报分享出去，邀请好友助力，完成任务之后，即可领取超实用会计大奖资料哦，大家快快积极踊跃参与";
        public void ProcessRequest(HttpContext context)
        {
            string httpMethod = context.Request.HttpMethod.ToLower();
            context.Response.ContentType = "text/plain";
            string result = string.Empty;

            if (httpMethod == "post")
            {
                StreamReader str = new StreamReader(context.Request.InputStream, System.Text.Encoding.UTF8);
                WeiXinModel model = null;
                if (str != null)
                {
                    model = GetModel(str);
                    LogModel(model, context);
                    result = GetResult(model, context);
                }

                context.Response.Write(result);
                context.Response.End();
            }
            else
            {
                Valid(context);
            }
        }

        private string GetResult(WeiXinModel model, HttpContext context)
        {
            string result = string.Empty;
            switch (model.MsgType)
            {
                case "text":
                    result = GetText(model, context);
                    break;
                case "image":
                    result = GetImage(model);
                    break;
                case "event":
                    result = OperationEvent(model, context);
                    break;
                default: break;
            }
            return result;
        }
        private void LogModel(WeiXinModel model, HttpContext context)
        {
            using (var stream = new FileStream(context.Server.MapPath("textRequest.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine("ToUserName:" + model.ToUserName);
                writer.WriteLine("FromUserName:" + model.FromUserName);
                writer.WriteLine("CreateTime:" + model.CreateTime);
                writer.WriteLine("MsgType:" + model.MsgType);
                writer.WriteLine("Content:" + model.Content);
                writer.WriteLine("MsgId:" + model.MsgId);
                writer.WriteLine("PicUrl:" + model.PicUrl);
                writer.WriteLine("MediaId:" + model.MediaId);
                writer.WriteLine("Event:" + model.Event);
                writer.WriteLine("EventKey:" + model.EventKey);
                writer.WriteLine("Ticket:" + model.Ticket);
                writer.Close();
            }
        }
        private WeiXinModel GetModel(StreamReader str)
        {
            WeiXinModel model = new WeiXinModel();
            XmlDocument doc = new XmlDocument();
            doc.Load(str);

            model.ToUserName = doc.SelectSingleNode("xml").SelectSingleNode("ToUserName").InnerText;
            model.FromUserName = doc.SelectSingleNode("xml").SelectSingleNode("FromUserName").InnerText;
            model.CreateTime = doc.SelectSingleNode("xml").SelectSingleNode("CreateTime").InnerText;
            model.MsgType = doc.SelectSingleNode("xml").SelectSingleNode("MsgType").InnerText;
            model.Content = doc.SelectSingleNode("xml").SelectSingleNode("Content") == null ? "" : doc.SelectSingleNode("xml").SelectSingleNode("Content").InnerText;
            model.MsgId = doc.SelectSingleNode("xml").SelectSingleNode("MsgId") == null ? "" : doc.SelectSingleNode("xml").SelectSingleNode("MsgId").InnerText;
            model.PicUrl = doc.SelectSingleNode("xml").SelectSingleNode("PicUrl") == null ? "" : doc.SelectSingleNode("xml").SelectSingleNode("PicUrl").InnerText;
            model.MediaId = doc.SelectSingleNode("xml").SelectSingleNode("MediaId") == null ? "" : doc.SelectSingleNode("xml").SelectSingleNode("MediaId").InnerText;
            model.Event = doc.SelectSingleNode("xml").SelectSingleNode("Event") == null ? "" : doc.SelectSingleNode("xml").SelectSingleNode("Event").InnerText;
            model.EventKey = doc.SelectSingleNode("xml").SelectSingleNode("EventKey") == null ? "" : doc.SelectSingleNode("xml").SelectSingleNode("EventKey").InnerText;
            model.Ticket = doc.SelectSingleNode("xml").SelectSingleNode("Ticket") == null ? "" : doc.SelectSingleNode("xml").SelectSingleNode("Ticket").InnerText;

            return model;
        }

        private string OperationEvent(WeiXinModel model, HttpContext context)
        {
            switch (model.Event)
            {
                case "subscribe"://初次关注
                    HandleUser(model, context, true);
                    return GetText(model, context);
                case "SCAN"://已经关注
                    return GetText(model, context);
                default:
                    return GetText(model, context);
            }

        }
        public string GetImage(WeiXinModel model)
        {
            return string.Format(@"<xml>
                                        <ToUserName><![CDATA[{0}]]></ToUserName>
                                        <FromUserName><![CDATA[{1}]]></FromUserName>
                                        <CreateTime>{2}</CreateTime>
                                        <MsgType><![CDATA[image]]></MsgType>
                                        <Image>
                                            <MediaId><![CDATA[{3}]]></MediaId>
                                        </Image>
                                  </xml>", model.FromUserName, model.ToUserName, DateTimeToUnixInt(DateTime.Now).ToString(),
                                         model.MediaId);
        }

        public string GetText(WeiXinModel model, HttpContext context)
        {
            if (model.Content.Contains("二维码"))
            {
                string openId = HandleUser(model, context, false);

                string result = WeiXin_API_Helper.GenerateQrCode(context, openId);
                PostMaterialResponse pmr = JsonConvert.DeserializeObject<PostMaterialResponse>(result);
                model.MediaId = pmr.media_id;
                return GetImage(model);
            }
            return string.Format(@"<xml>
                                        <ToUserName><![CDATA[{0}]]></ToUserName>
                                        <FromUserName><![CDATA[{1}]]></FromUserName>
                                        <CreateTime>{2}</CreateTime>
                                        <MsgType><![CDATA[text]]></MsgType>
                                        <Content><![CDATA[{3}]]></Content>
                                  </xml>", model.FromUserName, model.ToUserName, DateTimeToUnixInt(DateTime.Now).ToString(), Welcome);
        }
        public string HandleUser(WeiXinModel model, HttpContext context, bool subscribe)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?openid={0}&lang=zh_CN&access_token=", model.FromUserName);
            string userInfoStr = WeiXin_API_Helper.GetRequest(context, url);
            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfoStr);

            WeixinUsersBLL bll = new WeixinUsersBLL();
            if (!bll.Exsits(userInfo.openid))
            {
                userInfo.tagid_list_str = string.Join(",", userInfo.tagid_list);
                userInfo.subscribeTime = GetTime(userInfo.subscribe_time.ToString());
                userInfo.userType = subscribe ? 2 : 1;
                userInfo.remark = userInfo.nickname;
                bll.Add(userInfo);
            }
            return userInfo.openid;
        }
        public int DateTimeToUnixInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        private DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        public void Valid(HttpContext context)
        {
            string token = "orange";
            string timestamp = context.Request["timestamp"].ToString();
            string nonce = context.Request["nonce"].ToString();
            string signature = context.Request["signature"].ToString();
            string echostr = context.Request["echostr"].ToString();

            string[] arr = { token, timestamp, nonce };
            Array.Sort(arr);
            string list = string.Join("", arr);
            list = FormsAuthentication.HashPasswordForStoringInConfigFile(list, "SHA1").ToLower();

            context.Response.ContentType = "text/plain";

            if (list == signature)
            {
                using (var stream = new FileStream(context.Server.MapPath("request.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine("token:" + token);
                    writer.WriteLine("timestamp:" + timestamp);
                    writer.WriteLine("nonce:" + nonce);
                    writer.WriteLine("signature:" + signature);
                    writer.WriteLine("Success!");
                    writer.Close();
                }

                context.Response.Write(echostr);
            }
            else
            {
                context.Response.Write(echostr);
            }
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}