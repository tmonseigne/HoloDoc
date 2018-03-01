using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkUDPClient : NetworkClient
{
    public string host = "127.0.0.1";
    public int port = 33333;

    UdpClient client;
    IPEndPoint ipep;

    protected internal override void CleanThreadBeforeStop()
    {
        if (client != null)
        {
            client.Close();
        }
    }

    protected internal override void InitThread()
    {
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33333);
    }

    protected override void NetworkLoopBegin ()
    {
        client = new UdpClient("localhost", 33333);
        client.Client.Blocking = true;
    }

    protected override void NetworkLoopEnd()
    {
        if (client != null) {
            client.Close();
        }
    }

    protected internal override byte[] ReceiveResponse()
    {
        byte[] dataLength = client.Receive(ref ipep);

        if (dataLength.Length == 4)
        {
            int nbBytes = BitConverter.ToInt32(dataLength, 0);

            byte[] response = new byte[nbBytes];
            int readed = 0;
            do
            {
                byte[] data = client.Receive(ref ipep);

                data.CopyTo(response, readed);
                readed += data.Length;
            } while (readed < nbBytes);


            return response;
        }

        return null;
    }

    protected internal override int SendImageData()
    {
        byte[] dataLength = BitConverter.GetBytes(data.Length);

        client.Send(dataLength, dataLength.Length);

        int toSend = data.Length;

        int sended = 0;

        if (toSend > 0)
        {
            int i = 0;
            int subSize = 1024;
            while (sended < toSend)
            {
                int extract = 0;
                int at = i * subSize;
                byte[] result = new byte[subSize];

                int j = 0;
                for (j = 0; j < subSize; ++j)
                {
                    int index = j + at;
                    if (index >= data.Length)
                    {
                        break;
                    }

                    result[j] = data[index];
                }

                extract = j;

                sended += extract;

                client.Send(result, extract);
                i++;
            }
        }

        return sended;
    }
}
