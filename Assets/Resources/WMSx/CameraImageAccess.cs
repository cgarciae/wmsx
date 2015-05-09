using UnityEngine;
using System.Collections;
using System;
using ZXing;
using ZXing.QrCode;
using ZXing.Client.Result;
using Vuforia;

public class CameraImageAccess : MonoBehaviour, ITrackerEventHandler {
	
	public QCARBehaviour qcarBehaviour;
	private bool isFrameFormatSet;
	
	private Image cameraFeed;
	private string tempText;
	private string qrText;
	BarcodeReader barcodeReader = new BarcodeReader();
	
	void Start () {
		
		
		if (qcarBehaviour) {
			qcarBehaviour.RegisterTrackerEventHandler(this);
		}
		WaitSeconds (1f, () => {
			isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
			print ("aca");
			InvokeRepeating("Autofocus", 1f, 2f);
		});
	}
	
	void WaitSeconds (float t, Action f)
	{
		StartCoroutine (_WaitSeconds(t, f));
	}
	
	IEnumerator _WaitSeconds (float t, Action f)
	{
		var to = Time.time;
		while (Time.time < to + t)
			yield return null;
		
		f();
	}
	
	void Autofocus () {
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
	}
	
	void OnGUI () {
		GUI.Box(new Rect(0, Screen.height - 25, Screen.width, 25), qrText);
	}
	
	public void OnTrackablesUpdated () {
		try {
			print (1);
			if(!isFrameFormatSet) {
				isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Image.PIXEL_FORMAT.GRAYSCALE, true);
			}
			print (2);
			
			var image = CameraDevice.Instance.GetCameraImage(Image.PIXEL_FORMAT.GRAYSCALE);
			print (3);
			var data = barcodeReader.Decode(image.Pixels, image.BufferWidth, image.BufferHeight, RGBLuminanceSource.BitmapFormat.Gray8);
			print (4);
			print (data.Text);
		}
		catch (Exception e) {
			// Fail detecting QR Code!
			print (e);
		}
		finally {
			if(!string.IsNullOrEmpty(tempText)) {
				qrText = tempText;
			}
		}
	}
	
	#region ITrackerEventHandler implementation
	
	public void OnInitialized ()
	{
		//throw new System.NotImplementedException ();
	}
	
	#endregion
}
