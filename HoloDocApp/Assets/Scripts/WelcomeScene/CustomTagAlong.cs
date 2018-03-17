using UnityEngine;

public class CustomTagAlong : MonoBehaviour {

	public float Radius = 1.0f;
	public float Speed = 2.0f;

	private float initialDistanceToCamera;

	private float	effectiveRadius;
    private Vector3 perfectPosition;
    private Vector3 targetPosition;
    private bool	displayRadius = false;
    private bool	displayTargetPosition = false;

	void Start() {
        this.effectiveRadius = Radius;
		this.initialDistanceToCamera = Vector3.Distance(this.transform.position, Camera.main.transform.position);
    }

    void Update() {
		// display gizmos as soon as we hit play
		this.displayRadius = true; 
        Vector3 currentPos = this.transform.position;
		this.perfectPosition = Camera.main.transform.position + Camera.main.transform.forward * this.initialDistanceToCamera;

        Vector3 offsetDir = currentPos - this.perfectPosition;

		// If the object distance is higher than the radius of the sphere we need to move the object
		this.displayTargetPosition = (offsetDir.magnitude > this.effectiveRadius);

        if (this.displayTargetPosition) {
			this.targetPosition = this.perfectPosition;
            this.transform.position = Vector3.Lerp(currentPos, this.targetPosition, Speed * Time.deltaTime);
			this.effectiveRadius = 0.002f;
        }
		else {
			this.effectiveRadius = Radius;
        }
    }

    public void OnDrawGizmos() {
        Color oldColor = Gizmos.color;

        if (this.displayRadius) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.perfectPosition, Radius);
        }

        if (this.displayTargetPosition) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.targetPosition, 0.1f);
        }

        Gizmos.color = oldColor;
    }
}