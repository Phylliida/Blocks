using P2P;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityPeer : MonoBehaviour {

    WebsocketPeer websocketPeer;

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

    public string wsUrl = "ws://sample-bean.herokuapp.com";
    public string room = "testRoom";

    void Start () {
        websocketPeer = new WebsocketPeer(wsUrl, room);
        websocketPeer.OnBytesFromPeer += Peer_OnBytesFromPeer;
        websocketPeer.OnConnection += Peer_OnConnection;
        websocketPeer.OnDisconnection += Peer_OnDisconnection;
        websocketPeer.OnGetID += Peer_OnGetID;
        websocketPeer.OnTextFromPeer += Peer_OnTextFromPeer;
        websocketPeer.Open();
	}

    void Peer_OnGetID(string id)
    {
        if (OnGetID != null)
        {
            OnGetID(id);
        }
    }

    void Peer_OnConnection(string peer)
    {
        if (OnConnection != null)
        {
            OnConnection(peer);
        }
    }

    void Peer_OnDisconnection(string peer)
    {
        if (OnDisconnection != null)
        {
            OnDisconnection(peer);
        }
    }

    void Peer_OnTextFromPeer(string peer, string text)
    {
        if (OnTextFromPeer != null)
        {
            OnTextFromPeer(peer, text);
        }
    }

    void Peer_OnBytesFromPeer(string peer, byte[] bytes)
    {
        if (OnBytesFromPeer != null)
        {
            OnBytesFromPeer(peer, bytes);
        }
    }

    public void Send(string peerId, byte[] data)
    {
        websocketPeer.Send(peerId, data);
    }
    public void Send(string peerId, string text)
    {
        websocketPeer.Send(peerId, text);
    }

    private void OnDestroy()
    {
        websocketPeer.Disconnect();
        websocketPeer.Dispose(); // it is fine to call this more then once
    }
    void OnApplicationQuit()
    {
        websocketPeer.Disconnect();
        websocketPeer.Dispose();
    }

    void Update () {
        websocketPeer.Update();
	}
}
