using System.Collections;

using UnityEngine;

public class DocAnimator : MonoBehaviour
{
	[Range(1.0f, 10.0f)]
	public float ZoomSpeed = 4.0f;

	[Range(1.0f, 2.0f)]
	public float ZoomFactor = 1.2f;

	[Range(1.0f, 10.0f)]
	public float TransformationSpeed = 2.0f;

	public bool IsOpen = false;
	private bool zoomIn = false;
	private bool zoomOut = false;

	private Vector3 zoomSize;
	private Vector3 initialScale;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	private GameObject docPreview;

	void Awake() {
		docPreview = transform.Find("Preview").gameObject;
	}

	void Start() {
		initialScale = docPreview.transform.localScale;
		zoomSize = initialScale * ZoomFactor;
		InitTransform();
	}

	public void InitTransform() {
		initialRotation = this.transform.rotation;
		initialPosition = this.transform.position;
	}

	// Update is called once per frame
	void Update() {
		if (zoomIn) {
			docPreview.transform.localScale = Vector3.Lerp(docPreview.transform.localScale, zoomSize, ZoomSpeed * Time.deltaTime);
		}
		else if (zoomOut) {
			docPreview.transform.localScale = Vector3.Lerp(docPreview.transform.localScale, initialScale, ZoomSpeed * Time.deltaTime);
			if (Vector3.Distance(docPreview.transform.localScale, initialScale) < 0.005) {
				docPreview.transform.localScale = initialScale;
				zoomOut = false;
			}
		}
	}

	public void ZoomIn() {
		if (!IsOpen) {
			zoomIn = true;
			zoomOut = false;
		}
	}

	public void ZoomOut() {
		if (!IsOpen) {
			zoomOut = true;
			zoomIn = false;
		}
	}

	public void PerformAnimation() {
		if (!IsOpen) {
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
		StartCoroutine(OpenTransformation(destination, rotation, TransformationSpeed));
		IsOpen = true;
	}

	public void CloseAnimation() {
		StartCoroutine(CloseTransformation(this.transform.position, this.transform.rotation, TransformationSpeed));
		IsOpen = false;
	}

	IEnumerator OpenTransformation(Vector3 targetPosition, Quaternion targetRotation, float speed) {
		zoomOut = false;
		zoomIn = false;

		for (float time = 0f; time < 1f; time += speed * Time.deltaTime) {
			this.transform.position = Vector3.Lerp(initialPosition, targetPosition, time);
			this.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, time);
			this.docPreview.transform.localScale = Vector3.Lerp(docPreview.transform.localScale, initialScale, ZoomSpeed * Time.deltaTime);
			yield return null;
		}
		this.transform.position = targetPosition;
		this.transform.rotation = targetRotation;
		this.docPreview.transform.localScale = initialScale;
	}

	IEnumerator CloseTransformation(Vector3 currentPosition, Quaternion currentRotation, float speed) {
		for (float time = 0f; time < 1f; time += speed * Time.deltaTime) {
			this.transform.position = Vector3.Lerp(currentPosition, initialPosition, time);
			this.transform.rotation = Quaternion.Lerp(currentRotation, initialRotation, time);
			yield return null;
		}
		this.transform.position = initialPosition;
		this.transform.rotation = initialRotation;
	}
}
