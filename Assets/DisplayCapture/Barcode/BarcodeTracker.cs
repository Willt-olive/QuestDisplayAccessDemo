using Anaglyph.XRTemplate.DepthKit;
using System;
using System.Collections.Generic;
using UnityEngine;

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

		public struct Result
		{
			public string text;
			public Vector3 startPoint;
			public Vector3 endPoint; 
			// taditional barcodes only have a start and an end unlike qr which has 4 corners

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

			displayCaptureProjection = Matrix4x4.Perspective(Fov, aspect, 1, 100f);
		}

		private void OnDestroy()
		{
			if(barcodeReader != null)
				barcodeReader.OnReadBarcodes -= OnReadBarcodes;
		}

		private void OnReadBarcodes(IEnumerable<BarcodeReader.Result> barcodeResults)
		{
			results.Clear();

			foreach (BarcodeReader.Result barcodeResult in barcodeResults)
			{
				// Result trackResult = new Result(barcodeResult.text);

				float timestampInSeconds = barcodeResult.timestamp * 0.000000001f;
				OVRPlugin.PoseStatef headPoseState = OVRPlugin.GetNodePoseStateAtTime(timestampInSeconds, OVRPlugin.Node.Head);
				OVRPose headPose = headPoseState.Pose.ToOVRPose();
				Matrix4x4 headTransform = Matrix4x4.TRS(headPose.position, headPose.orientation, Vector3.one);

				if (barcodeResult.points.Length < 2) continue;

				Result trackResult = new Result(barcodeResult.text);

				BarcodeReader.Point startPixel = barcodeResult.points[0];
				BarcodeReader.Point endPixel = barcodeResult.points[1];

				Vector2Int size = DisplayCaptureManager.Instance.Size;

				Vector2 startUV = new Vector2(startPixel.x / size.x, 1f - startPixel.y / size.y);
				Vector2 endUV = new Vector2(endPixel.x / size.x, 1f - endPixel.y / size.y);

				trackResult.startPoint = Unproject(displayCaptureProjection, startUV);
				trackResult.endPoint = Unproject(displayCaptureProjection, endUV);

				trackResult.startPoint.z = -trackResult.startPoint.z;
				trackResult.endPoint.z = -trackResult.endPoint.z;

				trackResult.startPoint = headTransform.MultiplyPoint(trackResult.startPoint);
				trackResult.endPoint = headTransform.MultiplyPoint(trackResult.endPoint);

				Vector3 barcodeCenter = (trackResult.startPoint + trackResult.endPoint) / 2f;
				Vector3 forward = (trackResult.endPoint - trackResult.startPoint).normalized;
				Vector3 up = Vector3.up; // assume barcode is horizontally aligned

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