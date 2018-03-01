using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDocumentDetector : MonoBehaviour {

    public bool UseNetworkDetection = false;
    public bool UseUDP = true;


    private NetworkTCPClient tcpClient;
    private NetworkUDPClient udpClient;

	// Use this for initialization
	void Start () {
        tcpClient = this.gameObject.GetComponent<NetworkTCPClient>();
        tcpClient.InitClient();

        udpClient = this.gameObject.GetComponent<NetworkUDPClient>();
        udpClient.InitClient();

        if (UseNetworkDetection)
        {
            if (UseUDP)
            {
                udpClient.StartClient();
            }
            else
            {
                tcpClient.StartClient();
            }
        } 
        else
        {
            Debug.Log("Network is not used for document detection, please use the DLL ;)");
        }
    }
	
    public void UseUDPClient ()
    {
        if (!UseUDP)
        {
            tcpClient.StopClient();
            udpClient.StartClient();
            UseUDP = true;
        }
    }

    public void UseTCPClient ()
    {
        if (UseUDP)
        {
            udpClient.StopClient();
            tcpClient.StartClient();
            UseUDP = false;
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
