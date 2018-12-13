using System.Collections.Generic;
using LitJson;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.IO;
using UnityEngine.Networking;
using System.Threading;
using WebSocketSharp;


namespace P2P
{
    public class WebsocketPeer : IDisposable
    {
        WebSocket websocket;
        bool isWebsocketRunning = false;
        bool websocketClosed = false;
        float timeWebsocketSftarted;
        public string myId;

        public delegate void OnConnectionCallback(string peer);
        public event OnConnectionCallback OnConnection;

        public delegate void OnDisconnectionCallback(string peer);
        public event OnDisconnectionCallback OnDisconnection;

        public delegate void OnBytesFromPeerCallback(string peer, byte[] bytes);
        public event OnBytesFromPeerCallback OnBytesFromPeer;

        public delegate void OnTextFromPeerCallback(string peer, string text);
        public event OnTextFromPeerCallback OnTextFromPeer;

        public delegate void GetIDCallback(string id);
        public event GetIDCallback OnGetID;

        long createTime;

        string room;
        public DispatchQueue dispatchQueue;
        // Use this for initialization
        public WebsocketPeer(string websocketUrl, string room)
        {
            this.room = room;
            createTime = GetTimeInMillis();
            myId = System.Guid.NewGuid().ToString();
            dispatchQueue = new DispatchQueue();

            dispatchQueue.async(() =>
            {
                websocket = new WebSocket(websocketUrl);
                websocket.OnOpen += Websocket_OnOpen;
                websocket.OnError += Websocket_OnError;
                websocket.OnMessage += Websocket_OnMessage;
                websocket.OnClose += Websocket_OnClose;
                Log("made");
            });

            


        }

        private void Websocket_OnClose(object sender, CloseEventArgs e)
        {
            // closed but we didn't want it to, try to reopen
            isWebsocketRunning = false;
            websocketClosed = true;
            Log("closed websocket");
            if (!disconnected)
            {
                Log("we don't want it to be closed yet, trying to reopen websocket");

                dispatchQueue.async(() =>
                {
                    Open();
                });

            }
        }

        private void Websocket_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText)
            {
                Log("got websocket message " + e.Data);
                websocketMessagesReceived.Enqueue(e.Data);
            }
            else
            {
                Log("got " + e.IsBinary + " = isBinary message but we don't support that right now");
            }
        }

        private void Websocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Log("got websocket error " + e.Exception);
        }

        private void Websocket_OnOpen(object sender, EventArgs e)
        {
            Log("opened websocket");
            isWebsocketRunning = true;
            timeWebsocketStarted = GetTimeInMillis();
            timeSentLastPing = 0;
        }
        
        public void Open()
        {
            dispatchQueue.async(() =>
            {
                websocket.Connect();
                Log("opening");
            });
        }
        
        public void Send(string peer, byte[] data)
        {
            dispatchQueue.async(() =>
            {
                JsonData messageData = new JsonData();
                messageData["senderId"] = myId;
                messageData["receiverId"] = peer;
                messageData["room"] = room;
                messageData["type"] = "bin";
                messageData["data"] = Convert.ToBase64String(data);
                try
                {
                    websocket.Send(messageData.ToJson());
                }
                catch (Exception e)
                {
                    Log("failed to binary message because: " + e.Message + " " + e.ToString());
                }
            });
        }
        public void Send(string peer, string text)
        {
            dispatchQueue.async(() =>
            {
                JsonData messageData = new JsonData();
                messageData["senderId"] = myId;
                messageData["receiverId"] = peer;
                messageData["room"] = room;
                messageData["type"] = "txt";
                messageData["data"] = text;
                try
                {
                    websocket.Send(messageData.ToJson());
                }
                catch (Exception e)
                {
                    Log("failed to send text message because: " + e.Message + " " + e.ToString());
                }
            });
        }


        bool disconnected = false;
        public void Disconnect()
        {
            dispatchQueue.async(() =>
            {
                if (!disconnected)
                {
                    disconnected = true;
                    JsonData messageData = new JsonData();
                    messageData["senderId"] = myId;
                    messageData["receiverId"] = "all";
                    messageData["room"] = room;
                    messageData["type"] = "disconnect";
                    try
                    {
                        if (isWebsocketRunning)
                        {
                            websocket.Send(messageData.ToJson());
                        }
                        else
                        {
                            Log("not sending disconnect message because websocket isn't open");
                        }
                    }
                    catch (Exception e)
                    {
                        Log("failed to send disconnect message because: " + e.Message + " " + e.ToString());
                    }
                    Update();

                    Log("disposing of websocket and other thread");
                    websocket.Close();
                    dispatchQueue.Dispose();
                    Log("disposed of websocket and other thread");

                    foreach (string peer in peers)
                    {
                        if (OnDisconnection != null)
                        {
                            OnDisconnection(peer);
                        }
                    }
                }
            });
        }

        public HashSet<string> peers = new HashSet<string>();
        public ConcurrentQueue<string> websocketMessagesReceived = new ConcurrentQueue<string>();


        void Log(string text)
        {
            UnityEngine.Debug.Log(text);
            //System.Console.WriteLine(text);
        }
        
        long timeWebsocketStarted;
        long millisToDoFastPing = 10000;
        long slowPingRateInMillis = 10000;
        long fastPingRateInMillis = 1500;
        long timeSentLastPing;


        // From https://stackoverflow.com/questions/4016483/get-time-in-milliseconds-using-c-sharp
        long GetTimeInMillis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        bool firstLoop = true;
        bool prevIsWebsocketRunning = false;
        // Update is called once per frame
        public void Update()
        {
            if (firstLoop)
            {
                firstLoop = false;
                if (OnGetID != null)
                {
                    OnGetID(myId);
                }
            }
            if (websocketClosed)
            {
                if (OnDisconnection != null)
                {
                    foreach (String peer in peers)
                    {
                        OnDisconnection(peer);
                    }
                }
                websocketClosed = false;
            }
            if (!isWebsocketRunning)
            {
                return;
            }

            if (GetTimeInMillis() - timeWebsocketStarted < millisToDoFastPing)
            {
                if (GetTimeInMillis() - timeSentLastPing > fastPingRateInMillis)
                {
                    SendPing();
                    timeSentLastPing = GetTimeInMillis();
                }
            }
            else
            {
                if (GetTimeInMillis() - timeSentLastPing > slowPingRateInMillis)
                {
                    SendPing();
                    timeSentLastPing = GetTimeInMillis();
                }
            }
            string message;
            while (websocketMessagesReceived.TryDequeue(out message))
            {
                ParseMessage(message);
            }
        }


        void ParseMessage(string message)
        {
            try
            {
                JsonData data = JsonMapper.ToObject(message);
                string senderId = data["senderId"].ToString();
                if (senderId == myId)
                {
                    //Debug.Log("from me: " + message);
                    return;
                }

                string senderRoom = data["room"].ToString();

                if (senderRoom != room)
                {
                    // Log("sent to another room, not this one: " + message);
                    return;
                }

                // "all" means broadcast to everyone in the room (useful for things like ping and disconnect)
                string receiverId = data["receiverId"].ToString();

                if (receiverId != "all" && receiverId != myId)
                {
                    // Log("to someone else: " + message);
                    return;
                }


                if (!peers.Contains(senderId))
                {
                    if (OnConnection != null)
                    {
                        OnConnection(senderId);
                    }
                    peers.Add(senderId);
                }

                string messageType = data["type"].ToString();

                if (messageType == "ping")
                {

                }
                else if (messageType == "bin")
                {
                    byte[] bytes = Convert.FromBase64String(data["data"].ToString());
                    if (OnBytesFromPeer != null)
                    {
                        OnBytesFromPeer(senderId, bytes);
                    }
                }
                else if (messageType == "txt")
                {
                    string text = data["data"].ToString();
                    if (OnTextFromPeer != null)
                    {
                        OnTextFromPeer(senderId, text);
                    }
                }
                else if (messageType == "disconnect")
                {
                    if (peers.Contains(senderId))
                    {
                        peers.Remove(senderId);
                        if (OnDisconnection != null)
                        {
                            OnDisconnection(senderId);
                        }
                    }
                }
                else
                {
                    Log("unknown message type: " + messageType);
                }
            }
            catch (Exception e)
            {
                Log("failed to parse " + message + " with error " + e.Message + " " + e.StackTrace);
            }
        }

        void SendPing()
        {
            dispatchQueue.async(() =>
            {
                JsonData pingData = new JsonData();
                pingData["senderId"] = myId;
                pingData["receiverId"] = "all";
                pingData["type"] = "ping";
                pingData["room"] = room;
                try
                {
                    websocket.Send(pingData.ToJson());
                }
                catch (Exception e)
                {
                    Log("failed to send ping because: " + e.Message + " " + e.ToString());
                }
            });
        }

        object cleanupLock = new object();
        bool isCleanedUp = false;
        void Cleanup()
        {
            lock (cleanupLock)
            {
                if (!isCleanedUp)
                {
                    isCleanedUp = true;
                    Disconnect();

                }
            }
        }


        ~WebsocketPeer()
        {
            Cleanup();
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}