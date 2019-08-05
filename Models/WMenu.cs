using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinLearn.Models
{
    public class MButton
    {
        public string type { get; set; }
        public string name { get; set; }
        public string key { get; set; }
        public string url { get; set; }
        public string appid { get; set; }
        public string pagepath { get; set; }
        public string media_id { get; set; }
        public List<MButton> sub_button { get; set; }
    }
}