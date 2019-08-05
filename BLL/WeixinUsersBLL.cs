using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WeiXinLearn.DAL;
using WeiXinLearn.Models;

namespace WeiXinLearn.BLL
{
    public class WeixinUsersBLL
    {
        public WeiXinUsersDAL dal;
        public WeixinUsersBLL()
        {
            dal = new WeiXinUsersDAL();
        }
        public UserInfo GetModel(string openid)
        {
            return dal.GetModel(openid);
        }
        public bool Exsits(string openid)
        {
            return dal.Exsits(openid);
        }


        public int Add(UserInfo model)
        {
            return dal.Add(model);
        }
        public bool AddRange(List<UserInfo> list)
        {
            return dal.AddRange(list);
        }

        public List<UserInfo> GetList(string strWhere, int pageIndex = 0, int pageSize = 0)
        {
            return dal.GetList(strWhere, pageIndex, pageSize);
        }
        public int GetCount(string strWhere)
        {
            return dal.GetCount(strWhere);
        }
    }
}