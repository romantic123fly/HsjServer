using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace HsjServer.Net
{
    public class ServerSocket : Singleton<ServerSocket>
    {
        public static string PublicKey = "幻世界"; //公钥
        public static string SecretKey = "HuanShijie";   //密钥，后续可以随时间进行变化
#if DEBUG
        private string m_IpStr = "127.0.0.1";
#else
        private string m_IpStr = "127.0.0.1";
#endif
        private const int m_Port = 8011;
        public static long m_PingInterval = 5;
        private static Socket m_ListenSocket; //服务器监听socket
        public static Dictionary<Socket, ClientSocket> m_ClientDic = new Dictionary<Socket, ClientSocket>();//所有链接客户端的一个字典
        private static List<Socket> m_CheckReadList = new List<Socket>();//保存所有socket的集合
        public static List<ClientSocket> m_TempList = new List<ClientSocket>();//临时存储断线的客户端

        public void Init()
        {
            IPAddress ip = IPAddress.Parse(m_IpStr);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, m_Port);
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.Bind(ipEndPoint);
            m_ListenSocket.Listen(50000);
            Debug.LogInfo("服务器{0}启动监听成功", m_ListenSocket.LocalEndPoint.ToString());

            StartCheckReceiveClient();
        }

        private void StartCheckReceiveClient()
        {
            while (true)
            {
                m_CheckReadList.Clear();
                m_CheckReadList.Add(m_ListenSocket);
                foreach (Socket s in m_ClientDic.Keys)
                {
                    m_CheckReadList.Add(s);
                }
                //多路复用模式
                try { Socket.Select(m_CheckReadList, null, null, 1000); }
                catch (Exception e) { Debug.LogError(e.Message); }
                for (int i = m_CheckReadList.Count - 1; i >= 0; i--)
                {
                    Socket s = m_CheckReadList[i];
                    if (s == m_ListenSocket)
                        ReadListen(s);//存储当前客户端
                    else
                        ReadClientMessage(s); //说明链接的客户端可读，有信息传上来了
                }
                CheckHeart();
            }
        }

        //检测是否心跳包超时的计算
        private void CheckHeart()
        {
            m_TempList.Clear();
            foreach (ClientSocket clientSocket in m_ClientDic.Values)
            {
                if (GetTimeStamp() - clientSocket.LastPingTime > m_PingInterval * 4)
                {
                    Debug.Log("Ping Close" + clientSocket.Socket.RemoteEndPoint.ToString());
                    m_TempList.Add(clientSocket);
                }
            }
            foreach (ClientSocket clientSocket in m_TempList)
            {
                CloseClient(clientSocket);
            }
        }
        /// <summary>
        /// 获取链接的客户端加入管理
        /// </summary>
        /// <param name="listen">监听Socket</param>
        void ReadListen(Socket listen)
        {
            try
            {
                Socket client = listen.Accept();
                ClientSocket clientSocket = new ClientSocket();
                clientSocket.Socket = client;
                clientSocket.LastPingTime = GetTimeStamp();
                m_ClientDic.Add(client, clientSocket);
                Debug.Log("一个客户端链接：{0},当前{1}个客户端在线！", client.LocalEndPoint.ToString(), m_ClientDic.Count);
            }
            catch (SocketException ex)
            {
                Debug.LogError("Accept fali:" + ex.ToString());
            }
        }
        /// <summary>
        ///接收到客户端消息
        /// </summary>
        /// <param name="client"></param>
        void ReadClientMessage(Socket client)
        {
            ClientSocket clientSocket = m_ClientDic[client];
            ByteArray readBuff = clientSocket.ReadBuff;

            //如果上一次接收数据刚好占满了1024的数组，
            if (readBuff.Remain <= 0)
            {
                //保证到如果数据长度大于默认长度，扩充数据长度，保证信息的正常接收
                while (readBuff.Remain <= 0)
                {
                    int expandSize = readBuff.Length < ByteArray.DEFAULT_SIZE ? ByteArray.DEFAULT_SIZE : readBuff.Length;
                    readBuff.ReSize(expandSize * 2);
                }
            }
            int count = client.Receive(readBuff.Bytes, readBuff.WriteIdx, readBuff.Remain, 0);
            if (count <= 0) //代表客户端断开链接了
            {
                CloseClient(clientSocket);
                return;
            }

            readBuff.WriteIdx += count;
            OnReceiveData(clientSocket);
        }

        /// <summary>
        /// 接收数据处理，根据信息解析协议，根据协议内容处理消息再下发到客户端
        /// </summary>
        /// <param name="clientSocket"></param>
        void OnReceiveData(ClientSocket clientSocket)
        {
            ByteArray readbuff = clientSocket.ReadBuff;
            byte[] bytes = readbuff.Bytes;
            int bodyLength = BitConverter.ToInt32(bytes, readbuff.ReadIdx);
            //判断接收到的信息长度是否小于包体长度+包体头长度，如果小于，代表我们的信息不全，大于代表信息全了（有可能有粘包存在）
            if (readbuff.Length < bodyLength + 4) { return; }

            //解析协议名
            ProtocolEnum proto = MsgBase.DecodeName(readbuff.Bytes);
            if (proto == ProtocolEnum.None)
            {
                Debug.LogError("OnReceiveData MsgBase.DecodeName  fail");
                CloseClient(clientSocket);
                return;
            }

            //解析协议体
            int bodyCount = bodyLength - readbuff.Bytes[4] - 2;

            MsgBase msgBase = MsgBase.Decode(proto, readbuff.Bytes, bodyCount);
            if (msgBase == null)
            {
                Debug.LogError("{0}协议内容解析错误：" + proto.ToString());
                CloseClient(clientSocket);
                return;
            }
            //通过反射分发消息
            MethodInfo mi = typeof(MsgHandler).GetMethod(proto.ToString());
            object[] o = { clientSocket, msgBase };
            if (mi != null) mi.Invoke(null, o);
            else Debug.LogError("OnReceiveData Invoke fail:" + proto.ToString());

            readbuff.ReadIdx = bodyLength + 4;
            readbuff.CheckAndMoveBytes();
            //继续读取消息
            if (readbuff.Length > 4)
            {
                OnReceiveData(clientSocket);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="msgBase"></param>
        public static void SendMessage(ClientSocket clientSocket, MsgBase msgBase)
        {
            if (clientSocket == null || !clientSocket.Socket.Connected)
            {
                return;
            }
            try
            {
                //分为三部分，头：总协议长度；名字；协议内容。
                byte[] nameBytes = MsgBase.EncodeName(msgBase);
                byte[] bodyBytes = MsgBase.Encond(msgBase);
                int len = nameBytes.Length + bodyBytes.Length;
                byte[] byteHead = BitConverter.GetBytes(len);
                byte[] sendBytes = new byte[byteHead.Length + len];
                Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);
                Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);
                Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);

                clientSocket.Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
            }
            catch (SocketException ex)
            {
                Debug.LogError("Socket发送数据失败：" + ex);
            }
        }

        public void CloseClient(ClientSocket client)
        {
            client.Socket.Close();
            m_ClientDic.Remove(client.Socket);
            Debug.Log("一个客户端断开链接，当前总连接数：{0}", m_ClientDic.Count);
        }

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
    }
}