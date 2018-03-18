using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

using HoloToolkit.Unity;
using System.Text;

public class RequestLauncher : Singleton<RequestLauncher> {

    public abstract class RequestData
    {
        public virtual string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }
    }

    #region Answers classes

    [Serializable]
    public class RequestAnswerDocument : RequestAnswerSimple
    {
        public string Id;
        public string Name;
        public string Label;
        public string Desc;
        public string Author;
        public string Date;
        public string Path;
        public string Image;

        public CameraFrame CameraFrameFromBase64()
        {
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(Convert.FromBase64String(Image));

            CameraFrame frame = new CameraFrame(new Resolution { width = tex.width, height = tex.height }, tex.GetPixels32());
            DestroyImmediate(tex);

            return frame;
        }
    }

    [Serializable]
    public class RequestAnswerConnected : RequestAnswerSimple
    {
        public bool Connected;
    }

    [Serializable]
    public class RequestAnswerSimple
    {
        public string Error;
    }

    [Serializable]
    public class RequestAnswerColor : RequestAnswerSimple
    {
        public int R;
        public int G;
        public int B;
    }

    #endregion

    #region Document requests

    public void MatchOrCreateDocument(CameraFrame frame, OnRequestResponse<RequestAnswerDocument> callback)
    {
		MatchOrCreateRequestData data = new MatchOrCreateRequestData {
			image = frame
		};
		StartCoroutine(LaunchRocket<RequestAnswerDocument>(data, "/document/matchorcreate", callback));
    }

    public void UpdateDocument(DocumentProperties properties, OnRequestResponse<UpdateRequestData> callback)
    {
		UpdateRequestData data = new UpdateRequestData {
			label = properties.Label,
			desc = properties.Description,
			author = properties.Author,
			date = properties.Date
		};

		StartCoroutine(LaunchRocket<UpdateRequestData>(data, "/document/update", callback));
    }

    public void UpdateDocumentPhoto(string documentId, CameraFrame frame, OnRequestResponse<RequestAnswerDocument> callback)
    {
        UpdatePhotoRequestData data = new UpdatePhotoRequestData
        {
            image = frame,
            id = documentId
        };

        StartCoroutine(LaunchRocket<RequestAnswerDocument>(data, "/document/updatephoto", callback));
    }

    public void SetBackgroundColor(Color32 color, OnRequestResponse<BackGroundColorRequestData> callback)
    {
        BackGroundColorRequestData data = new BackGroundColorRequestData
        {
            R = color.r,
            G = color.g,
            B = color.b
        };

        StartCoroutine(LaunchRocket<BackGroundColorRequestData>(data, "/document/background", callback));
    }

    public class MatchOrCreateRequestData : RequestData
    {
        public CameraFrame image;

        protected string CameraFrameToJson(CameraFrame frame)
        {
            Texture2D tex = new Texture2D(frame.Resolution.width, frame.Resolution.height);
            tex.SetPixels32(frame.Data);

            string json = BitConverter.ToString(tex.EncodeToJPG()).Replace("-", "");

            Destroy(tex);

            return json;
        }

        public override string ToJSON()
        {
            return "{ \"image\" : \"" + CameraFrameToJson(image) + "\" }";
        }
    }

    public class UpdatePhotoRequestData : MatchOrCreateRequestData
    {
        public string id;

        public override string ToJSON()
        {
            return "{ \"id\": \"" + id +"\", \"image\" : \"" + CameraFrameToJson(image) + "\" }";
        }
    }

    public class UpdateRequestData : RequestData
    {
        public string id;
        public string name;
        public string label;
        public string desc;
        public string author;
        public string date;
    }

    public class BackGroundColorRequestData : RequestData
    {
        public int R;
        public int G;
        public int B;
    }

    #endregion

    #region Link requests

    public void CreateLink(string firstId, string secondId, OnRequestResponse<RequestAnswerSimple> callback)
    {
        LinkRequestData data = new LinkRequestData();
        data.firstId = firstId;
        data.secondId = secondId;

        StartCoroutine(LaunchRocket<RequestAnswerSimple>(data, "/link/create", callback));
    }

    public void RemoveLink(string firstId, string secondId, OnRequestResponse<RequestAnswerSimple> callback)
    {
        LinkRequestData data = new LinkRequestData();
        data.firstId = firstId;
        data.secondId = secondId;

        StartCoroutine(LaunchRocket<RequestAnswerSimple>(data, "/link/remove", callback));
    }

    public void AreConnected(string firstId, string secondId, OnRequestResponse<RequestAnswerConnected> callback)
    {
        LinkRequestData data = new LinkRequestData();
        data.firstId = firstId;
        data.secondId = secondId;

        StartCoroutine(LaunchRocket<RequestAnswerConnected>(data, "/document/connected", callback));
    }

    public class LinkRequestData : RequestData
    {
        public string firstId;
        public string secondId;
    }

	#endregion

	public delegate void OnRequestResponse<T>(T item, bool success);

    IEnumerator LaunchRocket <T>(RequestData data, string request, OnRequestResponse<T> onResponse)
    {
        string payload = data.ToJSON();
        Debug.Log(payload);
        string url = "http://" + PersistentData.ServerIp + ":" + PersistentData.ServerPort + request;
        string method = UnityWebRequest.kHttpVerbPOST;
		UploadHandler uploader = new UploadHandlerRaw(Encoding.ASCII.GetBytes(payload)) {
			contentType = "custom/content-type"
		};

		DownloadHandler downloader = new DownloadHandlerBuffer();

        UnityWebRequest www = new UnityWebRequest(url, method, downloader, uploader);

        yield return www.SendWebRequest();
        
        T answer = JsonUtility.FromJson<T>(www.downloadHandler.text);
        
        if (onResponse != null)
        {
            onResponse.Invoke(answer, !(www.isNetworkError || www.isHttpError));
        }
    }
}
