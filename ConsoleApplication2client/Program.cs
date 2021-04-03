using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace SocketTcpClient
{
    class Program
    {
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, currentLineCursor);
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
                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
                    do
                    {
                        bytes = socket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (socket.Available > 0);
                    Console.WriteLine(builder.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                string message = "Server at " + socket.RemoteEndPoint.ToString() + " is down";
                Console.WriteLine(message);
            }
        }
        static int port = 8005;
        static string address = "192.168.100.7";
        static void Main(string[] args)
        {
            Console.WriteLine("Введите ip сервера:");
            address = Console.ReadLine();
            Console.WriteLine("Введите порт сервера:");
            port = Int32.Parse(Console.ReadLine());
            while (true)
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(ipPoint);
                    Task listeningTask = new Task(() => ReadFromSocket(socket));
                    listeningTask.Start();
                    while (true)
                    {
                        string message = Console.ReadLine();
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        ClearCurrentConsoleLine();
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        socket.Send(Encoding.Unicode.GetBytes(message));
                    }
                }
                catch (Exception ex)
                {
                    string message = "Server at " + ipPoint.ToString() + " is down, reconnecting... ";
                    Console.WriteLine(message);
                }
            }
        }
    }
}