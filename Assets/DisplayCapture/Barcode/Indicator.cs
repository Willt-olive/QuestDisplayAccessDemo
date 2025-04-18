using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace Anaglyph.DisplayCapture.Barcodes
{
    public class Indicator : MonoBehaviour
    {
	    [SerializeField] private TMP_Text textMesh;
        [SerializeField] private float verticalOffset = 0.05f;

        public void Set(BarcodeTracker.Result result, ProductInfo info)
        {
            lastSeenTime = Time.time;
            gameObject.SetActive(true);

            string preferenceWarning = GetPreferenceWarnings(info.Ingredients ?? "");
            string ingredientText = string.IsNullOrEmpty(preferenceWarning) ? "" : $"<color=red>Contains: {preferenceWarning}</color>\n";

             // Set text with product info
            textMesh.text = 
                $"<b>{info.Name}</b>\n" +
                $"${info.Price:F2}\n" +
                $"By: {info.Manufacturer}\n" +
                $"Exp: {info.ExpiryDate}\n" +
                $"{ingredientText}" +
                $"<i>[Tap to Add]</i>";

            UpdatePosition(result);
        }

        private string GetPreferenceWarnings(string ingredients)
        {
            List<string> found = new();
            ingredients = ingredients.ToLower();

            if (UserPreferences.Instance.SelectedPreferences.Contains(Preference.Gluten) && ingredients.Contains("wheat"))
                found.Add("Gluten");
            if (UserPreferences.Instance.SelectedPreferences.Contains(Preference.Lactose) && (ingredients.Contains("milk") || ingredients.Contains("cheese") || ingredients.Contains("butter")))
                found.Add("Lactose");
            if (UserPreferences.Instance.SelectedPreferences.Contains(Preference.Meat) && (ingredients.Contains("beef") || ingredients.Contains("chicken") || ingredients.Contains("pork")))
                found.Add("Meat");

            return string.Join(", ", found);
        }

        private void UpdatePosition(BarcodeTracker.Result result)
        {
            Vector3 barcodeCenter = (result.startPoint + result.endPoint) / 2f;
            Vector3 targetPosition = barcodeCenter + Vector3.up * verticalOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            // Rotate indicator to face the camera horizontally
            Vector3 cameraDirection = Camera.main.transform.position - transform.position;
            cameraDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(-cameraDirection); // - to invert text
        }

        // timeout for info
        private float lastSeenTime;
        private const float timeout = 2f;

        private void Update()
        {
            if (Time.time - lastSeenTime > timeout)
            {
                gameObject.SetActive(false);
            }
        }

    }
}
