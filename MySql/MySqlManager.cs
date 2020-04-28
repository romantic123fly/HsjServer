using SqlSugar;
using System;
using System.Linq;

namespace MySql
{
    public class MySqlManager:Singleton<MySqlManager>
    {
#if DEBUG
        private const string connectingStr = "server=localhost;uid=root;pwd=romantic;database=user";
#else
         private const string connectingStr = "server=localhost;uid=hsj;pwd=romantic123456;database=SqlSugar4XTest";
#endif
        public SqlSugarClient sqlSugarClient = null;

        public  void Init()
        {
            sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connectingStr,
                DbType =DbType.MySql,
                IsAutoCloseConnection =true,
                InitKeyType = InitKeyType.Attribute,
            }) ;

#if DEBUG
            sqlSugarClient.Aop.OnLogExecuting = (sql,pars) =>
            {
                Console.WriteLine(sql + "\r\n" + sqlSugarClient.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
#endif
        }
    }
}
