﻿using HsjServer.Net;
using MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HsjServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //MySqlManager.Instance.Init();
            ServerSocket.Instance.Init();
            Console.ReadLine();
        }
    }
}
