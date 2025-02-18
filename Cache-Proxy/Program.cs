using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class CacheProxyServer
{
    // Cache dictionary to store request and response pairs
    private static Dictionary<string, string> cache = new Dictionary<string, string>();

    public static void Main(string[] args)
    {
        TcpListener server = null;
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                TcpClient client = server.AcceptTcpClient();

                // Spawn a new thread to handle the client request
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unknown Exception: {0}", ex);
        }
        finally
        {
            server?.Stop();
        }

        Console.WriteLine("\nHit enter to continue...");
        Console.Read();
    }

    private static void HandleClient(object obj)
    {
        using TcpClient client = (TcpClient)obj;
        Console.WriteLine("Connected!");

        Byte[] bytes = new Byte[256];
        String? data = null;
        NetworkStream stream = client.GetStream();

        int i;

        // Log when waiting for data
        Console.WriteLine("Waiting for data from client...");

        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
        {
            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
            Console.WriteLine("Received: {0}", data);

            if (cache.ContainsKey(data))
            {
                string cachedResponse = cache[data];
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(cachedResponse);
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Cache hit: Sent cached response: {0}", cachedResponse);
            }
            else
            {
                string response = ProcessRequest(data).Result; // Await the async method

                cache[data] = response;

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Cache miss: Sent new response: {0}", response);
            }
        }

        // Log when the client disconnects
        Console.WriteLine("Client disconnected.");
    }

    private static async Task<string> ProcessRequest(string request)
    {
        // Define the target server address and port
        string targetServer = "127.0.0.1"; // Replace with the target server's IP or hostname
        int targetPort = 80; // Replace with the target server's port

        using (TcpClient targetClient = new TcpClient())
        {
            try
            {
                // Connect to the target server
                await targetClient.ConnectAsync(targetServer, targetPort);
                Console.WriteLine("Connected to target server.");

                // Get the network stream for sending and receiving data
                NetworkStream targetStream = targetClient.GetStream();

                // Convert the request string to bytes
                byte[] requestBytes = Encoding.ASCII.GetBytes(request);

                // Send the request to the target server
                await targetStream.WriteAsync(requestBytes, 0, requestBytes.Length);
                Console.WriteLine("Request forwarded to target server.");

                // Read the response from the target server
                byte[] buffer = new byte[4096];
                int bytesRead = await targetStream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Response received from target server.");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ProcessRequest: {0}", ex.Message);
                return "Error: " + ex.Message;
            }
        }
    }
}