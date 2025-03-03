using System;
using System.Collections.Generic;
using UnityEngine;

namespace Anaglyph.DisplayCapture.Barcodes
{
	public class BarcodeReader : MonoBehaviour
	{
		[Serializable]
		private struct Results
		{
			public Result[] results;
		}

		[Serializable]
		public struct Result
		{
			public string text;
			public Point[] points;
			public long timestamp;
			public string format;
		}

		[Serializable]
		public struct Point
		{
			public float x, y;
		}

		private class AndroidInterface
		{
			private AndroidJavaClass androidClass;
			private AndroidJavaObject androidInstance;

			public AndroidInterface(GameObject messageReceiver)
			{
				try
                {
                    androidClass = new AndroidJavaClass("com.trev3d.DisplayCapture.BarcodeReader");
                    androidInstance = androidClass.CallStatic<AndroidJavaObject>("getInstance");
                    androidInstance.Call("setup", messageReceiver.name);
                    Debug.Log($"Android BarcodeReader initialized and setup with {messageReceiver.name}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to initialize Android BarcodeReader: {e.Message}");
                }
			}

			public void SetEnabled(bool enabled)
            {
                try
                {
                    if (androidInstance != null)
                    {
                        androidInstance.Call("setEnabled", enabled);
                        Debug.Log($"Android BarcodeReader enabled: {enabled}");
                    }
                    else
                    {
                        Debug.LogError("Cannot enable BarcodeReader - androidInstance is null");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error setting BarcodeReader enabled: {e.Message}");
                }
            }
		}

		public event Action<IEnumerable<Result>> OnReadBarcodes;

		private AndroidInterface androidInterface;

		private void Awake()
		{
			Debug.Log("BarcodeReader Awake");
			androidInterface = new AndroidInterface(gameObject);
		}

		private void OnEnable()
		{
			Debug.Log("BarcodeReader OnEnable - Enabling Android interface");
			androidInterface.SetEnabled(true);
		}

		private void OnDisable()
		{
			Debug.Log("BarcodeReader OnDisable - Disabling Android interface");
			androidInterface.SetEnabled(false);
		}

		private void OnDestroy()
		{
			Debug.Log("BarcodeReader OnDestroy");
			OnReadBarcodes = delegate { };
		}


#pragma warning disable IDE0051 // Remove unused private members
		private void OnBarcodeResults(string json)
		{
			Results results = JsonUtility.FromJson<Results>(json);
			OnReadBarcodes.Invoke(results.results);
		}
#pragma warning restore IDE0051 // Remove unused private members
	}
}