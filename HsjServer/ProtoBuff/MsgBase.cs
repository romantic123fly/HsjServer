using HsjServer.Net;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgBase
{
    public virtual ProtocolEnum ProtoType { get; set; }

    /// <summary>
    /// 编码协议名
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase) 
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());
        Int16 len = (Int16)nameBytes.Length;
        //定义新的字节数组存储前两位存储协议名长度，后面存储协议名字节内容
        byte[] bytes = new byte[2 + len];
        //协议名的长度占两个字节
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        //把协议名长度和协议名内容拼起来
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }

    /// <summary>
    /// 解码协议名
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static ProtocolEnum DecodeName(byte[] bytes) 
    {
        try
        {
            string name = System.Text.Encoding.UTF8.GetString(bytes,6, bytes[4]);
            return (ProtocolEnum)System.Enum.Parse(typeof(ProtocolEnum), name);
        }
        catch (Exception ex) 
        {
            Debug.LogError("不存在的协议:" + ex.ToString());
            return ProtocolEnum.None;
        }
    }

    /// <summary>
    /// 协议序列化及加密
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encond(MsgBase msgBase) 
    {
        using (var memory = new MemoryStream()) 
        {
            //将我们的协议类进行序列化转换成数组
            Serializer.Serialize(memory, msgBase);
            byte[] bytes = memory.ToArray();
            string secret = ServerSocket.SecretKey;
            //对数组进行加密
            if (msgBase is MsgSecret) 
            {
                secret = ServerSocket.PublicKey;
            }
            bytes = AES.AESEncrypt(bytes, secret);
            return bytes;
        }
    }

    /// <summary>
    /// 协议解密
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static MsgBase Decode(ProtocolEnum protocol, byte[] bytes, int count) 
    {
        try
        {
            byte[] newBytes = new byte[count];
            Array.Copy(bytes, 6 + bytes[4], newBytes, 0, count);
            string secret = ServerSocket.SecretKey;
            if (protocol == ProtocolEnum.MsgSecret) 
            {
                secret = ServerSocket.PublicKey;
            }
            newBytes = AES.AESDecrypt(newBytes, secret);
            using (var memory = new MemoryStream(newBytes, 0, newBytes.Length)) 
            {
                Type t = System.Type.GetType(protocol.ToString());
                return (MsgBase)Serializer.NonGeneric.Deserialize(t, memory);
            }
        }
        catch(Exception ex) 
        {
            Debug.LogError("协议解密出错:" + ex.ToString());
            return null;
        }
    }
}

