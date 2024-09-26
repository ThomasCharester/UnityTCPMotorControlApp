using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class EspRecieveSender : MonoBehaviour
{
    public static EspRecieveSender Instance = null;
    void Awake()
    {
        //ConnectToServer();
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance == this)
        {
            Destroy(gameObject);
        }
    }

    public delegate void GetDataFromServer(string data);
    public event GetDataFromServer GetDataFromServerEvent;

    public delegate void ServerConnection(bool connectionStatus,string address);
    public event ServerConnection ServerConnectionEvent;

    public string serverIP = "192.168.100.55"; // Set this to your server's IP address.
    public int serverPort = 5045;             // Set this to your server's port.

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    public void SetIpAddress(string ipAdress)
    {
        serverIP = ipAdress;
    }
    void Update()
    {
    }
    public void StartConnection()
    {
        StopConnection();
        ConnectToServer();
    }

    public void StopConnection()
    {
        if (client != null)
        {

            if (clientReceiveThread != null)
                clientReceiveThread.Abort();
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
        ServerConnectionEvent.Invoke(false,null);
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(serverIP, serverPort);

            stream = client.GetStream();
            Debug.Log("Connected to server.");
            ServerConnectionEvent.Invoke(true, serverIP);


            clientReceiveThread = new Thread(new ThreadStart(ListenForData));

            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
            UIController.Instance.ShowError("Connection error: " + e.ToString());
        }
    }

    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                // Check if there's any data available on the network stream
                if (stream.DataAvailable)
                {
                    int length;
                    // Read incoming stream into byte array.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        Debug.Log("Server message received: " + serverMessage);
                        
                        GetDataFromServerEvent.Invoke(serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
            UIController.Instance.ShowError("Listen error: " + socketException);
        }
    }

    public void SendMessageToServer(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            UIController.Instance.ShowError("Connection error: not connected to server");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent message to server: " + message);
    }

    void OnApplicationQuit()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientReceiveThread != null)
            clientReceiveThread.Abort();
    }
}
