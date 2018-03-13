using System.Collections;

using UnityEngine;

public class DocumentAnimator : MonoBehaviour {
	[Range(1.0f, 10.0f)]
	public float ZoomSpeed = 4.0f;

	[Range(1.0f, 2.0f)]
	public float ZoomFactor = 1.2f;

	[Range(1.0f, 10.0f)]
	public float TransformationSpeed = 2.0f;

	private bool isOpen = false;
	private bool zoomIn = false;
	private bool zoomOut = false;

	private Vector3		zoomSize;
	private Vector3		initialScale;
	private Vector3		initialPosition;
	private Quaternion	initialRotation;

	// Use this for initialization
	void Awake() {
	}

	void Start() {
		initialScale = this.transform.localScale;
		zoomSize = initialScale * ZoomFactor;
		initialRotation = this.transform.rotation;
		initialPosition = this.transform.position;
	}

	// Update is called once per frame
	void Update() {
		if (zoomIn) {
			this.transform.localScale = Vector3.Lerp(this.transform.localScale, zoomSize, ZoomSpeed * Time.deltaTime);
		}
		else if (zoomOut) {
			this.transform.localScale = Vector3.Lerp(this.transform.localScale, initialScale, ZoomSpeed * Time.deltaTime);
			if (Vector3.Distance(this.transform.localScale, initialScale) < 0.005) {
				this.transform.localScale = initialScale;
				zoomOut = false;
			}
		}
	}

	public void ZoomIn() {
		if (!isOpen) {
			zoomIn = true;
			zoomOut = false;
		}
	}

	public void ZoomOut() {
		if (!isOpen) {
			zoomOut = true;
			zoomIn = false;
		}
	}

	public void PerformAnimation() {
		Vector3 destination;
		Quaternion rotation;
		if (!isOpen) {
			destination = Camera.main.transform.position + Camera.main.transform.forward * 1;
			Vector3 offset = new Vector3(this.transform.localScale.x/2 + 0.1f, 0, 0);
			Vector3 directionToTarget = Camera.main.transform.position - transform.position;
			rotation = Quaternion.LookRotation(-directionToTarget);
			StartCoroutine(OpenTransformation(destination - offset, rotation, TransformationSpeed));
			this.GetComponent<DocumentManage>().SetInformationPosition(destination, 0.1f);
		} else {
			StartCoroutine(CloseTransformation(this.transform.position, this.transform.rotation, TransformationSpeed));
		}
		isOpen = !isOpen;
	}

	IEnumerator OpenTransformation(Vector3 targetPosition, Quaternion targetRotation, float speed) {
		for (float t = 0f; t < 1f; t += speed * Time.deltaTime) {
			this.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
			this.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t);
			// NOTE : The zoom is still effective here.
			// Maybe we should zoom out it while openning and zoom in while closing.
			yield return null;
		}
		this.transform.position = targetPosition;
		this.transform.rotation = targetRotation;
	}

	IEnumerator CloseTransformation(Vector3 currentPosition, Quaternion currentRotation, float speed) {
		for (float t = 0f; t < 1f; t += speed * Time.deltaTime) {
			this.transform.position = Vector3.Lerp(currentPosition, initialPosition, t);
			this.transform.rotation = Quaternion.Lerp(currentRotation, initialRotation, t);
			// NOTE : The zoom is still effective here.
			// Maybe we should zoom out it while openning and zoom in while closing.
			yield return null;
		}
		this.transform.position = initialPosition;
		this.transform.rotation = initialRotation;
	}
}
