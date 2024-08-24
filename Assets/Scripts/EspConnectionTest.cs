//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Net.Sockets;
//using System.Net;
//using System.Text;
//using System.Threading;
//using UnityEngine;

//public class EspConnectionTest : MonoBehaviour
//{
//    Thread m_Thread;
//    UdpClient m_Client;


//    void Start()
//    {
//        m_Thread = new Thread(new ThreadStart(ReceiveData));
//        m_Thread.IsBackground = true;
//        m_Thread.Start();
//    }

//    private void Update()
//    {
//        udpSend();
//    }

//    void ReceiveData()
//    {

//        try
//        {

//            m_Client = new UdpClient(4001);
//            m_Client.EnableBroadcast = true;
//            while (true)
//            {

//                IPEndPoint hostIP = new IPEndPoint(IPAddress.Any, 0);
//                byte[] data = m_Client.Receive(ref hostIP);
//                string returnData = Encoding.ASCII.GetString(data);

//                Debug.Log(returnData);
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.Log(e);

//            OnApplicationQuit();
//        }
//    }

//    private void OnApplicationQuit()
//    {
//        if (m_Thread != null)
//        {
//            m_Thread.Abort();
//        }

//        if (m_Client != null)
//        {
//            m_Client.Close();
//        }
//    }
//    void udpSend()
//    {
//        var IP = IPAddress.Parse("192.175.0.20");

//        int port = 4000;


//        var udpClient1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//        var sendEndPoint = new IPEndPoint(IP, port);


//        try
//        {

//            //Sends a message to the host to which you have connected.
//            byte[] sendBytes = Encoding.ASCII.GetBytes("hello from unity");

//            udpClient1.SendTo(sendBytes, sendEndPoint);

//            print("Sent");


//        }
//        catch (Exception e)
//        {
//            Debug.Log(e.ToString());
//        }

//    }
//}
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;

    private void Start()
    {
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToClient("Hello");
        }
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse("172.33.133.57");
            server = new TcpListener(localAddr, 1984);
            server.Start();

            byte[] buffer = new byte[1024];
            string data = null;

            while (true)
            {
                Debug.Log("Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Connected!");

                data = null;
                stream = client.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("Received: " + data);

                    string response = "Server response: " + data.ToString();
                    SendMessageToClient(message: response);
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
        server.Stop();
        thread.Abort();
    }

    public void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("Sent: " + message);
    }
}