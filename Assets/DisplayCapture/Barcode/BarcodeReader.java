package com.trev3d.DisplayCapture;

import static android.content.ContentValues.TAG;

import android.graphics.Bitmap;

import com.google.android.gms.tasks.Task;
import com.google.gson.Gson;
import com.google.zxing.BinaryBitmap;
import com.google.zxing.DecodeHintType;
import com.google.zxing.LuminanceSource;
import com.google.zxing.MultiFormatReader;
import com.google.zxing.RGBLuminanceSource;
import com.google.zxing.Result;
import com.google.zxing.ResultPoint;
import com.google.zxing.common.HybridBinarizer;
import com.google.zxing.qrcode.QRCodeReader;
import com.unity3d.player.UnityPlayer;

import java.io.Serializable;
import java.nio.ByteBuffer;
import java.util.EnumMap;
import java.util.EnumSet;
import java.util.Map;
import java.util.Objects;

import android.util.Log;
import com.google.zxing.BarcodeFormat;


public class BarcodeReader implements IDisplayCaptureReceiver {

	private static class Result implements Serializable {
		public String text;
		public Point[] points;
		public long timestamp;
		public String format;

		public Result(com.google.zxing.Result result, long timestamp) {
			this(result.getText(), result.getResultPoints(), timestamp);
			this.format = result.getBarcodeFormat().toString();
		}

		public Result(String text, ResultPoint[] points, long timestamp) {
			this.text = text;
			this.timestamp = timestamp;

			this.points = new Point[points.length];
			for(int i = 0; i < points.length; i++)
				this.points[i] = new Point(points[i]);
		}
	}

	private static class Results implements Serializable {
		public BarcodeReader.Result[] results;

		public Results(int size) {
			results = new BarcodeReader.Result[size];
		}
	}

	private static class Point implements Serializable {
		public float x, y;

		public Point(android.graphics.Point point) {
			x = point.x;
			y = point.y;
		}

		public Point(ResultPoint point) {
			x = point.getX();
			y = point.getY();
		}
	}

	public static BarcodeReader instance = null;

	private final MultiFormatReader multiFormatReader;
	private final Gson gson;
	
	private boolean enabled;
	private volatile boolean readingBarcode = false;

	private UnityInterface unityInterface;

	private record UnityInterface(String gameObjectName) {
		private void Call(String functionName) {
			UnityPlayer.UnitySendMessage(gameObjectName, functionName, "");
		}

		public void OnBarcodeResults(String json) {
			UnityPlayer.UnitySendMessage(gameObjectName, "OnBarcodeResults", json);
		}
	}

	public BarcodeReader() {


		multiFormatReader = new MultiFormatReader();

		Map<DecodeHintType, Object> hints = new EnumMap<>(DecodeHintType.class);

		EnumSet<BarcodeFormat> decodeFormats = EnumSet.of(
			BarcodeFormat.QR_CODE,
			BarcodeFormat.UPC_A,
			BarcodeFormat.UPC_E,
			BarcodeFormat.EAN_13,
			BarcodeFormat.EAN_8,
			BarcodeFormat.CODE_39,
			BarcodeFormat.CODE_93,
			BarcodeFormat.CODE_128,
			BarcodeFormat.ITF,
			BarcodeFormat.DATA_MATRIX,
			BarcodeFormat.AZTEC,
			BarcodeFormat.PDF_417
		);

		hints.put(DecodeHintType.POSSIBLE_FORMATS, decodeFormats);
		hints.put(DecodeHintType.TRY_HARDER, Boolean.TRUE);
		multiFormatReader.setHints(hints);

		gson = new Gson();
		Log.i(TAG, "BarcodeReader Started");
	}

	public static synchronized BarcodeReader getInstance()
	{
		if (instance == null)
			instance = new BarcodeReader();

		return instance;
	}

	public void setEnabled(boolean enabled) {
		if(this.enabled == enabled)
			return;

		this.enabled = enabled;
		Log.i(TAG, "BarcodeReader enabled");

		if(this.enabled) {
			DisplayCaptureManager.getInstance().receivers.add(this);
		} else {
			DisplayCaptureManager.getInstance().receivers.remove(this);
		}
	}

	@Override
	public void onNewImage(ByteBuffer byteBuffer, int width, int height, long timestamp) {

		if(readingBarcode) {
			Log.v(TAG, "BarcodeReader already in use");
			return;
		}

		if(unityInterface == null) {
			Log.e(TAG, "UnityInterface not started");
			return;
		}

		Log.v(TAG, "Starting barcode detection on image " + width + "x" + height);
		readingBarcode = true;

		new Thread(() -> {
			try {
				var bitmap = Bitmap.createBitmap(
					width,
					height,
					Bitmap.Config.ARGB_8888
				);

				byteBuffer.rewind();
				bitmap.copyPixelsFromBuffer(byteBuffer);

				int[] pixels = new int[width * height];
				bitmap.getPixels(pixels, 0, width, 0, 0, width, height);
				BinaryBitmap binaryBitmap = new BinaryBitmap(new HybridBinarizer(new RGBLuminanceSource(width, height, pixels)));

				try {
					var barcodeResult = multiFormatReader.decode(binaryBitmap);
					Log.i(TAG, "Barcode detected" + barcodeResult.getText());
					Result result = new Result(barcodeResult, timestamp);


					Results results = new Results(1);
					results.results[0] = result;

					String resultsAsJson = gson.toJson(results);
					Log.i(TAG, "JSON: " + resultsAsJson);

					if(unityInterface != null) {
						unityInterface.OnBarcodeResults(resultsAsJson);
					} else {
						Log.e(TAG, "Unity interface is null when trying to send results");
					}
				} catch (Exception e) {
					Log.v(TAG, "No barcode detected in this frame: " + e.getMessage());
				}
			} catch (Exception e) {
				Log.e(TAG, "Error processing image for barcode detection: " + e.getMessage(), e);
			} finally {
				readingBarcode = false;
			}
		}).start();
	}

	public void setup(String gameObjectName) {
		unityInterface = new UnityInterface(gameObjectName);
	}

}
