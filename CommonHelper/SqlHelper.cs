using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WeiXinLearn.CommonHelper
{
    public class SqlHelper
    {
        private readonly string ConStr = ConfigurationManager.ConnectionStrings["EduPcDb"].ToString();
        public SqlHelper() { }
        public IDbConnection OpenConnection()
        {
            var connection = new SqlConnection(ConStr);
            if (connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return connection;
        }
    }
}