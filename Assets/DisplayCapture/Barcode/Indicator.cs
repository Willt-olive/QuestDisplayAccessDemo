using TMPro;
using UnityEngine;

namespace Anaglyph.DisplayCapture.Barcodes
{
	public class Indicator : MonoBehaviour
	{
		[SerializeField] private LineRenderer lineRenderer;
		public LineRenderer LineRenderer => lineRenderer;

		[SerializeField] private TMP_Text textMesh;
		public TMP_Text TextMesh => textMesh;

		// private Vector3[] offsetPositions = new Vector3[4]; // 4 points needed for qr
		private Vector3[] linePositions = new Vector3[2];

		// public void Set(BarcodeTracker.Result result) => Set(result.text, result.corners); // corners is for qr
		public void Set(BarcodeTracker.Result result) => Set(result.text, result.startPoint, result.endPoint);

		public void Set(string text, Vector3 startPoint, Vector3 endPoint)
		{
			Vector3 center = (startPoint + endPoint) / 2f;
			transform.position = center;

			Vector3 facingUser = (endPoint - startPoint).normalized;

			Vector3 forward = -Camera.main.transform.forward;
			Vector3 up = Vector3.Cross(facingUser, forward).normalized; // Assume barcode is upright
			if (up.magnitude < 0.001f) // If barcode is nearly parallel to camera view
            {
                up = Vector3.up; // Default to world up
            }
			
			Vector3 right = Vector3.Cross(up, forward).normalized;

			transform.rotation = Quaternion.LookRotation(forward, up);

			linePositions[0] = startPoint;
			linePositions[1] = endPoint;
			lineRenderer.positionCount = 2;
			lineRenderer.SetPositions(linePositions);

			textMesh.text = text;
			textMesh.transform.localPosition = new Vector3(0, 0, -0.05f); // to help visability

		}

	}
}