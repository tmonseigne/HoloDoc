using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalActions : Singleton<GlobalActions> {

	public GameObject	GlobalInput;
	public Texture2D	DefaultTexture;

	private GlobalInputReceiver globalInputReceiver;

	private bool photoMode = true;

	private bool updatingDocument = false;
	private GameObject document;

	void Start() {
		this.globalInputReceiver = GlobalInput.GetComponent<GlobalInputReceiver>();
		this.globalInputReceiver.OnSingleTap += SingleTap;
		this.globalInputReceiver.OnDoubleTap += DoubleTap;
	}

	void SingleTap(float delay) {
		// We need to use a coroutine to way for the delay before invoking single tap.
		// This will let some time to the user to perform a double tap.
		StartCoroutine(WaitForDoubleTap(delay));
	}

	IEnumerator WaitForDoubleTap(float delay) {
		yield return new WaitForSeconds(delay);
		if (this.globalInputReceiver.SingleTapped && photoMode) {
			Debug.Log("Single tap");
			// This should take a photo, send it to the server which will check if its valid crop and unwrap it and send it back. Then call the instanciator with this new photo a create a document and add it to the document panel.
			AudioPlayer.Instance.PlayClip(AudioPlayer.Instance.Photo);
			if (PhotoCapturer.Instance.HasFoundCamera) {
				PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
			}
			else {
				Resolution defaultTextureResolution = new Resolution {
					width = DefaultTexture.width,
					height = DefaultTexture.height
				};

				OnPhotoTaken(this.DefaultTexture, defaultTextureResolution);
			}
		}
	}

	void DoubleTap() {
		Debug.Log("Double tap");

		// This should toogle (open/close) the document viewer panel & deactivate/activate the single tap event.
		// We need to deactivate the single tap event so that if we miss the documents, we won't take a photo while in
		// document view mode.
		if (DocumentCollection.Instance.DocumentsCount() > 0) {
			DocumentCollection.Instance.Toggle();
			this.photoMode = !DocumentCollection.Instance.IsActive();
			this.updatingDocument = false;
		}
	}

	public void OnPhotoTaken(Texture2D photo, Resolution res) {
		this.photoMode = false;

#if USE_SERVER
		CameraFrame frame = new CameraFrame(res, photo.GetPixels32());
		if (updatingDocument) {	
			RequestLauncher.Instance.UpdateDocumentPhoto(document.GetComponent<DocumentManager>().Properties.Id, frame, OnUpdatePhotoRequest);
		}
		else {
			RequestLauncher.Instance.MatchOrCreateDocument(frame, OnMatchOrCreateRequest);
		}
#else
		Texture2D croppedPhoto = new Texture2D(photo.width, photo.height);
		croppedPhoto.SetPixels32(photo.GetPixels32());
		croppedPhoto.Apply();
		if (updatingDocument) {
			document.GetComponent<DocumentManager>().SetPhoto(croppedPhoto);
			DocumentCollection.Instance.Toggle();
			DocumentCollection.Instance.SetFocusedDocument(document);
			updatingDocument = false;
		}
		else {
			DocumentCollection.Instance.AddDocument(croppedPhoto, "-1");
		}
#endif
	}

    private void OnMatchOrCreateRequest(RequestLauncher.RequestAnswerDocument item, bool success) {
        if (String.IsNullOrEmpty(item.Error))
        {
            CameraFrame frame = item.CameraFrameFromBase64();
            Texture2D croppedPhoto = new Texture2D(frame.Resolution.width, frame.Resolution.height);
            croppedPhoto.SetPixels32(frame.Data);
            croppedPhoto.Apply();
            
            DocumentCollection.Instance.AddDocument(croppedPhoto, item.Id);
            DocumentProperties received = DocumentCollection.Instance.Documents[DocumentCollection.Instance.Documents.Count - 1].GetComponent<DocumentManager>().Properties;
            received.Author = item.Author;
            received.Description = item.Desc;
            received.Label = item.Label;

            Debug.Log("ici");
            if (item.Link != null) // we are in a link, we need to see if the link localy exists
            {
                Debug.Log("ici");
                List<GameObject> docs = DocumentCollection.Instance.Documents;

                foreach (string id in item.Link)
                {
                    foreach (GameObject doc in docs)
                    {
                        DocumentProperties prop = doc.GetComponent<DocumentManager>().Properties;
                        if (prop != null && id == prop.Id)
                        {
                            if (prop.LinkId != -1)
                            {
                                Debug.Log("la");
                                LinkManager.Instance.Links[prop.LinkId].Add(doc);
                                doc.GetComponent<DocumentManager>().SetColor(LinkManager.Instance.Links[prop.LinkId].LinkColor);
                            }
                            else if (docs[docs.Count - 1] != doc)
                            {
                                Debug.Log("bloup");
                                LinkManager.Instance.OnLinkStarted(doc);
                                LinkManager.Instance.OnLinkEnded(docs[docs.Count - 1], false);
                            }

                            goto EndOfLoop;
                        }
                    }
                }
                EndOfLoop:;
            }
        }
        else {
            Debug.Log(item.Error);
            Debug.Log("nop");
            this.photoMode = true;
        }
    }

	private void OnUpdatePhotoRequest(RequestLauncher.RequestAnswerDocument item, bool success) {
        if (String.IsNullOrEmpty(item.Error))
        { 
            if (item.Image != null)
            {
                CameraFrame frame = item.CameraFrameFromBase64();
                Texture2D croppedPhoto = new Texture2D(frame.Resolution.width, frame.Resolution.height);
                croppedPhoto.SetPixels32(frame.Data);
                croppedPhoto.Apply();
                document.GetComponent<DocumentManager>().SetPhoto(croppedPhoto);
                DocumentCollection.Instance.Toggle();
                DocumentCollection.Instance.SetFocusedDocument(document);

                
            }
		}
		else {
			Debug.Log(item.Error);
            Debug.Log("nop");
			this.photoMode = true;
		}
		updatingDocument = false;
	}

	public void UpdateDocumentPhoto(GameObject document) {
		this.photoMode = true;
		this.updatingDocument = true;
		this.document = document;
	}
}
