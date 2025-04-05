using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Anaglyph.DisplayCapture.Barcodes
{
	public class BarcodeTracker : MonoBehaviour
	{
		[SerializeField] private BarcodeReader barcodeReader;

		[SerializeField] private float horizontalFieldOfViewDegrees = 82f;
		public float Fov => horizontalFieldOfViewDegrees;
		private Matrix4x4 displayCaptureProjection;

		private List<Result> results = new();
		public IEnumerable<Result> Results => results;

		public event Action<IEnumerable<Result>> OnTrackBarcodes = delegate { };
        
        [SerializeField] private float unprojectionDepth = 5.0f; // distance

		public struct Result
		{
			public string text;
			public Vector3 startPoint;
			public Vector3 endPoint; 
			public Pose pose;

			public Result(string text)
			{
				this.text = text;
				startPoint = Vector3.zero;
				endPoint = Vector3.zero;
				pose = new Pose();
			}
		}

		private void Awake()
		{
			barcodeReader.OnReadBarcodes += OnReadBarcodes;

			Vector2Int size = DisplayCaptureManager.Instance.Size;
			float aspect = size.x / (float)size.y;
            // Create a projection matrix that matches your camera settings
			displayCaptureProjection = Matrix4x4.Perspective(Fov, aspect, 1, 100f);
		}

		private void OnDestroy()
		{
			if(barcodeReader != null)
				barcodeReader.OnReadBarcodes -= OnReadBarcodes;
		}

		private void OnReadBarcodes(IEnumerable<BarcodeReader.Result> barcodeResults)
		{
            Debug.Log($"BarcodeTracker received {barcodeResults.Count()} barcodes");
			results.Clear();

            Vector2Int size = DisplayCaptureManager.Instance.Size;

			foreach (BarcodeReader.Result barcodeResult in barcodeResults)
			{

				if (barcodeResult.points.Length < 2) continue;

				Result trackResult = new Result(barcodeResult.text);

				BarcodeReader.Point startPixel = barcodeResult.points[0];
				BarcodeReader.Point endPixel = barcodeResult.points[1];

				Vector2Int size = DisplayCaptureManager.Instance.Size;

				Vector2 startUV = new Vector2(startPixel.x / size.x, 1f - startPixel.y / size.y);
				Vector2 endUV = new Vector2(endPixel.x / size.x, 1f - endPixel.y / size.y);

                // Unproject these UV coordinates using the custom method with a valid depth value
                Vector3 startCameraSpace = Unproject(displayCaptureProjection, startUV, unprojectionDepth);
                Vector3 endCameraSpace = Unproject(displayCaptureProjection, endUV, unprojectionDepth);

                // Multiply by the current camera transform to obtain world positions
                Matrix4x4 cameraMatrix = Camera.main.transform.localToWorldMatrix;
                Vector3 startWorld = cameraMatrix.MultiplyPoint(startCameraSpace);
                Vector3 endWorld = cameraMatrix.MultiplyPoint(endCameraSpace);

				Vector3 barcodeCenter = (trackResult.startPoint + trackResult.endPoint) / 2f;
				Vector3 forward = (trackResult.endPoint - trackResult.startPoint).normalized;
				Vector3 up = Vector3.up; // assume barcode is horizontally aligned

                trackResult.startPoint = startWorld;
                trackResult.endPoint = endWorld;
				trackResult.pose = new Pose(barcodeCenter, Quaternion.LookRotation(forward, up));

				results.Add(trackResult);
			}

			OnTrackBarcodes.Invoke(results);
		}

		private static Vector3 Unproject(Matrix4x4 projection, Vector2 uv)
		{
			Vector2 v = 2f * uv - Vector2.one;
			var p = new Vector4(v.x, v.y, 0.1f, 1f);
			p = projection.inverse * p;
			return new Vector3(p.x, p.y, p.z) / p.w;
		}
	}
}