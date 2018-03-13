using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

using HoloToolkit.Unity;

public class RequestLauncher : Singleton<RequestLauncher> {

	public delegate void OnDetectRequestCallback(Vector2Int[] points, int nbDocuments);
	public delegate void OnMatchOrCreateDocumentCallback(DocumentProperties properties);

	public void CreateNewDocument(CameraFrame frame) {
		StartCoroutine(NewDocumentRequest(frame));
	}

	private IEnumerator NewDocumentRequest(CameraFrame frame) {
		Texture2D texture = new Texture2D(frame.Resolution.width, frame.Resolution.height);
		texture.SetPixels32(frame.Data);
		texture.Apply(true);
		byte[] payload = texture.EncodeToPNG();
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

		// Avoid memory leak
		Destroy(texture);
	}

	public void MatchOrCreateDocument(DocumentProperties properties, OnMatchOrCreateDocumentCallback callback) {
		StartCoroutine(MatchOrCreateDocumentRequest(properties, callback));
	}

	// TODO: This function must send the current image stored in properties.Photo and match/create it.
	// During match or create, the image should be cropped and wrapped on the server.
	// Corners (for cropping) are available in properties.Corners
	IEnumerator MatchOrCreateDocumentRequest(DocumentProperties properties, OnMatchOrCreateDocumentCallback callback) {
		throw new NotImplementedException();
	}

	public void CreateLink(int[] ids) {
		StartCoroutine(CreateLinkRequest(ids));
	}

	// TODO
	IEnumerator CreateLinkRequest(int[] ids) {
		throw new NotImplementedException();
	}

	public void UpdateDocumentInformations(DocumentProperties properties) {
		StartCoroutine(UpdateDocumentInformationsRequest(properties));
	}

	IEnumerator UpdateDocumentInformationsRequest(DocumentProperties properties) {
		string url = "http://localhost:8080/document/update";
		string data = JsonUtility.ToJson(properties);
		Debug.Log(data);
		using (UnityWebRequest www = UnityWebRequest.Post(url, data)) {
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError) {
				Debug.Log(www.error);
			}
			else {
				Debug.Log("Properties upload complete!");
			}
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
}