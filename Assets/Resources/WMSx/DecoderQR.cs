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
					resultPoints = new List<ResultPoint> (data.ResultPoints);
					//print (data.putMetadata);
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
		var points = resultPoints
			.Select<ResultPoint,Vector2> (point2Vector)
			.ToList();
			
		var n = 0;
		foreach (var p in points.Take(1))
		{
			var rect = new Rect(p.x, p.y, 15, 15);
			rect.center = new Vector2 (p.x, p.y);
			GUI.Label(rect, "", style);
			//print (String.Format ("Result {0}: {1}", n++, p));
		}
		
//		print (String.Format("Camera width {0}, height {1}", image.Width, image.Height));
//		print (String.Format("Screen width {0}, height {1}", Screen.width, Screen.height));
		
		
	}
	
	Vector2 point2Vector (ResultPoint point)
	{
		var pI = ((float)DecoderQR.image.Height)/((float)DecoderQR.image.Width);
		var pS = ((float)Screen.height)/((float)Screen.width);
		var pxS = ((float)Screen.width)/((float)Screen.height);
		var pxC = ((float)DecoderQR.image.Width)/((float)DecoderQR.image.Height);
		
		float x = point.X * Screen.width / DecoderQR.image.Width;
		float y = point.Y * Screen.height / DecoderQR.image.Height;
		
		if (pxS < pxC)
		{
			//print ("GT");
			var w = ((float)DecoderQR.image.Width);
			var pwS = ((float)Screen.width)/((float)Screen.height);
			var wS = pwS * w;
			var dx = (w - wS)/2f;
			var px = (point.X - dx)/wS;
			
			x = px * ((float)Screen.width);
			print (String.Format("px {0}", px));
			print (String.Format("x {0}", x));
			//x *= 1f / pS;
		}
		else
		{
			print ("LT");
			y *= pI;
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
