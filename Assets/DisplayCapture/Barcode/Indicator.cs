using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Anaglyph.DisplayCapture.Barcodes
{
	public class Indicator : MonoBehaviour
	{
		[SerializeField] private LineRenderer lineRenderer;
		public LineRenderer LineRenderer => lineRenderer;

		[SerializeField] private TMP_Text textMesh;
		public TMP_Text TextMesh => textMesh;

		[SerializeField] private bool showDebugText = true;

		private Vector3[] linePositions = new Vector3[2];
		private BarcodeReader barcodeReader;

		[SerializeField] private GameObject Panel;


		private void Awake()
		{
			barcodeReader = FindFirstObjectByType<BarcodeReader>();
			if (barcodeReader == null)
			{
				Debug.LogWarning("BarcodeReader didn't load, won't be able to look up products");
			}
		}

		public void start()
		{
			if (Panel == null)
			{
				GameObject panelFromScene = GameObject.Find("Panel"); // since i couldnt assign this should find it instead
				if (panelFromScene != null)
				{
					Panel = panelFromScene;
					Debug.Log("Panel assigned at runtime.");
				}
				else
				{
					Debug.LogWarning("Couldn't find Panel at runtime.");
				}
			}
		}

		public void Set(BarcodeTracker.Result result) => Set(result.text, result.startPoint, result.endPoint);

		public void Set(string text, Vector3 startPoint, Vector3 endPoint)
		{
			Vector3 center = (startPoint + endPoint) / 2f;
			transform.position = center;

			Vector3 facingUser = (endPoint - startPoint).normalized;

			Vector3 forward = -Camera.main.transform.forward;
			Vector3 up = Vector3.Cross(facingUser, forward).normalized;

			if (up.magnitude < 0.001f)
			{
				up = Vector3.up;
			}

			transform.rotation = Quaternion.LookRotation(forward, up);

			linePositions[0] = startPoint;
			linePositions[1] = endPoint;
			lineRenderer.positionCount = 2;
			lineRenderer.SetPositions(linePositions);

			if (Panel != null)
			{
				// debugs to check if the panel is assigned
				Debug.Log("Panel is assigned and Set() is running.");
				Debug.Log("Panel is on layer: " + Panel.layer);
				Debug.Log("Panel is in hierarchy under: " + Panel.transform.root.name);


				// Make panel visible when it loads in
				Panel.SetActive(true);

				// -------------------- this is testing if the error is panel or canvas
				// Jump it into camera view
				Panel.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
				Panel.transform.LookAt(Camera.main.transform);
				Panel.transform.localScale = Vector3.one * 0.1f;
				Panel.name = "!! VISIBLE PANEL !!";

				// Add an outline to make easer to spot
				Image img = Panel.GetComponent<Image>();
				if (img != null)
				{
					img.color = Color.magenta;
				}
				else
				{
					Debug.Log("No Image component found!");
				}
				// ------------------------------
					
				// Print current position
				Debug.Log("Panel position BEFORE: " + Panel.transform.position);

				// Move it clearly up
				Panel.transform.position = transform.position + new Vector3(0, 2f, 0);
				Panel.transform.LookAt(Camera.main.transform);

				// Rename to prove it's the correct gameObject
				Panel.name = "!!! PANEL WAS TOUCHED !!!";

				// Try to change color
				Image panelImage = Panel.GetComponent<Image>();
				if (panelImage != null)
				{
					panelImage.color = Color.red;
					Debug.Log("Color changed to red."); // this log was showing whithout changing the colour
				}
				else
				{
					Debug.LogWarning("Image component not found on Panel.");
				}
			}
			else
			{
				Debug.LogWarning("Panel is still null inside Set()");
			}


			if (showDebugText)
			{
				ProductInfo info = barcodeReader.LookupProduct(text);

				textMesh.gameObject.SetActive(true);
				textMesh.text = $"<b>{info.name}</b>\n" +
        $"Price: ${info.price:F2}\n" +
        $"Expiry: {info.expiry.ToShortDateString()}\n" +
        $"Maker: {info.manufacturer}\n\n" +
        $"<i>[Tap to add]</i>";
				textMesh.transform.localPosition = new Vector3(0, 0, -0.05f); // For visibility

				if (Panel != null)
				{
					Panel.SetActive(true); // Show the panel
					Panel.transform.position = center + new Vector3(0, 0.05f, 0); // Slight offset above barcode
					//Panel.transform.rotation = Quaternion.LookRotation(forward, up); // folow head
					Panel.GetComponent<Image>().color = Color.red; // changing colour to test if it is interacting correctly
					Panel.name = "RedPanel"; // seeing if it is being changed in the hierarchy
				}
			}
			else
			{
				textMesh.gameObject.SetActive(false);

				if (Panel != null)
				{
					Panel.SetActive(false); // hide it if not showing debug
				}
			}
		}
	}
}