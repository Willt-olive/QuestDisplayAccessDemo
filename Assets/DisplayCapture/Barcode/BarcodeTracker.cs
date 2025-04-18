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
        
        [SerializeField] private float unprojectionDepth = 1.0f; // distance

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
            // Create a projection matrix that matches your camera settings
            displayCaptureProjection = DisplayCaptureManager.Instance.ProjectionMatrix;
        }

		private void OnDestroy()
		{
			if (barcodeReader != null)
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

                // Directly convert to viewport coordinates (0 to 1 range)
				Vector3 startUV = new Vector3(startPixel.x / size.x, 1f - (startPixel.y / size.y), unprojectionDepth);
				Vector3 endUV = new Vector3(endPixel.x / size.x, 1f - (endPixel.y / size.y), unprojectionDepth);

                // Using Unity's built-in viewport to world conversion for stability
                Vector3 startWorld = Camera.main.ViewportToWorldPoint(startUV);
                Vector3 endWorld = Camera.main.ViewportToWorldPoint(endUV);

                Vector3 barcodeCenter = (startWorld + endWorld) / 2f;
                Vector3 forward = (endWorld - startWorld).normalized;
                Vector3 up = Vector3.up;

                trackResult.startPoint = startWorld;
                trackResult.endPoint = endWorld;
				trackResult.pose = new Pose(barcodeCenter, Quaternion.LookRotation(forward, up));

				results.Add(trackResult);
			}

			OnTrackBarcodes.Invoke(results);
		}


        private Vector3 UnprojectToCameraSpace(Vector2 uv, float depth)
        {
            // Convert UV (0,1) to normalized device coordinates (-1,1)
            Vector4 clipSpace = new Vector4(uv.x * 2f - 1f, uv.y * 2f - 1f, 2f * (depth - Camera.main.nearClipPlane) / (Camera.main.farClipPlane - Camera.main.nearClipPlane) - 1f, 1f);

            // Convert from clip space to camera space
            Vector4 cameraSpacePos = displayCaptureProjection.inverse * clipSpace;
            cameraSpacePos /= cameraSpacePos.w;

            return new Vector3(cameraSpacePos.x, cameraSpacePos.y, cameraSpacePos.z);
        }
    }
}
