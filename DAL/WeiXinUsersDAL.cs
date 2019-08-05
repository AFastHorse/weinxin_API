using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using WeiXinLearn.Models;
using WeiXinLearn.CommonHelper;

namespace WeiXinLearn.DAL
{
    public class WeiXinUsersDAL : SqlHelper
    {
        public WeiXinUsersDAL() { }
        public UserInfo GetModel(string openid)
        {
            UserInfo model = null;
            try
            {
                using (var conn = OpenConnection())
                {
                    string sqlStr = @"select * from [WeiXinUsers] where openid=@OpenId ";
                    model = conn.QueryFirstOrDefault<UserInfo>(sqlStr, new { OpenId = openid });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return model;
        }
        public bool Exsits(string openid)
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    string sqlStr = @"select count(*) from [WeiXinUsers] where openid=@OpenId ";
                    return conn.ExecuteScalar<int>(sqlStr, new { OpenId = openid }) > 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            } 
        }

        public int Add(UserInfo model)
        {
            using (var conn = OpenConnection())
            {
                string sqlStr = @"insert into WeiXinUsers
                                (subscribe,openid,nickname,sex,language,city,province,country,headimgurl,subscribeTime,unionid,
                                 remark,groupid,tagid_list_str,subscribe_scene,qr_scene,qr_scene_str,userType)
                                values
                                (@subscribe,@openid,@nickname,@sex,@language,@city,@province,@country,@headimgurl,@subscribeTime,
                                 @unionid,@remark,@groupid,@tagid_list_str,@subscribe_scene,@qr_scene,@qr_scene_str,@userType);
                                 select @@Identity;";
                return conn.ExecuteScalar<int>(sqlStr, model);
            }
        }
        public bool AddRange(List<UserInfo> list)
        {
            using (var conn = OpenConnection())
            {
                string sqlStr = @"insert into WeiXinUsers
                                (subscribe,openid,nickname,sex,language,city,province,country,headimgurl,subscribeTime,unionid,
                                 remark,groupid,tagid_list_str,subscribe_scene,qr_scene,qr_scene_str,userType)
                                values
                                (@subscribe,@openid,@nickname,@sex,@language,@city,@province,@country,@headimgurl,@subscribeTime,
                                 @unionid,@remark,@groupid,@tagid_list_str,@subscribe_scene,@qr_scene,@qr_scene_str,@userType);";
                return conn.ExecuteScalar<int>(sqlStr, list) > 0;
            }
        }
        public List<UserInfo> GetList(string strWhere, int pageIndex = 0, int pageSize = 0)
        {
            using (var conn = OpenConnection())
            {
                string sqlStr = @"select * from [WeiXinUsers] where 1=1 " + strWhere + " Order by subscribeTime";
                if (pageSize != 0)
                {
                    sqlStr = @"select top " + pageSize + @" * from (select row_number() over(order by subscribeTime) as rownumber,
                            * from WeiXinUsers where 1=1 " + strWhere + @") users where rownumber between " + ((pageIndex - 1) * pageSize + 1)
                            + " and " + pageIndex * pageSize + strWhere + " Order by subscribeTime";
                }
                return conn.Query<UserInfo>(sqlStr).ToList();
            }
        }
        public int GetCount(string strWhere)
        {
            using (var conn = OpenConnection())
            {
                string sqlStr = @"select count(*) from [WeiXinUsers] where 1=1 " + strWhere;
                return conn.ExecuteScalar<int>(sqlStr);
            }
        }
    }
}
