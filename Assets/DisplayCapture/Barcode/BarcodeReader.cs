using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Anaglyph.DisplayCapture.Barcodes
{
	[Serializable]
    public class ProductInfo
    {
        public string Name;
        public double Price;
        public string Manufacturer;
        public string ManufactureDate;
        public string ExpiryDate;
        public string Ingredients;

        public ProductInfo(string name, double price, string manufacturer, string manufactureDate, string expiryDate, string ingredients)
        {
            Name = name;
            Price = price;
            Manufacturer = manufacturer;
            ManufactureDate = manufactureDate;
            ExpiryDate = expiryDate;
            Ingredients = ingredients;
        }

        public override string ToString()
        {
            return $"{Name}\n${Price:F2}\nBy: {Manufacturer}\nExp: {ExpiryDate}\nIngredients: {Ingredients}";
        }
    }

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

		private Dictionary<string, ProductInfo> productDatabase = new Dictionary<string, ProductInfo>();

        public event Action<IEnumerable<Result>> OnReadBarcodes = delegate { };
        public event Action<Result, ProductInfo> OnReadKnownProduct = delegate { };

		private AndroidInterface androidInterface;

		private void Awake()
		{
			Debug.Log("BarcodeReader Awake");
			androidInterface = new AndroidInterface(gameObject);
			InitializeProductDatabase();
            //InitializeProductDatabaseFromCSV();
		}
        
        /*
        private void InitializeProductDatabaseFromCSV()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "tesco_groceries_dataset.csv");
            var lines = File.ReadAllLines(path).Skip(1);

            foreach (var line in lines)
            {
                var cols = line.Split('\t');
                if (cols.Length < 15) continue; // Ensure sufficient data
        
                string gtin13 = cols[3];
                string name = cols[0];
                double price = double.TryParse(cols[4], out var parsedPrice) ? parsedPrice : 0;
                string manufacturer = cols[8];
                string ingredients = cols[14];

                productDatabase[gtin13] = new ProductInfo(name, price, manufacturer, "", "", ingredients);
            }

            Debug.Log($"Loaded CSV dataset, total products: {productDatabase.Count}");
        }
        */

		private void InitializeProductDatabase()
        {
            productDatabase.Add("1234567890123", new ProductInfo(
                "Whole Milk", 
                3.49, 
                "DairyFresh Inc.", 
                "2025-01-10", 
                "2025-03-10",
                "milk"));

            productDatabase.Add("2345678901234", new ProductInfo(
                "White Bread", 
                2.29, 
                "Golden Bake Ltd.", 
                "2025-02-01", 
                "2025-04-15",
                "wheat"));

            productDatabase.Add("3456789012345", new ProductInfo(
                "Chicken Breast", 
                8.99, 
                "Fresh Farms Meat", 
                "2025-02-05", 
                "2025-02-20",
                "chicken"));

            productDatabase.Add("4567890123456", new ProductInfo(
                "Rice (5kg)", 
                12.50, 
                "Harvest Grains", 
                "2024-12-15", 
                "2025-12-15",
                "rice"));

            productDatabase.Add("5678901234567", new ProductInfo(
                "Apple Juice", 
                7.79, 
                "Fruity Beverages", 
                "2025-01-20", 
                "2025-06-20",
                "fruit"));

            productDatabase.Add("6789012345678", new ProductInfo(
                "Salted Butter", 
                5.29, 
                "Creamy Dairy", 
                "2025-01-18", 
                "2025-04-18",
                "butter"));

            productDatabase.Add("7890123456789", new ProductInfo(
                "Corn Flakes", 
                6.49, 
                "Crunchy Cereals", 
                "2025-01-12", 
                "2025-07-12",
                "wheat"));

            productDatabase.Add("8901234567890", new ProductInfo(
                "Instant Noodles", 
                1.29, 
                "QuickEats Ltd.", 
                "2025-02-08", 
                "2026-02-08",
                "wheat"));

            productDatabase.Add("9012345678901", new ProductInfo(
                "Dishwashing Liquid", 
                3.99, 
                "Sparkle Clean", 
                "2025-01-05", 
                "2027-01-05",
                "chemicals"));

            productDatabase.Add("1122334455667", new ProductInfo(
                "Shampoo", 
                7.99, 
                "HairCare Co.", 
                "2024-12-20", 
                "2027-12-20",
                "chemicals"));

            productDatabase.Add("2233445566778", new ProductInfo(
                "Toothpaste", 
                2.99, 
                "FreshSmile Corp.", 
                "2024-11-25", 
                "2026-11-25",
                "chemicals"));

            productDatabase.Add("3344556677889", new ProductInfo(
                "Laundry Detergent", 
                9.99, 
                "BrightWash Ltd.", 
                "2025-01-15", 
                "2027-01-15",
                "chemicals"));

            productDatabase.Add("4455667788990", new ProductInfo(
                "Frozen Peas", 
                3.49, 
                "GreenHarvest", 
                "2025-01-02", 
                "2026-01-02",
                "vegetables"));

            productDatabase.Add("5566778899001", new ProductInfo(
                "Cheddar Cheese", 
                5.79, 
                "DairyGoodness", 
                "2025-02-03", 
                "2025-05-03",
                "cheese"));

            productDatabase.Add("6677889900112", new ProductInfo(
                "Tomato Ketchup", 
                2.89, 
                "SaucyFoods Inc.", 
                "2024-12-28", 
                "2026-12-28",
                "fruit"));

            productDatabase.Add("7788990011223", new ProductInfo(
                "Olive Oil", 
                10.49, 
                "Mediterranean Gold", 
                "2024-11-15", 
                "2026-11-15",
                "fruit"));

            productDatabase.Add("8899001122334", new ProductInfo(
                "Canned Tuna", 
                3.99, 
                "Ocean Delights", 
                "2024-10-30", 
                "2027-10-30",
                "beef"));

            productDatabase.Add("9900112233445", new ProductInfo(
                "Chocolate Bar", 
                1.79, 
                "SweetTreats Ltd.", 
                "2025-01-22", 
                "2026-01-22",
                "milk"));

            productDatabase.Add("1001223344556", new ProductInfo(
                "Bottled Water", 
                1.49, 
                "PureSpring Water", 
                "2025-02-10", 
                "2027-02-10",
                "water"));

            productDatabase.Add("1102334455667", new ProductInfo(
                "Coffee Beans", 
                14.99, 
                "Aroma Roasters", 
                "2024-12-05", 
                "2025-12-05",
                "fruit"));

            productDatabase.Add("5449000000996", new ProductInfo(
                "Test product", 
                1.00, 
                "Test Company", 
                "2024-12-28", 
                "2025-12-28",
                "wheat"));

            Debug.Log($"Product database initialized with {productDatabase.Count} products");

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
            OnReadKnownProduct = delegate { };
		}

         public bool IsKnownProduct(string barcodeId, out ProductInfo productInfo)
        {
            return productDatabase.TryGetValue(barcodeId, out productInfo);
        }

        private void Update()
        {
            // If we have recent results, continuously update them 
            // This helps keep indicators tracked even when no new barcodes are read
            if (lastResults.results != null && lastResults.results.Length > 0)
            {
                OnReadBarcodes.Invoke(lastResults.results);
            }
        }

        private Results lastResults;

#pragma warning disable IDE0051 // Remove unused private members
        private void OnBarcodeResults(string json)
        {
            Debug.Log($"Received barcode results from Android: {json}");
            try
            {
                if (string.IsNullOrEmpty(json)) return;
        
                lastResults = JsonUtility.FromJson<Results>(json);
                if (lastResults.results == null || lastResults.results.Length == 0) return;
        
                OnReadBarcodes.Invoke(lastResults.results);
        
                foreach (var result in lastResults.results)
                {
                    if (productDatabase.TryGetValue(result.text, out ProductInfo productInfo))
                    {
                        Debug.Log($"Found product in database: {productInfo.Name}");
                        OnReadKnownProduct.Invoke(result, productInfo);
                    }
                    else
                    {
                        Debug.LogWarning($"Barcode not in database: {result.text}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[OnBarcodeResults] Error: {e.Message}\n{e.StackTrace}");
            }
        }

#pragma warning restore IDE0051 // Remove unused private members
	}
}