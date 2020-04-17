using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HsjServer.Net
{
    public class ClientSocket
    {
        public Socket Socket { get; set; }
        public long LastPingTime { get; set; }
        public ByteArray ReadBuff = new ByteArray();

    }
}
