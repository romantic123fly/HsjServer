using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HsjServer.Business
{
    public enum RegisterType { 
    Phone,
    Mail
    
    }
    public enum RegisterResult
    {
        Success,
        Failed,
        AlreadyExist,
        WrongCode,
        Forbidden
    }
    public enum LoginResult
    {
        Success,
        Failed,
        AlreadyLogin,
        WrongPwd,
        UserNotExits,
        TimeoutToken
    }
}
