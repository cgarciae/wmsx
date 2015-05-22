using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ZXing;
using ZXing.QrCode;
using ZXing.Client.Result;
using UnityEngine.UI;
using Vuforia;
using System.Threading;
using Async;
using System.Linq;

public class DecoderQR : MonoBehaviour, ITrackerEventHandler {
	
	//STATIC
	public static DecoderQR instance;
	
	//PUBLIC
	public GUIStyle style;
	public QCARBehaviour qcarBehaviour;
	
	//PRIVATE
	private bool isFrameFormatSet;
	BarcodeReader barcodeReader = new BarcodeReader();
	Thread thread;
	bool decoding = false;
	public static Vuforia.Image image;
	Func<Result> mainThread = null;
	List<ResultPoint> resultPoints = new List<ResultPoint>();
	float timeLastRecognition = -10000000000000f;
	
	
	public Result lastResult;
	public StreamController<Result> onDetection = new StreamController<Result>();
	
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
					timeLastRecognition = Time.time;
					resultPoints = new List<ResultPoint> (data.ResultPoints);
					return data;
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
	
	public float dX;
	public float dY;
	void OnGUI ()
	{
		if (resultPoints == null || resultPoints.Count < 3)
			return;
			
		if (Time.time > timeLastRecognition + 1f)
			return;
		
		var points = resultPoints
			.Select<ResultPoint,Vector2> (point2Vector)
			.ToList();
			
		var n = 0;
		var p = (points[0] + points[2]) / 2f;
		//foreach (var p in points)
		{
			var rect = new Rect(p.x, p.y, 100, 100);
			rect.center = new Vector2 (p.x, p.y);
			GUI.Label(rect, "", style);
			//print (String.Format("x {0}, y {1}", p.x, p.y));
		}
		
//		print (String.Format("Camera width {0}, height {1}", image.Width, image.Height));
//		print (String.Format("Screen width {0}, height {1}", Screen.width, Screen.height));
		
		
	}
	
	Vector2 point2Vector (ResultPoint point)
	{
		var imageHeight = ((float)DecoderQR.image.Height);
		var imageWidth = ((float)DecoderQR.image.Width);
		var screenHeight = ((float)Screen.height);
		var screenWidth = ((float)Screen.width);
		
		var imageProportions = imageHeight / imageWidth;
		var screenProportions = screenHeight / screenWidth;
		
		float x = point.X * Screen.width / DecoderQR.image.Width;
		float y = point.Y * Screen.height / DecoderQR.image.Height;
		
		if (screenProportions > imageProportions)
		{
			//imageHeight = imageWidth * p;
			var p = imageHeight / screenHeight;
			var transformedScreenWidth = screenWidth * p;
			var offset = (imageWidth - transformedScreenWidth) / 2f;
			x = (point.X - offset) / p;
		}
		else
		{
			var p = imageWidth / screenWidth;
			var transformedScreenHeight = screenHeight * p;
			var offset = (imageHeight - transformedScreenHeight) / 2f;
			y = (point.Y - offset) / p;
		}
		return new Vector2 (x, y);
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
