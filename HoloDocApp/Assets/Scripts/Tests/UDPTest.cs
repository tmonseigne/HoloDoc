using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

public class UDPTest : MonoBehaviour {

	public bool enableLog = false;

	int nbOfTimes = 0;
	double cumulative = 0;

	Texture2D currentTexture;

	byte[] data;

	// read Thread
	Thread readThread;

	// udpclient object
	UdpClient client;
	IPEndPoint ipep;

	// port number
	public int port = 33333;

	// UDP packet store
	public string lastReceivedPacket = "";
	public string allReceivedPackets = ""; // this one has to be cleaned up from time to time

	// start from unity3d
	void Start() {
		Resolution res = CameraStream.Instance.Frame.Resolution;

		currentTexture = new Texture2D(res.width, res.height);
		data = new byte[0];
		// create thread for reading UDP messages
		readThread = new Thread(new ThreadStart(ReceiveData));
		ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 33333);
		readThread.IsBackground = true;
		readThread.Start();
	}

	// Unity Update Function
	void Update() {
		currentTexture.SetPixels32(CameraStream.Instance.Frame.Data);
		currentTexture.Apply(false);
		data = currentTexture.EncodeToJPG();

		// check button "s" to abort the read-thread
		if (Input.GetKeyDown("q"))
			stopThread();
	}

	// Unity Application Quit Function
	void OnApplicationQuit() {
		if (cumulative > 0 && nbOfTimes > 0)
			UnityEngine.Debug.Log("Execution time mean in milliseconds: " + cumulative / nbOfTimes);

		stopThread();
	}

	// Stop reading UDP messages
	private void stopThread() {
		if (readThread.IsAlive) {
			readThread.Abort();
		}

		client.Close();
	}

	// receive thread function
	private void ReceiveData() {
		while (true) {
			client = new UdpClient("localhost", 33333);
			client.Client.Blocking = true;
			Stopwatch sw = new Stopwatch();

			sw.Start();
			bool keeptime = false;
			try {
				int written = sendImageData(client);

				if (written > 0) { // succesfully written
					byte[] result = receiveData(client, ipep);

					if (result.Length > 0) {
						LOG("Number of received bytes: " + result.Length);

						LOG("Buffer beginning: x=" + BitConverter.ToInt32(result, 0) + " y=" + BitConverter.ToInt32(result, 4));
						keeptime = true;
					}
					else {
						throw new Exception("Not receiving data");
					}
				}
			}
			catch (Exception err) {
				LOG(err.ToString());
			}

			client.Close();

			sw.Stop();

			//Writing Execution Time in label
			if (keeptime) {
				nbOfTimes++;
				cumulative += sw.Elapsed.TotalMilliseconds;
				string ExecutionTimeTaken = string.Format("Minutes :{0} Seconds :{1}  Mili seconds :{2}", sw.Elapsed.Minutes,
					sw.Elapsed.Seconds, sw.Elapsed.TotalMilliseconds);
				LOG(ExecutionTimeTaken);
			}
		}
	}

	private byte[] receiveData(UdpClient socket, IPEndPoint endPoint) {
		byte[] dataLength = socket.Receive(ref endPoint);

		if (dataLength.Length == 4) {
			int nbBytes = BitConverter.ToInt32(dataLength, 0);

			byte[] response = new byte[nbBytes];
			int readed = 0;
			do {
				byte[] data = socket.Receive(ref endPoint);

				data.CopyTo(response, readed);
				readed += data.Length;
			} while (readed < nbBytes);


			return response;
		}
		else {
			throw new Exception("Not receiving the dataLength");
		}
	}

	private int sendImageData(UdpClient socket) {
		byte[] dataLength = BitConverter.GetBytes(data.Length);

		client.Send(dataLength, dataLength.Length);

		int toSend = data.Length;

		int sended = 0;

		if (toSend > 0) {
			int i = 0;
			int subSize = 1024;
			while (sended < toSend) {
				int extract = 0;
				int at = i * subSize;
				byte[] result = new byte[subSize];

				int j = 0;
				for (j = 0; j < subSize; ++j) {
					int index = j + at;
					if (index >= data.Length) {
						break;
					}

					result[j] = data[index];
				}

				extract = j;

				sended += extract;

				socket.Send(result, extract);
				i++;
			}
		}

		return sended;
	}

	// return the latest message
	public string getLatestPacket() {
		allReceivedPackets = "";
		return lastReceivedPacket;
	}

	void LOG(string messsage) {
		if (enableLog) {
			UnityEngine.Debug.Log(messsage);
		}
	}
}