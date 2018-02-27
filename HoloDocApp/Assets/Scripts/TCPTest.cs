using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System;

public class TCPTest : MonoBehaviour {

	public bool enableLog = false;

	int nbOfTimes = 0;
	double cumulative = 0;

	TcpClient client;
	public string host = "127.0.0.1";
	public int port = 44444;
	NetworkStream stream;

	Texture2D currentTexture;
	byte[] data;

	Thread thread;


	// Use this for initialization
	void Start () {
		Resolution res = CameraStream.Instance.Resolution;
		currentTexture = new Texture2D (res.width, res.height);


	
		thread = new Thread(new ThreadStart(ReceiveData));
		thread.IsBackground = true;
		thread.Start();
	}

	private void ReceiveData()
	{
		client = new TcpClient (host, port);
		stream = client.GetStream ();
		while (true)
		{

			Stopwatch sw = new Stopwatch();

			sw.Start(); 
			bool keeptime = false;
			try
			{
				int written = sendImageData(stream);
				LOG("Number of written bytes: " + written);

				if (written > 0) { // succesfully written
					byte[] result = receiveData(stream);

					if (result.Length > 0) {
						LOG("Number of received bytes: " + result.Length);

						LOG ("Buffer beginning: x=" + BitConverter.ToInt32 (result, 0) + " y=" + BitConverter.ToInt32 (result, 4));
						keeptime = true;
					} else {
						throw new Exception ("Not receiving data");
					}
				} 
			}
			catch (Exception err)
			{
				LOG(err.ToString());
			}

			sw.Stop();

			//Writing Execution Time in label
			if (keeptime) {
				nbOfTimes++;
				cumulative += sw.Elapsed.TotalMilliseconds;
				string ExecutionTimeTaken = string.Format("Minutes :{0} Seconds :{1}  Mili seconds :{2}",sw.Elapsed.Minutes,sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds);
				LOG (ExecutionTimeTaken);
			}
		}

		client.Close ();
		stream.Close ();
	}

	private byte[] receiveData (NetworkStream stream) {
		byte[] dataLength = new byte[4];
		if (stream.CanRead) {
			stream.Read (dataLength, 0, dataLength.Length);

			int nbBytes = BitConverter.ToInt32 (dataLength, 0);

			if (nbBytes > 0) {
				byte[] result = new byte[nbBytes];

				stream.Read (result, 0, result.Length);
				return result;

			} 
		}

		return new byte[0];
	}

	private int sendImageData (NetworkStream stream) {

		byte[] dataLength = BitConverter.GetBytes (data.Length);
		LOG(""+dataLength.Length);
		stream.Write (dataLength, 0, dataLength.Length);

		stream.Write (data, 0, data.Length);

		return data.Length;
	}

	// Update is called once per frame
	void Update () {
		currentTexture.SetPixels32 (CameraStream.Instance.Frame.Data);
		currentTexture.Apply (false);
		data = currentTexture.EncodeToJPG ();
	}

	// Stop reading UDP messages
	private void stopThread()
	{
		if (thread != null && thread.IsAlive)
		{
			thread.Abort();
		}
	}

	private void OnApplicationQuit() {
		if (cumulative > 0 && nbOfTimes > 0)
			UnityEngine.Debug.Log ("Execution time mean in milliseconds: " + cumulative / nbOfTimes);

		if (stream != null)
			stream.Close ();
		stopThread ();        
		if (client != null)
			client.Close();  
	}

	void LOG(string messsage)
	{
		if (enableLog)
			UnityEngine.Debug.Log(messsage);
	}
}
