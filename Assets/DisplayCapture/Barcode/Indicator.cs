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

		/*
		public void Set(string text, Vector3[] corners)
		{
			Vector3 topCenter = (corners[2] + corners[3]) / 2f;
			transform.position = topCenter;

			Vector3 up = (corners[1] - corners[0]).normalized;
			Vector3 right = (corners[2] - corners[1]).normalized;
			Vector3 normal = -Vector3.Cross(up, right).normalized;

			Vector3 center = (corners[2] + corners[0]) / 2f;

			for (int i = 0; i < 4; i++)
			{
				Vector3 dir = (corners[i] - center).normalized;
				offsetPositions[i] = corners[i] + (dir * 0.1f);
			}

			transform.rotation = Quaternion.LookRotation(normal, up);

			lineRenderer.SetPositions(offsetPositions);
			textMesh.text = text;
		}
		*/
		public void Set(string text, Vector3 startPoint, Vector3 endPoint)
		{
			Vector3 center = (startPoint + endPoint) / 2f;
			transform.position = center;

			Vector3 forward = (endPoint - startPoint).normalized;
			Vector3 up = Vector3.up; // Assume barcode is upright
			transform.rotation = Quaternion.LookRotation(forward, up);

			linePositions[0] = startPoint;
			linePositions[1] = endPoint;
			lineRenderer.positionCount = 2;
			lineRenderer.SetPositions(linePositions);

			textMesh.text = text;
		}

	}
}