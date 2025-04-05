using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Anaglyph.DisplayCapture.Barcodes
{
	public class IndicatorDriver : MonoBehaviour
	{
		[SerializeField] private BarcodeTracker barcodeTracker;
		[SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private BarcodeReader barcodeReader;

		private List<Indicator> indicators = new(5);

		private void InstantiateIndicator() => indicators.Add(Instantiate(indicatorPrefab).GetComponent<Indicator>());

		private void Awake ()
		{
			for (int i = 0; i < indicators.Capacity; i++)
				InstantiateIndicator();

            if (barcodeReader == null)
            {
                barcodeReader = FindFirstObjectByType<BarcodeReader>();
                Debug.Log(barcodeReader != null ? "Found BarcodeReader" : "BarcodeReader not found");
            }

			barcodeTracker.OnTrackBarcodes += OnTrackBarcodes;
            Debug.Log("Subscribed to OnTrackBarcodes event");
		}

		private void OnDestroy()
		{
			foreach(Indicator indicator in indicators)
			{
				Destroy(indicator.gameObject);
			}

			if(barcodeTracker != null)
				barcodeTracker.OnTrackBarcodes -= OnTrackBarcodes;
		}

		private void OnTrackBarcodes(IEnumerable<BarcodeTracker.Result> results)
		{
			int i = 0;
			foreach (BarcodeTracker.Result result in results)
			{
				if (i > indicators.Count)
					InstantiateIndicator();

                // Use direct dictionary lookup since IsKnownProduct is causing issues
                bool isKnown = TryGetProductInfo(result.text, out ProductInfo productInfo);
                
                if (isKnown)
                {
				    indicators[i].gameObject.SetActive(true);
                    indicators[i].Set(result, productInfo);
				    i++;
			    }
            }

            // Hide any unused indicators
            for (int j = i; j < indicators.Count; j++)
            {
                indicators[j].gameObject.SetActive(false);
            }
        }
        
        // Helper method as an alternative to BarcodeReader.IsKnownProduct
        private bool TryGetProductInfo(string barcodeId, out ProductInfo productInfo)
        {
            productInfo = null;
            
            if (barcodeReader == null)
            {
                Debug.LogError("BarcodeReader is null in IndicatorDriver");
                return false;
            }
            
            // Use reflection to get access to the productDatabase field in BarcodeReader
            var type = barcodeReader.GetType();
            var fieldInfo = type.GetField("productDatabase", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (fieldInfo == null)
            {
                Debug.LogError("Couldn't find productDatabase field in BarcodeReader");
                return false;
            }
            
            var database = fieldInfo.GetValue(barcodeReader) as Dictionary<string, ProductInfo>;
            
            if (database == null)
            {
                Debug.LogError("Couldn't access productDatabase in BarcodeReader");
                return false;
            }
            
            return database.TryGetValue(barcodeId, out productInfo);
        }
    }
}