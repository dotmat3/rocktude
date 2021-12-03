using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class NetworkController : MonoBehaviour {
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private List<SocketEvent> receivedEvents = new List<SocketEvent>();
    public string serverAddress = "127.0.0.1";
    public int serverPort = 2004;

    void Start() {
        ConnectToTcpServer();
    }

    private void ConnectToTcpServer() {
        try {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }

        catch (Exception e) {
            Debug.Log("On client connect exception " + e);
        }
    }

    private void ListenForData() {
        socketConnection = new TcpClient(serverAddress, serverPort);

        try {
            Byte[] bytes = new Byte[1024];
            while (true) {
                using (NetworkStream stream = socketConnection.GetStream()) {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        string serverMessage = Encoding.ASCII.GetString(incomingData);
                        SocketEvent socketEvent = JsonUtility.FromJson<SocketEvent>(serverMessage);
                        Type eventType = Type.GetType(socketEvent.type);
                        SocketEvent newEvent = (SocketEvent)JsonUtility.FromJson(serverMessage, eventType);
                        lock (receivedEvents) {
                            receivedEvents.Add(newEvent);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void SendSocketMessage(string clientMessage) {
        if (socketConnection == null) {
            return;
        }
        try {
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite) {
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent: " + clientMessage);
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void SendEvent(SocketEvent sEvent) {
        SendSocketMessage(sEvent.ToJson());
    }

    void Update() {
        lock (receivedEvents) {
            receivedEvents.ForEach(sEvent => sEvent.ExecuteHandler());
            receivedEvents.Clear();
        }
    }
}