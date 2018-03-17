using System.Collections;

using UnityEngine;

public class DocumentAnimator : MonoBehaviour {

	[Range(1.0f, 10.0f)]
	public float ZoomSpeed = 4.0f;
	[Range(1.0f, 2.0f)]
	public float ZoomFactor = 1.2f;
	[Range(1.0f, 10.0f)]
	public float TransformationSpeed = 2.0f;

	private bool zoomIn	= false;
	private bool zoomOut = false;

	private Vector3		zoomSize;
	private Vector3		initialScale;
	private Vector3		initialPosition;
	private Quaternion	initialRotation;
	private GameObject	docPreview;

	void Awake() {
		this.docPreview = transform.Find("Preview").gameObject;
	}

	void Start() {
		this.initialScale = this.docPreview.transform.localScale;
		this.zoomSize = this.initialScale * this.ZoomFactor;
		InitTransform();
	}

	public void InitTransform() {
		this.initialRotation = this.transform.rotation;
		this.initialPosition = this.transform.position;
	}
	
	void Update() {
		if (zoomIn) {
			this.docPreview.transform.localScale = Vector3.Lerp(this.docPreview.transform.localScale, this.zoomSize, this.ZoomSpeed * Time.deltaTime);
		}
		else if (zoomOut) {
			this.docPreview.transform.localScale = Vector3.Lerp(this.docPreview.transform.localScale, this.initialScale, this.ZoomSpeed * Time.deltaTime);
			if (Vector3.Distance(this.docPreview.transform.localScale, this.initialScale) < 0.005) {
				this.docPreview.transform.localScale = this.initialScale;
				this.zoomOut = false;
			}
		}
	}

	public void ZoomIn() {
		if (!DocumentCollection.Instance.IsFocused(this.gameObject)) {
			this.zoomIn = true;
			this.zoomOut = false;
		}
	}

	public void ZoomOut() {
		if (!DocumentCollection.Instance.IsFocused(this.gameObject)) {
			this.zoomIn = false;
			this.zoomOut = true;
		}
	}

	public void Animate() {
		if (DocumentCollection.Instance.IsFocused(this.gameObject)) {
			OpenAnimation();
		}
		else {
			CloseAnimation();
		}
	}

	public void OpenAnimation() {
		this.transform.rotation = Camera.main.transform.rotation;
		Vector3 destination = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
		Vector3 directionToTarget = Camera.main.transform.position - destination;
		Quaternion rotation = Quaternion.LookRotation(-directionToTarget, this.transform.up);
		StartCoroutine(OpenTransformation(destination, rotation, this.TransformationSpeed));
	}

	public void CloseAnimation() {
		StartCoroutine(CloseTransformation(this.transform.position, this.transform.rotation, this.TransformationSpeed));
	}

	IEnumerator OpenTransformation(Vector3 targetPosition, Quaternion targetRotation, float speed) {
		this.zoomOut = false;
		this.zoomIn = false;

		for (float time = 0.0f; time < 1.0f; time += speed * Time.deltaTime) {
			this.transform.position = Vector3.Lerp(this.initialPosition, targetPosition, time);
			this.transform.rotation = Quaternion.Lerp(this.initialRotation, targetRotation, time);
			this.docPreview.transform.localScale = Vector3.Lerp(this.docPreview.transform.localScale, this.initialScale, this.ZoomSpeed * Time.deltaTime);
			yield return null;
		}

		this.transform.position = targetPosition;
		this.transform.rotation = targetRotation;
		this.docPreview.transform.localScale = this.initialScale;
	}

	IEnumerator CloseTransformation(Vector3 currentPosition, Quaternion currentRotation, float speed) {
		for (float time = 0.0f; time < 1.0f; time += speed * Time.deltaTime) {
			this.transform.position = Vector3.Lerp(currentPosition, this.initialPosition, time);
			this.transform.rotation = Quaternion.Lerp(currentRotation, this.initialRotation, time);
			yield return null;
		}

		this.transform.position = this.initialPosition;
		this.transform.rotation = this.initialRotation;
	}
}
