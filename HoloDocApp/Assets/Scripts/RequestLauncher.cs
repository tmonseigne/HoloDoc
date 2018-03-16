using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

using HoloToolkit.Unity;
using System.Text;

public class RequestLauncher : Singleton<RequestLauncher> {
    public abstract class RequestData
    {
        public virtual string toJSON()
        {
            return JsonUtility.ToJson(this);
        }
    }


    #region Answers classes

    [System.Serializable]
    public class RequestAnswerDocument
    {
        public string name;
        public string label;
        public string desc;
        public string author;
        public string date;
        public string path;
        public string image;
        public string error;

        public CameraFrame CameraFrameFromBase64()
        {
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(Convert.FromBase64String(image));

            CameraFrame frame = new CameraFrame(new Resolution { width = tex.width, height = tex.height }, tex.GetPixels32());
            DestroyImmediate(tex);

            return frame;
        }
    }

    [System.Serializable]
    public class RequestAnswerConnected
    {
        public bool connected;
        public string error;
    }

    [System.Serializable]
    public class RequestAnswerSimple
    {
        public string error;
    }

    #endregion


    #region Document requests

    public void MatchOrCreateDocument (CameraFrame frame, OnRequestResponse<RequestAnswerDocument> callback)
    {
        MatchOrCreateRequestData data = new MatchOrCreateRequestData();
        data.image = frame;
        StartCoroutine(launchRocket<RequestAnswerDocument>(data, "/document/matchorcreate", callback));
    }

    public void UpdateDocument(DocumentProperties properties, OnRequestResponse<UpdateRequestData> callback)
    {
        UpdateRequestData data = new UpdateRequestData();
        data.label = properties.Label;
        data.desc = properties.Description;
        data.author = properties.Author;
        data.date = properties.Date;

        StartCoroutine(launchRocket<UpdateRequestData>(data, "/document/update", callback));
    }

    public class MatchOrCreateRequestData : RequestData
    {
        public CameraFrame image;

        private string CameraFrameToJson(CameraFrame frame)
        {
            Texture2D tex = new Texture2D(frame.Resolution.width, frame.Resolution.height);
            tex.SetPixels32(frame.Data);

            string json = BitConverter.ToString(tex.EncodeToJPG()).Replace("-", "");

            Destroy(tex);

            return json;
        }

        public override string toJSON()
        {
            return "{ \"image\" : \"" + CameraFrameToJson(image) + "\" }";
        }
    }

    public class UpdateRequestData : RequestData
    {
        public string name;
        public string label;
        public string desc;
        public string author;
        public string date;
    }

    #endregion

    #region Link requests

    public void CreateLink(string firstId, string secondId, OnRequestResponse<RequestAnswerSimple> callback)
    {
        LinkRequestData data = new LinkRequestData();
        data.firstId = firstId;
        data.secondId = secondId;

        StartCoroutine(launchRocket<RequestAnswerSimple>(data, "/link/create", callback));
    }

    public void RemoveLink(string firstId, string secondId, OnRequestResponse<RequestAnswerSimple> callback)
    {
        LinkRequestData data = new LinkRequestData();
        data.firstId = firstId;
        data.secondId = secondId;

        StartCoroutine(launchRocket<RequestAnswerSimple>(data, "/link/remove", callback));
    }

    public void AreConnected(string firstId, string secondId, OnRequestResponse<RequestAnswerConnected> callback)
    {
        LinkRequestData data = new LinkRequestData();
        data.firstId = firstId;
        data.secondId = secondId;

        StartCoroutine(launchRocket<RequestAnswerConnected>(data, "/document/connected", callback));
    }
    #endregion

    public class LinkRequestData : RequestData
    {
        public string firstId;
        public string secondId;
    }


    public delegate void OnRequestResponse<T>(T item, bool success);

    IEnumerator launchRocket <T>(RequestData data, string request, OnRequestResponse<T> onResponse)
    {
        string payload = data.toJSON();

        string url = "http://localhost:8080" + request;
        string method = UnityWebRequest.kHttpVerbPOST;
        UploadHandler uploader = new UploadHandlerRaw(Encoding.ASCII.GetBytes(payload));
        uploader.contentType = "custom/content-type";

        DownloadHandler downloader = new DownloadHandlerBuffer();

        UnityWebRequest www = new UnityWebRequest(url, method, downloader, uploader);

        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
        T answer = JsonUtility.FromJson<T>(www.downloadHandler.text);
        if (onResponse != null)
        {
            onResponse.Invoke(answer, !(www.isNetworkError || www.isHttpError));
        }
    }
}
