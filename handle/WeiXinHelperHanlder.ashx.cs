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
using WeiXinLearn.CommonHelper;
using WeiXinLearn.Models;

namespace WeiXinLearn.handle
{
    /// <summary>
    /// WeiXinHelperHanlder 的摘要说明
    /// </summary>
    public class WeiXinHelperHanlder : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string action = context.Request["action"];
            string result = string.Empty;
            switch (action)
            {
                case "access_token"://获取token
                    result = Access_TokenDAL.GetAccessToken().access_token;
                    break;
                case "ip_list"://获取微信服务器IP地址
                    result = WeiXin_API_Helper.Get_IPList(context);
                    break;
                case "check"://网络检测
                    result = WeiXin_API_Helper.Get_CheckResult(context);
                    break;
                case "create_menu"://创建自定义菜单
                    result = WeiXin_API_Helper.CreateMenu(context);
                    break;
                case "get_temp_media"://获取临时素材
                    result = WeiXin_API_Helper.Get_TempMedia(context);
                    break;
                case "post_media"://上传临时/永久素材
                    result = WeiXin_API_Helper.PostMedia(context);
                    break;
                case "get_qrcode":
                    result = WeiXin_API_Helper.GetQrCode(context, WeiXin_API_Helper.InitQrCode(context));
                    break;
                case "generate_qrcode":
                    result = WeiXin_API_Helper.GenerateQrCode(context);
                    break;
            }
            context.Response.ContentType = "text/plain";
            context.Response.Write(result);
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