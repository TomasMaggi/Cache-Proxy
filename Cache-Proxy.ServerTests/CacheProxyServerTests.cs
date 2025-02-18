using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CacheProxyServerTests
{
    private static string server = "127.0.0.1";
    private static int port = 13000;

    [TestMethod]
    public async Task TestTcpRequest()
    {
        // Arrange
        string tcpMessage = "Hello, TCP Server!";

        // Act
        string tcpResponse = await SendTcpRequest(tcpMessage);

        // Assert
        Assert.IsNotNull(tcpResponse);
        Assert.IsTrue(tcpResponse.Length > 0);
        Console.WriteLine("TCP Response: " + tcpResponse);
    }

    [TestMethod]
    public async Task TestHttpRequest()
    {
        // Arrange
        string httpRequest = "GET / HTTP/1.1\r\nHost: 127.0.0.1\r\nConnection: close\r\n\r\n";

        // Act
        string httpResponse = await SendHttpRequest(httpRequest);

        // Assert
        Assert.IsNotNull(httpResponse);
        Assert.IsTrue(httpResponse.Length > 0);
        Console.WriteLine("HTTP Response: " + httpResponse);
    }

    // Helper method to send a raw TCP request
    private static async Task<string> SendTcpRequest(string message)
    {
        try
        {
            using TcpClient client = new TcpClient(server, port);
            NetworkStream stream = client.GetStream();

            // Convert the message to bytes and send it
            byte[] requestBytes = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
            Console.WriteLine("Sent TCP request: " + message);

            // Read the response from the server
            byte[] buffer = new byte[4096];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in TCP request: " + ex.Message);
            throw; // Rethrow the exception to fail the test
        }
    }

    // Helper method to send an HTTP request
    private static async Task<string> SendHttpRequest(string httpRequest)
    {
        try
        {
            using TcpClient client = new TcpClient(server, port);
            NetworkStream stream = client.GetStream();

            // Convert the HTTP request to bytes and send it
            byte[] requestBytes = Encoding.ASCII.GetBytes(httpRequest);
            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
            Console.WriteLine("Sent HTTP request:\n" + httpRequest);

            // Read the response from the server
            byte[] buffer = new byte[4096];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in HTTP request: " + ex.Message);
            throw; // Rethrow the exception to fail the test
        }
    }
}