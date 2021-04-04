using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
namespace SocketTcpServer
{
    class Program
    {
        static List<Socket> connections = new List<Socket>();
        public static void SendAll(string message)
        {
            foreach (Socket connection in connections)
            {
                connection.Send(Encoding.Unicode.GetBytes(message));
            }
        }
        public static void ReadFromSocket(Socket socket)
        {
            try
            {
                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];
                    do
                    {
                        bytes = socket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (socket.Available > 0);
                    Console.WriteLine("[" + DateTime.Now.ToShortTimeString() + "]" + socket.RemoteEndPoint + ": " + builder.ToString());
                    SendAll("[" + DateTime.Now.ToShortTimeString() + "]" + socket.RemoteEndPoint + ": " + builder.ToString());
                }
            }
            catch (Exception ex)
            {
                connections.Remove(socket);
                string message = "User at " + socket.RemoteEndPoint + " disconnected ";
                Console.WriteLine(message);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                SendAll(message);
            }
        }
        public static bool PortInUse(int port)
        {
            bool inUse = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
        static int port = 8005;
        static void Main(string[] args)
        {
            Console.WriteLine("Available for connection ip:");
            List<string> adresses = new List<string>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Console.WriteLine(ip.Address.ToString());
                            adresses.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            Console.WriteLine("Enter desired ip");
            string adr = Console.ReadLine();/*
            string adr = "";
            while (true)
            {
                string message = Console.ReadLine();
                if (!adresses.Contains(message))
                {
                    Console.WriteLine("Введён неверный адрес, повторите ввод");
                }
                else
                {
                    adr = message;
                    break;
                }
            }*/
            Console.WriteLine("Enter desired port");
            while (true)
            {
                string message = Console.ReadLine();
                if (PortInUse(Int32.Parse(message)))
                {
                    Console.WriteLine("Port is already in use");
                }
                else
                {
                    port = Int32.Parse(message);
                    break;
                }
            }
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(adr), port);
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(10);
                Console.WriteLine("Server launched. Waiting for connections...");
                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    connections.Add(handler);
                    Task listeningTask = new Task(() => ReadFromSocket(handler));
                    listeningTask.Start();
                    SendAll("User at " + handler.RemoteEndPoint + " just connected ");
                    Console.WriteLine("User at " + handler.RemoteEndPoint + " just connected ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}