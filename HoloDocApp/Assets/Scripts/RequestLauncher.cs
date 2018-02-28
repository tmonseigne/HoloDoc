using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RequestLauncher : MonoBehaviour {

	public static RequestLauncher Instance;

	public delegate void OnDetectRequestCallback(Vector2Int[] points, int nbDocuments);

	// Use this for initialization
	void Awake() {
		if (Instance) {
			DestroyImmediate(this);
		}

		RequestLauncher.Instance = this;
	}

	public void CreateNewDocument(Texture2D texture) {
		StartCoroutine(NewDocumentRequest(texture));
	}

	private IEnumerator NewDocumentRequest(Texture2D ploup) {
		//byte[] payload = ploup.ToArray();
		//byte[] payload = tex.GetRawTextureData();
		byte[] payload = ploup.EncodeToPNG();
		Debug.Log("Display payload : " + BitConverter.ToString(payload));

		string url = "http://localhost:8080/document/new";
		string method = UnityWebRequest.kHttpVerbPOST;
		UploadHandler uploader = new UploadHandlerRaw(payload);
		uploader.contentType = "custom/content-type";

		DownloadHandler downloader = new DownloadHandlerBuffer();
		UnityWebRequest www;
		www = new UnityWebRequest(url, method, downloader, uploader);

		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError) {
			Debug.Log(www.error);
		}
		else {
			// Show results as text
			Debug.Log(www.downloadHandler.text);

			// Or retrieve results as binary data
			byte[] results = www.downloadHandler.data;
		}
	}

	public void DetectDocuments(Texture2D texture, OnDetectRequestCallback callback) {
		DetectDocumentRequest(texture, callback);
	}

	private void DetectDocumentRequest(Texture2D ploup, OnDetectRequestCallback callback) {
		//byte[] payload = ploup.ToArray();
		//byte[] payload = tex.GetRawTextureData();

		DateTime start = DateTime.Now;

		byte[] payload = ploup.EncodeToJPG();
		Debug.Log("Display payload : " + BitConverter.ToString(payload));

		string url = "http://localhost:8080/document/detect";
		string method = UnityWebRequest.kHttpVerbPOST;
		UploadHandler uploader = new UploadHandlerRaw(payload);
		uploader.contentType = "custom/content-type";

		DownloadHandler downloader = new DownloadHandlerBuffer();
		UnityWebRequest www;
		www = new UnityWebRequest(url, method, downloader, uploader);

		Debug.Log("ici");
		UnityWebRequestAsyncOperation request = www.SendWebRequest();

		request.completed += delegate(AsyncOperation op) {
			if (www.isNetworkError || www.isHttpError) {
				Debug.Log(www.error);
			}
			else {
				// Show results as text
				Debug.Log(www.downloadHandler.text);

				// Or retrieve results as binary data
				byte[] results = www.downloadHandler.data;
				DateTime end = DateTime.Now;

				Debug.Log((end.Ticks - start.Ticks) / 10000.0f);
			}
		};
	}

	// Update is called once per frame
	void Update() {}
}