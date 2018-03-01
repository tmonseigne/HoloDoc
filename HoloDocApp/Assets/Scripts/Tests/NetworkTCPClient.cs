using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkTCPClient : NetworkClient {
    
    TcpClient client;
    public string host = "127.0.0.1";
    public int port = 44444;
    NetworkStream stream;

    protected internal override byte[] ReceiveResponse()
    {
        byte[] dataLength = new byte[4];
        if (stream.CanRead)
        {
            stream.Read(dataLength, 0, dataLength.Length);

            int nbBytes = BitConverter.ToInt32(dataLength, 0);

            if (nbBytes > 0)
            {
                byte[] result = new byte[nbBytes];

                stream.Read(result, 0, result.Length);
                return result;
            }
        }

        return null;
    }

    protected internal override int SendImageData()
    {
        byte[] dataLength = BitConverter.GetBytes(data.Length);
        stream.Write(dataLength, 0, dataLength.Length);

        stream.Write(data, 0, data.Length);

        return data.Length;
    }

    protected internal override void InitThread()
    {
        Debug.Log("Initialising TCP Client");

        client = new TcpClient(host, port);
        stream = client.GetStream();
    }

    protected internal override void CleanThreadBeforeStop()
    {
        if (client != null && stream != null)
        {
            client.Close();
            stream.Close();
        }
    }
}
