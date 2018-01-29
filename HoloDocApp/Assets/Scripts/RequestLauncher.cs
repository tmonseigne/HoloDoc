using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RequestLauncher : MonoBehaviour {

    public static RequestLauncher Instance;

	// Use this for initialization
	void Awake () {
        if (Instance)
        {
            DestroyImmediate(this);
        }

        RequestLauncher.Instance = this;
    }

    public void CreateNewDocument (Texture2D texture)
    {
        StartCoroutine(newDocumentRequest(texture));
    }

    private IEnumerator newDocumentRequest(Texture2D ploup)
    {
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

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
