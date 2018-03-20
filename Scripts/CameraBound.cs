using UnityEngine;
public class CameraBound : MonoBehaviour
{

	private const float DISTANCE_MARGIN = 1.0f;

	static float aspectRatio;
	static float tanFov;
	static int frame = 0;
	const float DISTANCE_LIMIT = 200;


 	public static Vector2  MIN_EDGE = new Vector2 (float.MaxValue, float.MaxValue);
	public static Vector2  MAX_EDGE = new Vector2 (float.MinValue, float.MinValue);


	public static bool FindMinMax(  float NX,  float NY) {
		bool updated = false;

		if (NX > MAX_EDGE.x) {
			MAX_EDGE.x = NX;
			updated = true;
		}
		if (NX < MIN_EDGE.x) {
			MIN_EDGE.x = NX;
			updated = true;
		}
		if (NY > MAX_EDGE.y) {
			MAX_EDGE.y = NY;
			updated = true;
		}
		if (NY < MIN_EDGE.y) {
			MIN_EDGE.y = NY;
			updated = true;
		}
		return updated;
	}


	public static void FrameCamera (Vector2 edge1, Vector2 edge2) {
		Vector3 newCameraPos = Camera.main.transform.position;

		// ignore upgdates until edge boundary stablized
		if (frame < 4) {
			aspectRatio = Screen.width / Screen.height;
			tanFov = Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView) * aspectRatio / 2.0f;
	 
			frame += 1; 
			return;
		}
	 
	 	Vector2 midVector = edge2 - edge1 ;
		Vector2 middlePoint = edge1 + 0.5f * midVector;
	 
	 	float boundaryDistance = midVector.magnitude;

		float cameraDistance = (boundaryDistance / 2.0f  / aspectRatio / tanFov);
		if (cameraDistance > DISTANCE_LIMIT) 
			cameraDistance = DISTANCE_LIMIT;
		else {
			Debug.Log("camera distance " +cameraDistance  );
			newCameraPos = new Vector3(middlePoint.x, middlePoint.y, -cameraDistance);
			 
			Camera.main.transform.position = newCameraPos;
		}
	}

	public static void Update(float NX, float NY) {

		if (CameraBound.FindMinMax(NX, NY))
			CameraBound.FrameCamera(  MIN_EDGE,   MAX_EDGE);
	}

	public static void Reset() {
		frame = 0;
		MIN_EDGE = new Vector2 (float.MaxValue, float.MaxValue);
		MAX_EDGE = new Vector2 (float.MinValue, float.MinValue);
	}
}