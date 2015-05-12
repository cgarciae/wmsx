using UnityEngine;
using System.Collections;
using System;
using ZXing;
using ZXing.QrCode;
using ZXing.Client.Result;
using UnityEngine.UI;
using Vuforia;
using System.Threading;

public class CameraImageAccess : MonoBehaviour, ITrackerEventHandler {
	
	public QCARBehaviour qcarBehaviour;
	public Text _text;
	public String text 
	{
		get {return _text.text;}
		set {_text.text = "(" + (n).ToString() + ") " + value;}
	}
	
	private int n = 0;
	
	private bool isFrameFormatSet;
	private Vuforia.Image cameraFeed;
	private string tempText;
	private string qrText;
	private float timeLastRecognition;
	private WebCamTexture cam;
	BarcodeReader barcodeReader = new BarcodeReader();
	
	
	void Start () {
		
		
		WaitSeconds (1f, () => {
			isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Vuforia.Image.PIXEL_FORMAT.GRAYSCALE, true);
			qcarBehaviour.RegisterTrackerEventHandler(this);
			InvokeRepeating("Autofocus", 1f, 2f);
		});
		
		StartCoroutine (ClearTextBehavioiur(0.5f));
		Loom.RunAsync (Decode);
		
	}
	
	void Update ()
	{
		if (mainThread != null)
			mainThread();
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
	
	
	IEnumerator ClearTextBehavioiur (float span)
	{
		while (true)
		{
			if (Time.time > timeLastRecognition + span)
			{
				if (text != "")
					text = "";
			}
			
			yield return null;
		}
	}
	
	void Autofocus () {
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
	}
	bool decoding = false;
	Vuforia.Image image;
	Action mainThread = ()=> {};
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
			while (! decoding) {
				Thread.Sleep (20);
			}
			_Decode();
		}
	}
	
	void _Decode ()
	{
		try {
			var data = barcodeReader.Decode(image.Pixels, image.Width, image.Height, RGBLuminanceSource.BitmapFormat.Gray8);
			
			if (data != null) {
				mainThread = () => {
					text = data.Text;
					timeLastRecognition = Time.time;
					mainThread = null;
					n++;
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
	public void OnInitialized ()
	{
		//throw new System.NotImplementedException ();
	}
	
	#endregion
	
	public Color32[] GetColorArray(byte[] rgb565Data)
	{
		if (rgb565Data.Length % 1 != 0) 
			throw new Exception("Must have an even length&quot");
		
		var colors = new Color32[rgb565Data.Length / 2];
		
		for (var i = 0; i < rgb565Data.Length; i+=2)
		{
			colors[i / 2] = new Color32((byte)(rgb565Data[i] & 0xF8),
			                            (byte)(((rgb565Data[i] & 7) << 5) | ((rgb565Data[i + 1] & 0xE0) >> 3)),
			                            (byte)((rgb565Data[i + 1] & 0x1F) & 3),
			                            (byte)1);
		}
		
		return colors;
	}
}
