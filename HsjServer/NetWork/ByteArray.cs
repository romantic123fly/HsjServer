using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HsjServer.Net
{
    public class ByteArray
    {
        //默认大小
        public const int DEFAULT_SIZE = 1024;
        //初始大小
        public int initSize = 0;
        //缓冲区
        public byte[] Bytes;
        //读写位置
        public int ReadIdx = 0;
        public int WriteIdx = 0;
        //容量
        private int capacity = 0;
        //剩余空间
        public int Remain { get { return capacity - WriteIdx; } }
        //数据长度
        public int Length { get { return WriteIdx - ReadIdx; } }

        public ByteArray()
        {
            Bytes = new byte[DEFAULT_SIZE];
            capacity = DEFAULT_SIZE;
            initSize = DEFAULT_SIZE;
            ReadIdx = 0;
            WriteIdx = 0;
        }
        //检测并移动数据
        public void CheckAndMoveBytes()
        {
            if (Length < 8)
            {
                Array.Copy(Bytes, ReadIdx, Bytes, 0, Length);
                WriteIdx = Length; ReadIdx = 0;
            }
        }


        public void ReSize(int size)
        {
            int a = 1024;
            while (size > a) a *= 2;
            capacity = a;
            byte[] newBytes = new byte[capacity];
            Array.Copy(Bytes, ReadIdx, newBytes, 0, Length);
            Bytes = newBytes;
            WriteIdx = Length; ReadIdx = 0;
        }
    }
}
