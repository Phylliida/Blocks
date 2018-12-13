using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using P2P;
public class ExampleUsage : MonoBehaviour {

    public UnityPeer unityPeer;

    // Use this for initialization
    void Start () {
        // += just means add a callback, so when unity peer gets its id it calls our Peer_OnGetID Function
        // Note that all of these callbacks will be called on the same thread as Update() so you don't need to worry about threading
        unityPeer.OnGetID += Peer_OnGetID;
        unityPeer.OnConnection += Peer_OnConnection;
        unityPeer.OnDisconnection += Peer_OnDisconnection;
        unityPeer.OnTextFromPeer += Peer_OnTextFromPeer;
        unityPeer.OnBytesFromPeer += Peer_OnBytesFromPeer;		
	}


    void Peer_OnGetID(string id)
    {
        Debug.Log("my id is " + id);
    }

    void Peer_OnConnection(string peerId)
    {
        Debug.Log(peerId + " connected");
        unityPeer.Send(peerId, "hello " + peerId);
    }

    private void Peer_OnDisconnection(string peerId)
    {
        Debug.Log(peerId + " disconnected");
    }

    void Peer_OnTextFromPeer(string peerId, string text)
    {
        Debug.Log(peerId + " sent " + text);
    }

    void Peer_OnBytesFromPeer(string peerId, byte[] bytes)
    {
        Debug.Log(peerId + " sent " + bytes.Length + " bytes");
    }
}
