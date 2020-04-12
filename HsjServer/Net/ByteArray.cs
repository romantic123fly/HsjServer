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
        public const int DefaultSize = 1024;
        //初始大小
        public int initSize = 0;
        //缓冲区
        public byte[] Bytes;
        //读写位置
        public int startIndex = 0;
        public int endIndex = 0;
        //容量
        private int capacity = 0;
        //剩余空间
        public int freeSpace { get { return capacity - endIndex; } }
        //数据长度
        public int Length { get { return endIndex - startIndex; } }

        public ByteArray()
        {
            Bytes = new byte[DefaultSize];
            capacity = DefaultSize;
            initSize = DefaultSize;
            startIndex = 0;
            endIndex = 0;
        }
        //检测并移动数据
        public void CheckAndMoveBytes()
        {
            if (Length < 8)
            {
                MoveBytes();
            }
        }
        public void MoveBytes()
        {
            Array.Copy(Bytes, startIndex, Bytes, 0, Length);
            endIndex = Length; startIndex = 0;

        }

        public void ReSize(int size)
        {
            int a = 1024;
            while (size > a) a *= 2;
            capacity = a;
            byte[] newBytes = new byte[capacity];
            Array.Copy(Bytes, startIndex, newBytes, 0, Length);
            Bytes = newBytes;
            endIndex = Length; startIndex = 0;
        }
    }
}
