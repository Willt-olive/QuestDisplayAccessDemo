using TMPro;
using UnityEngine;
using Anaglyph.DisplayCapture.Barcodes;

public class Indicator : MonoBehaviour
{
	[SerializeField] private TMP_Text textMesh;
    [SerializeField] private float offsetDistance = 0.05f; // Distance to offset from barcode
    [SerializeField] private Vector3 offsetDirection = new Vector3(0, 0.03f, 0); // Slight up offset

    private ProductInfo product;
    private BarcodeTracker.Result lastResult;
    private bool isTracking = false;

    public void Set(BarcodeTracker.Result result, ProductInfo info)
    {
        product = info;
        lastResult = result;
        isTracking = true;

        // Set text with product info
        textMesh.text = 
            $"<b>{info.Name}</b>\n" +
            $"${info.Price:F2}\n" +
            $"By: {info.Manufacturer}\n" +
            $"Exp: {info.ExpiryDate}\n" +
            $"<i>[Tap to Add]</i>";

        // Position will be updated in Update method
        UpdatePosition();
    }

    private void Update()
    {
        if (isTracking)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        // Position right next to the barcode
        Vector3 barcodeCenter = lastResult.pose.position;
        
        // Calculate offset - to the right of the barcode
        Vector3 right = lastResult.pose.rotation * Vector3.right;
        float barcodeWidth = Vector3.Distance(lastResult.startPoint, lastResult.endPoint);
        
        // Position indicator to the right of the barcode with some spacing
        transform.position = barcodeCenter + (right * (barcodeWidth/2 + offsetDistance)) + offsetDirection;
        
        // Always face the camera
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0); // Flip text to face camera correctly
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    private void OnMouseDown()
    {
        if (product != null)
        {
            BasketManager.Instance.AddToBasket(product);
            
            // Add visual feedback
            StartCoroutine(FlashConfirmation());
        }
    }
    
    private System.Collections.IEnumerator FlashConfirmation()
    {
        Color originalColor = textMesh.color;
        textMesh.color = Color.green;
        yield return new WaitForSeconds(0.2f);
        textMesh.color = originalColor;
    }
}