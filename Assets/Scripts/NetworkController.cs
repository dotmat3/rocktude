using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

public class NetworkController {

    public const char MSG_DELIMITER = '\n';
    public const int BUFFER_SIZE = 1024;
    public const string SERVER_ADDRESS = "skylion.zapto.org";
    public const int SERVER_PORT = 2042;

    private static NetworkController instance;

    public static NetworkController DefaultInstance {
        get {
            if (instance == null)
                instance = new NetworkController();
            return instance;
        }
    }

    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private List<SocketEvent> receivedEvents = new List<SocketEvent>();

    public void Connect() {
        try {
            socketConnection = new TcpClient(SERVER_ADDRESS, SERVER_PORT);
            Debug.Log("Socket connected");

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        } catch (Exception e) {
            Debug.Log("On client connect exception " + e);
        }
    }

    public void Disconnect() {
        if (socketConnection == null) return;

        Debug.Log("Socket disconnected");
        clientReceiveThread.Abort();
        socketConnection.Close();
    }

    public void HandleEvents() {
        lock (receivedEvents) {
            receivedEvents.ForEach(sEvent => sEvent.ExecuteHandler());
            receivedEvents.Clear();
        }
    }

    public void SendEvent(SocketEvent sEvent) {
        SendSocketMessage(sEvent.ToJson());
    }

    #region Handle messages
    private void ListenForData() {
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
        Debug.Log("Received message " + message);
        SocketEvent socketEvent = JsonUtility.FromJson<SocketEvent>(message);
        Type eventType = Type.GetType(socketEvent.type);
        SocketEvent newEvent = (SocketEvent) JsonUtility.FromJson(message, eventType);
        lock (receivedEvents) {
            receivedEvents.Add(newEvent);
        }
    }
    
    private void SendSocketMessage(string clientMessage) {
        Debug.Log("Sending socket message: " + clientMessage);
        if (socketConnection == null) {
            return;
        }

        try {
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite) {
                clientMessage += MSG_DELIMITER;
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Sent message " + clientMessage);
            }
            else {
                Debug.Log("Socket stream cannot write at the moment");
            }
        } catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }
    #endregion

    #region Send requests
    public static IEnumerator SendGetRequest(string url, Action<UnityWebRequest> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();
            callback(request);
        }
    }

    public static IEnumerator SendPostRequest(string url, string data, Action<UnityWebRequest> callback) {
        using (UnityWebRequest request = UnityWebRequest.Post(url, data)) {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            callback(request);
        }
    }

    public static IEnumerator SendPutRequest(string url, string data, Action<UnityWebRequest> callback) {
        using (UnityWebRequest request = UnityWebRequest.Put(url, data)) {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            callback(request);
        }
    }

    public static IEnumerator SendGetTextureRequest(string url, Action<UnityWebRequest> callback) {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url)) {
            yield return request.SendWebRequest();
            callback(request);
        }
    }
    #endregion
}