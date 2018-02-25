using UnityEngine;

public class FindPosition : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		Vector2Int posPixel = new Vector2Int(Camera.main.pixelHeight / 2, Camera.main.pixelWidth / 2);
		Vector3? position = ConvertPixelToScreen(posPixel.x, posPixel.y);
		Debug.Log("posConv : " + position);
	}

	// pixelX to [0 ; Camera.main.pixelHeight], pixelY to [0 ; Camera.main.pixelWidth]
	Vector3? ConvertPixelToScreen(int pixelX, int pixelY)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(pixelX, pixelY));
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 100))
		{
			return hit.point;
		}

		return null;
	}
}
