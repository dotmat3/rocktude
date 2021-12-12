using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class NetworkController : MonoBehaviour {
    public static char MSG_DELIMITER = '\n';
    public static int BUFFER_SIZE = 1024;

    public string serverAddress = "127.0.0.1";
    public int serverPort = 2004;

    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private List<SocketEvent> receivedEvents = new List<SocketEvent>();

    void Start() {
        ConnectToTcpServer();
    }

    private void ConnectToTcpServer() {
        try {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        } catch (Exception e) {
            Debug.Log("On client connect exception " + e);
        }
    }

    private void ListenForData() {
        socketConnection = new TcpClient(serverAddress, serverPort);
        
        try {
            byte[] bytes = new byte[BUFFER_SIZE];
            string msgBuffer = string.Empty;
            while (true) {
                using (NetworkStream stream = socketConnection.GetStream()) {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
                        byte[] incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        string serverMessage = Encoding.ASCII.GetString(incomingData);

                        int startMessage = 0;
                        msgBuffer += serverMessage;

                        for (int i = 0; i < msgBuffer.Length; i++) {
                            if (msgBuffer[i] == MSG_DELIMITER) {
                                HandleMessage(msgBuffer.Substring(startMessage, i - startMessage));
                                startMessage = i + 1;
                            }
                        }

                        if (msgBuffer[msgBuffer.Length - 1] != MSG_DELIMITER)
                            msgBuffer = msgBuffer.Substring(startMessage);
                        else
                            msgBuffer = string.Empty;
                        
                    }
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void HandleMessage(string message) {
        SocketEvent socketEvent = JsonUtility.FromJson<SocketEvent>(message);
        Type eventType = Type.GetType(socketEvent.type);
        SocketEvent newEvent = (SocketEvent) JsonUtility.FromJson(message, eventType);
        lock (receivedEvents) {
            receivedEvents.Add(newEvent);
        }
    }

    private void SendSocketMessage(string clientMessage) {
        if (socketConnection == null)
            return;

        try {
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite) {
                clientMessage += MSG_DELIMITER;
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            }
        } catch (SocketException socketException) {
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