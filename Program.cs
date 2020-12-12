using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace auctioncli
{
    public class Program
    {
        public static int TAM = 1024;
        private static string _ip = "127.0.0.1";
        private static int _port = 11000;

        public static void ReadServerIpPort()
        {
            string s;
            System.Console.WriteLine("Datos del servidor: ");
            string defIp = GetLocalIpAddress().ToString();
            System.Console.Write("Dir. IP [{0}]: ", defIp);
            s = Console.ReadLine();
            if ((s.Length > 0) && (s.Replace(".", "").Length == s.Length - 3))
            {
                _ip = s;
            }
            else
            {
                _ip = defIp;
            }
            System.Console.Write("PUERTO [{0}]: ", _port);
            s = Console.ReadLine();
            if (Int32.TryParse(s, out int i))
            {
                _port = i;
            }
        }

        private static IPAddress GetLocalIpAddress()
        {
            List<IPAddress> ipAddressList = new List<IPAddress>();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            int t = ipHostInfo.AddressList.Length;
            string ip;
            for (int i = 0; i < t; i++)
            {
                ip = ipHostInfo.AddressList[i].ToString();
                if (ip.Contains(".") && !ip.Equals("127.0.0.1")) ipAddressList.Add(ipHostInfo.AddressList[i]);
            }
            if (ipAddressList.Count > 0)
            {
                return ipAddressList[0];//devuelve la primera posible
            }
            return null;
        }

        public static Socket Connect()
        {
            IPAddress ipAddress = System.Net.IPAddress.Parse(_ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);

            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(remoteEP);

            return socket;
        }

        public static void Disconnect(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public static void Pujar(Socket sender)
        {
            byte[] bytes = new byte[TAM];
            Console.WriteLine("Conectado al servidor. Esperando producto...");
            while (true)
            {
                // Recibir producto
                int bytesRec = sender.Receive(bytes);
                if (bytesRec == 0) break;
                string producto = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                // Pujar
                int valor = LeerPuja(producto);
                // Enviar puja
                byte[] msg = Encoding.ASCII.GetBytes(valor.ToString());
                int bytesSent = sender.Send(msg);
                // Esperar resultado
                bytesRec = sender.Receive(bytes);
                string resultado = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine(resultado);
                Console.WriteLine("Esperando la recepción de un nuevo producto...");

            }
            Console.WriteLine("Fin de la subasta.");
        }

        private static int LeerPuja(string producto)
        {
            string s = null;
            while (true)
            {
                Console.WriteLine("Producto a subasta: {0}. Introduce puja:", producto);
                s = Console.ReadLine();
                if (Int32.TryParse(s, out int i))
                {
                    if (i >= 0)
                    {
                        return i;
                    }
                }
            }
        }

        public static int Main(String[] args)
        {
            ReadServerIpPort();
            Socket socket = Connect();
            Pujar(socket);
            Disconnect(socket);
            return 0;
        }
    }
}
