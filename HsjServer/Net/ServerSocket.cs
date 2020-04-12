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
        //心跳包检测时间
        public static long m_PingInterval = 30;

        //服务器监听socket
        private static Socket m_ListenSocket;

        //临时保存所有socket的集合
        private static List<Socket> m_CheckReadSocketList = new List<Socket>();

        //存储所有客户端的一个字典
        public static Dictionary<Socket, ClientSocket> m_ClientDic = new Dictionary<Socket, ClientSocket>();

        //存储断开链接的临时的客户端
        public List<ClientSocket> tempSockets = new List<ClientSocket>();
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
                try
                {
                    Socket.Select(m_CheckReadSocketList, null, null, 1000);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
                for (int i = m_CheckReadSocketList.Count - 1; i >= 0; i--)
                {
                    if (m_CheckReadSocketList[i] == m_ListenSocket)
                    {
                        //说明有客户端连接到服务器,服务器socket可读
                        ReadListen(m_CheckReadSocketList[i]);
                    }
                    else
                    {
                        //说明连接的客户端可读，有消息传上来了
                        ReadClient(m_CheckReadSocketList[i]);
                    }
                }

                //检测心跳包是否超时
                long timeNow = GetTimeStamp();
                tempSockets.Clear();
                foreach (var item in m_ClientDic.Values)
                {
                    if (timeNow - item.LastPingTime > m_PingInterval*4)
                    {
                        Debug.Log("Ping Close" + item.Socket.LocalEndPoint.ToString());
                        tempSockets.Add(item);
                        
                    }
                }
                foreach (var item in tempSockets)
                {
                    CloseClient(item);
                }
                tempSockets.Clear();
            }
        }

        public void ResetCheckRead()
        {
            m_CheckReadSocketList.Clear();
            m_CheckReadSocketList.Add(m_ListenSocket);

            //遍历客户端列表并添加至Socket总的集合
            foreach (var item in m_ClientDic.Keys)
            {
                m_CheckReadSocketList.Add(item);
            }
        }

        public void ReadListen(Socket m_ListenSocket)
        {
            try
            {
                Socket client = m_ListenSocket.Accept();
                ClientSocket clientSocket = new ClientSocket();
                clientSocket.Socket = client;
                clientSocket.LastPingTime = GetTimeStamp();
                m_ClientDic.Add(client,clientSocket);
                Debug.Log("一个客户端链接:{0},当前{1}个客户端在线！",client.LocalEndPoint.ToString(),m_ClientDic.Count);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

        }

        void ReadClient(Socket client)
        {
            ClientSocket clientSocket = m_ClientDic[client];
            ByteArray readBuff = clientSocket.Readbuff;
            //接收信息根据信息解析协议，根据协议内容处理信息下发到客户端
            int count = 0;

            try
            {
                count = client.Receive(readBuff.Bytes,readBuff.endIndex,readBuff.freeSpace,0);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                CloseClient(clientSocket);
                return;
            }
            //客户端断开链接了
            if (count <= 0)
            {
                CloseClient(clientSocket);
                return;
            }

            readBuff.endIndex += count;
            //解析信息
            readBuff.CheckAndMoveBytes();

        }
        //关闭客户端socket链接
        public void CloseClient(ClientSocket clientSocket)
        {
            clientSocket.Socket.Close();
            m_ClientDic.Remove(clientSocket.Socket);
            Debug.Log("客户端{0}断开链接,当前{1}个客户端在线！", clientSocket.Socket.LocalEndPoint.ToString(), m_ClientDic.Count);

        }

        public long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970,1,1,0,0,0,0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}