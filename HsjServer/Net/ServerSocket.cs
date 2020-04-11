using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HsjServer.Net
{
    public class ServerSocket : Singleton<ServerSocket>
    {
        //公钥
        public static string PublicKey = "OceanSever";
        //密钥，后续可以随时间进行变化
        public static string SecretKey = "Ocean_Up&&NB!!";

#if DEBUG
        private string m_IpStr = "127.0.0.1";
#else
        //对应阿里云或腾讯云的 本地ip地址（不是公共ip地址）
        private string m_IpStr = "172.45.756.54";
#endif
        private const int m_Port = 8011;

        public static long m_PingInterval = 30;

        //服务器监听socket
        private static Socket m_ListenSocket;

        //临时保存所有socket的集合
        private static List<Socket> m_CheckReadList = new List<Socket>();

        //存储所有客户端的一个字典
        public static Dictionary<Socket, ClientSocket> m_ClientDic = new Dictionary<Socket, ClientSocket>();

        public void Init()
        {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, m_Port);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);
            m_ListenSocket.Listen(50000);

            Debug.LogInfo("服务器启动监听{0}成功", m_ListenSocket.LocalEndPoint.ToString());

            while (true)
            {
                //检测是否有读取的Socket

                //处理找出所有Socket
                ResetCheckRead();

            }
        }

        public void ResetCheckRead()
        {
            m_CheckReadList.Clear();
            m_CheckReadList.Add(m_ListenSocket);

            //遍历客户端列表并添加至Socket总的集合
            foreach (var item in m_ClientDic.Keys)
            {
                m_CheckReadList.Add(item);
            }
        }
    }
}