using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Async;
using ZXing;
using System.Linq;

public class Login : View {
	
	public DecoderQR decoder;

	public GUIPanel gui;
	
	private TTS tts;
	private WMSx wmsx;

	#region implemented abstract members of View

	public override void New (object _info)
	{
		
	}

	public override void ViewStart ()
	{
		GetDependencies();

		gui.showImage = false;
		gui.status.text = "Login";
		gui.quantity.text = "";

		decoder.onDetection
			.Filter ((data)=> data.Text == "login")
			.OnData(InitLogin);
			
		gui.text.text = "Enfoca tu QR personal para ingresar";
		tts.Say (gui.text.text);
	}

	public override void ViewAwake ()
	{

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
			
		if (decoder == null)
			decoder = DecoderQR.instance;

		if (gui == null)
			gui = GUIPanel.instance;
	}
	
	bool loggedIn = false;
	void InitLogin (Result data)
	{
		if (loggedIn)
			return;
			
		//wmsx.click.Play();
			
		loggedIn = true;
		
		GetUser (data.Text).Then ((User user) => {
		
		
		gui.text.text = "Bienvenido " + user.name;
		print ("FOUND USER");
		tts.Say (gui.text.text);
		
		PlayerPrefs.SetString ("user", user.name);
		PlayerPrefs.SetString ("id", user.id);
		
		gui.showImage = true;
		gui.sprite = Resources.Load<Sprite> ("WMSx/view/materials/userPhoto");
		
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
