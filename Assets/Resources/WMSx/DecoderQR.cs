using UnityEngine;
using System.Collections;
using System;
using ZXing;
using ZXing.QrCode;
using ZXing.Client.Result;
using UnityEngine.UI;
using Vuforia;
using System.Threading;
using Async;

public class DecoderQR : MonoBehaviour, ITrackerEventHandler {
	
	public static DecoderQR instance;
	
	public QCARBehaviour qcarBehaviour;
	private bool isFrameFormatSet;
	BarcodeReader barcodeReader = new BarcodeReader();
	Thread thread;
	bool decoding = false;
	Vuforia.Image image;
	Func<String> mainThread = null;
	
	public StreamController<String> onDetection = new StreamController<String>();
	
	void Awake ()
	{
		instance = this;
	}
	
	void Start () 
	{
		Seq.WaitForSeconds (1f).Then(() => {
			isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Vuforia.Image.PIXEL_FORMAT.GRAYSCALE, true);
			qcarBehaviour.RegisterTrackerEventHandler(this);
			InvokeRepeating("Autofocus", 1f, 2f);
		})
		.Start(this);
	}
	
	void Update ()
	{
		if (mainThread != null)
		{
			onDetection.Broadcast(mainThread());
		}
	}
	
	void OnEnable()
	{
		thread = new Thread(Decode);
		thread.Start();
	}
	
	void OnDisable()
	{
		thread.Abort();
	}
//	
//	void WaitSeconds (float t, Action f)
//	{
//		StartCoroutine (_WaitSeconds(t, f));
//	}
//	
//	IEnumerator _WaitSeconds (float t, Action f)
//	{
//		var to = Time.time;
//		while (Time.time < to + t)
//			yield return null;
//		
//		f();
//	}
//	
//	
//	IEnumerator ClearTextBehavioiur (float span)
//	{
//		while (true)
//		{
//			if (Time.time > timeLastRecognition + span)
//			{
//				if (text != "")
//					text = "";
//			}
//			
//			yield return null;
//		}
//	}
//	
	void Autofocus () {
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
	}
	
	
	public void OnTrackablesUpdated () 
	{
		if (decoding)
			return;
			
		if(!isFrameFormatSet)
		{
			isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Vuforia.Image.PIXEL_FORMAT.GRAYSCALE, true);
		}
		image = CameraDevice.Instance.GetCameraImage(Vuforia.Image.PIXEL_FORMAT.GRAYSCALE);
		decoding = true;																								
	}
	
	void Decode () {
		while (true) {
			while (! decoding)
				Thread.Sleep (20);
			
			_Decode();
		}
	}
	
	void _Decode ()
	{
		try {
			var data = barcodeReader.Decode(image.Pixels, image.Width, image.Height, RGBLuminanceSource.BitmapFormat.Gray8);
			if (data != null) {
				var text = data.Text;
				mainThread = () => {
					mainThread = null;
					return text;
				};
			}
		}
		catch (Exception e){
			print (e);
		}
		finally {
			decoding = false;
		}
	}
	
	#region ITrackerEventHandler implementation
	public void OnInitialized () {}
	#endregion
	
//	public Color32[] GetColorArray(byte[] rgb565Data)
//	{
//		if (rgb565Data.Length % 1 != 0) 
//			throw new Exception("Must have an even length&quot");
//		
//		var colors = new Color32[rgb565Data.Length / 2];
//		
//		for (var i = 0; i < rgb565Data.Length; i+=2)
//		{
//			colors[i / 2] = new Color32((byte)(rgb565Data[i] & 0xF8),
//			                            (byte)(((rgb565Data[i] & 7) << 5) | ((rgb565Data[i + 1] & 0xE0) >> 3)),
//			                            (byte)((rgb565Data[i + 1] & 0x1F) & 3),
//			                            (byte)1);
//		}
//		
//		return colors;
//	}
}
