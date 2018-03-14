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

	private bool isOpen = false;
	private bool zoomIn = false;
	private bool zoomOut = false;

	private Vector3 zoomSize;
	private Vector3 initialScale;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	void Start() {
		initialScale = this.transform.localScale;
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
			this.transform.localScale = Vector3.Lerp(this.transform.localScale, zoomSize, ZoomSpeed * Time.deltaTime);
		}
		else if (zoomOut) {
			this.transform.localScale = Vector3.Lerp(this.transform.localScale, initialScale, ZoomSpeed * Time.deltaTime);
			if (Vector3.Distance(this.transform.localScale, initialScale) < 0.005)
			{
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
		if (!isOpen) {
			Vector3 destination = Camera.main.transform.position + Camera.main.transform.forward;
			Vector3 offset = Camera.main.transform.right * this.transform.localScale.x / 2f;
			Vector3 directionToTarget = Camera.main.transform.position - this.transform.position;
			Quaternion rotation = Quaternion.LookRotation(-directionToTarget);
			StartCoroutine(OpenTransformation(destination - offset, rotation, TransformationSpeed));
		} else {
			StartCoroutine(CloseTransformation(this.transform.position, this.transform.rotation, TransformationSpeed));
		}
		isOpen = !isOpen;
	}

	IEnumerator OpenTransformation(Vector3 targetPosition, Quaternion targetRotation, float speed) {
		for (float time = 0f; time < 1f; time += speed * Time.deltaTime) {
			this.transform.position = Vector3.Lerp(initialPosition, targetPosition, time);
			this.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, time);
			yield return null;
		}
		this.transform.position = targetPosition;
		this.transform.rotation = targetRotation;
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
