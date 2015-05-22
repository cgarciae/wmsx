using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Async;
using ZXing;
using System.Linq;

public class Login : View {
	
	public DecoderQR decoder;
	public Image userPhoto;
	public Text text;
	public GameObject background;
	
	private TTS tts;
	private WMSx wmsx;

	#region implemented abstract members of View

	public override void New (object _info)
	{
		
	}

	public override void ViewStart ()
	{
		GetDependencies();
		
		decoder.onDetection
			.Filter ((data)=> data.Text == "login")
			.OnData(InitLogin);
			
		text.text = "Enfoca tu QR personal para ingresar";
		tts.Say (text.text);
	}

	public override void ViewAwake ()
	{
		decoder = DecoderQR.instance;
		userPhoto.gameObject.SetActive (false);
	}

	public override void ViewOnEnable ()
	{
		
	}

	public override bool stateless {
		get {
			return true;
		}
	}
	
	#endregion
	
	void GetDependencies()
	{
		if (tts == null)
			tts = TTS.instance;
			
		if (wmsx == null)
			wmsx = WMSx.instance;
	}
	
	bool loggedIn = false;
	void InitLogin (Result data)
	{
		if (loggedIn)
			return;
			
		WMSx.instance.click.Play();
			
		loggedIn = true;
		
		GetUser (data.Text).Then ((User user) => {
		
		background.SetActive (true);
		text.text = "Bienvenido " + user.name;
		tts.Say (text.text);
		
		PlayerPrefs.SetString ("user", user.name);
		PlayerPrefs.SetString ("id", user.id);
		
		userPhoto.gameObject.SetActive (true);
		userPhoto.sprite = Resources.Load<Sprite> ("WMSx/view/materials/userPhoto");
		
		return Seq.WaitForSeconds (3f).Then<WorkerState>(() => {
			return wmsx.state = WorkerState.SelectingTask;
		})
		.GetFuture<WorkerState>(this);
		});
		
	}
	
	Future<User> GetUser (String data)
	{
		var user = new User();
		user.name = "Luis";
		user.id = "1234";
		
		return new Completer<User>(user);
	}
	
	
}

public interface ILogin
{
	
}

class User
{
	public String name;
	public String id;
}
