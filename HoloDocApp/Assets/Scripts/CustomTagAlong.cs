using UnityEngine;

public class CustomTagAlong : MonoBehaviour {
    private float	effectiveRadius;
    private Vector3 perfectPosition;
    private Vector3 targetPosition;
    private float	initialDistanceToCamera;
    private bool	displayRadius = false;
    private bool	displayTargetPosition = false;

	public float Radius = 1.0f;
	public float Speed = 2.0f;

	void Start() {
        effectiveRadius = Radius;
        initialDistanceToCamera = Vector3.Distance(this.transform.position, Camera.main.transform.position);
    }

    void Update() {
        displayRadius = true; // display gizmos as soon as we hit play
        Vector3 currentPos = this.transform.position;
        perfectPosition = Camera.main.transform.position + Camera.main.transform.forward * initialDistanceToCamera;

        Vector3 offsetDir = currentPos - perfectPosition;
        
        // If the object distance is higher than the radius of the sphere we need to move the object
        displayTargetPosition = (offsetDir.magnitude > effectiveRadius);

        if (displayTargetPosition) {
            targetPosition = perfectPosition;
            this.transform.position = Vector3.Lerp(currentPos, targetPosition, Speed * Time.deltaTime);
            effectiveRadius = 0.002f;
        }
		else {
            effectiveRadius = Radius;
        }
    }

    public void OnDrawGizmos() {
        Color oldColor = Gizmos.color;

        if (displayRadius) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(perfectPosition, Radius);
        }

        if (displayTargetPosition) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 0.1f);
        }

        Gizmos.color = oldColor;
    }
}