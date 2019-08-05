using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinLearn.Models
{
    public class WeiXinModel
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public string CreateTime { get; set; }
        public string MsgType { get; set; }
        public string Content { get; set; }
        public string MsgId { get; set; }
        public string PicUrl { get; set; }
        public string MediaId { get; set; }
        public string Event { get; set; }
        public string EventKey { get; set; }
        public string Ticket { get; set; }
    }
}