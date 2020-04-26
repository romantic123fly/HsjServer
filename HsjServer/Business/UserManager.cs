using MySql;
using MySql.MySqlData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HsjServer.Business
{
    public class UserManager : Singleton<UserManager>
    {

        public RegisterResult Register(RegisterType registerType,string userName,string pwd )
        {
			try
			{
				int count = MySqlManager.Instance.sqlSugarClient.Queryable<User>().Where(it => it.Username == userName).Count();
				if (count > 0)
					return RegisterResult.AlreadyExist;
				User user = new User();
				switch (registerType)
				{
					case RegisterType.Phone:
						user.Logintype = "Phone";
						break;
					case RegisterType.Mail:
						user.Logintype = "Mail";
						break;
					default:
						break;
				}

				user.Username = userName;
				user.Password = pwd;
				user.Logindate  = DateTime.Now;
				MySqlManager.Instance.sqlSugarClient.Insertable(user).ExecuteCommand();
				return RegisterResult.Success;
			}
			catch (Exception e)
			{
				Debug.LogError("注册失败：" + e);
				return RegisterResult.Failed;
			}
        }
    }
}
