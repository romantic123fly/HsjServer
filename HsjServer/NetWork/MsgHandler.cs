﻿using HsjServer.Business;
using HsjServer.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HsjServer.Net
{
    public partial class MsgHandler
    {
        /// <summary>
        /// 所有的协议处理函数都是这个标准，函数名=协议枚举名=类名
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgSecret(ClientSocket c, MsgBase msgBase) 
        {

            MsgSecret msgSecret = (MsgSecret)msgBase;
            msgSecret.Srcret = ServerSocket.SecretKey;
            ServerSocket.SendMessage(c, msgSecret);
        }

        public static void MsgPing(ClientSocket c, MsgBase msgBase)
        {
            c.LastPingTime = ServerSocket.GetTimeStamp();
            MsgPing msPong = new MsgPing();
            ServerSocket.SendMessage(c, msPong);
        }

        public static void MsgTest(ClientSocket c, MsgBase msgBase)
        {
            MsgTest msgTest = (MsgTest)msgBase;
            Debug.Log(msgTest.ReqContent);
            msgTest.RecContent = "服务器测试发送的数据:aaaaaaaaaaaaaaaa";
            ServerSocket.SendMessage(c, msgTest);
            ServerSocket.SendMessage(c, msgTest);
            ServerSocket.SendMessage(c, msgTest);
            ServerSocket.SendMessage(c, msgTest);


        }

        /// <summary>
        /// 处理注册信息
        /// </summary>
        /// <param name = "c" ></ param >
        /// < param name="msgBase"></param>
        public static void MsgRegister(ClientSocket c, MsgBase msgBase)
        {
            MsgRegister msg = (MsgRegister)msgBase;
            RegisterResult rst = UserManager.Instance.Register(msg.RegisterType, msg.Account, msg.Password, out string token);
            msg.Result = rst;
            ServerSocket.SendMessage(c, msg);
        }

        /// <summary>
        /// 处理登录信息
        /// </summary>
        /// <param name="c"></param>
        /// <param name="msgBase"></param>
        public static void MsgLogin(ClientSocket c, MsgBase msgBase)
        {
            MsgLogin msg = (MsgLogin)msgBase;
            LoginResult rst = UserManager.Instance.Login(msg.LoginType, msg.Account, msg.Password, out int userid, out string token);
            msg.Result = rst;
            msg.Token = token;
            c.UserId = userid;
            ServerSocket.SendMessage(c, msg);
        }
    }
}
