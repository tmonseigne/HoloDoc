using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class NetworkClient : MonoBehaviour {

    public bool enableLog = false;

    public delegate void OnDataEventHandler(object sender, OnDataEventArgs e);
    public event OnDataEventHandler OnDataEvent;

    private Texture2D currentTexture;
    protected byte[] data;

    protected Thread thread;


    public void InitClient()
    {
        Debug.Log("Starting a Client");
        Resolution res = CameraStream.Instance.Frame.Resolution;
        currentTexture = new Texture2D(res.width, res.height);

        thread = new Thread(new ThreadStart(ReceiveData));
        thread.IsBackground = true;
    }

    // Use this for initialization
    void Start () {
        
    }

    protected virtual void NetworkLoopBegin() {}

    protected virtual void NetworkLoopEnd() {}

    protected void ReceiveData ()
    {
        InitThread();
        while (true)
        {
            NetworkLoopBegin();
            try
            {
                int written = SendImageData();
                LOG("Number of written bytes: " + written);

                if (written > 0)
                { // succesfully written
                    byte[] result = ReceiveResponse();

                    if (result != null)
                    {
                        LOG("Number of received bytes: " + result.Length);
                        int[] coordinates = toInt32Array(result);

                        if (coordinates != null)
                        {
                            InvokeOnDataEvent(new OnDataEventArgs(coordinates));
                        } 
                        else
                        {
                            throw new Exception("The data is not complete");
                        }
                    }
                    else
                    {
                        throw new Exception("Not receiving data");
                    }
                }
            }
            catch (Exception err)
            {
                LOG(err.ToString());
            }

            NetworkLoopEnd();
        }
    }

    private int[] toInt32Array (byte[] data)
    {
        if (data != null && (data.Length % 4) == 0)
        {
            int[] result = new int[data.Length / 4];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = BitConverter.ToInt32(data, i * 4);
            }
        }

        return null;
    }

    protected internal abstract byte[] ReceiveResponse();
    protected internal abstract int SendImageData();
    protected internal abstract void InitThread();
    protected internal abstract void CleanThreadBeforeStop();

    public void StartClient()
    {
        if (thread != null)
        {
            Debug.Log("Starting client");
            thread.Start();
        }
    }

    public void StopClient()
    {
        if (thread != null && thread.IsAlive)
        {
            Debug.Log("Stoping client");
            CleanThreadBeforeStop();
            thread.Abort();
        }
    }

    // Update is called once per frame
    void Update () {
        Debug.Log("Updating client");
        currentTexture.SetPixels32(CameraStream.Instance.Frame.Data);
        currentTexture.Apply(false);
        data = currentTexture.EncodeToJPG();
    }

    protected void LOG(string messsage)
    {
        if (enableLog)
        {
            Debug.Log(messsage);
        }
    }

    private void InvokeOnDataEvent (OnDataEventArgs args)
    {
        if (OnDataEvent != null)
        {
            OnDataEvent.Invoke(this, args);
        }
    }

    private void OnDestroy()
    {
        this.StopClient();
    }
}
