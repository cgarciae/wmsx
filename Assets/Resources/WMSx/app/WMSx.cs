using UnityEngine;
using System.Collections;
using System;
using Async;

public class WMSx : MonoBehaviour {
	
	public AudioSource click;
	
	private static WMSx _instance;
	public static WMSx instance {get{return _instance;}}
	
	public WorkerState state = WorkerState.LoggedOut;
	public View view;
	private string viewsPath = "WMSx/view/prefabs/";
	
	private TTS tts;
	
	void Awake ()
	{
		_instance = this;
	}
	
	// Use this for initialization
	void Start () 
	{
		GetDependencies();
		//Set logout
		var loggedOut = new StateBehaviour<WorkerState> (
			WorkerState.LoggedOut,
			GetState,
			Seq.Wait);
		loggedOut.onEnter.OnData(LoadAndSet<Login> (viewsPath + "Login", null));
		loggedOut.onEnter.OnData(()=>{
			print ("ENTER LOGIN");
		});
		
			
		var selectingTaks = new StateBehaviour<WorkerState> (
			WorkerState.SelectingTask,
			(_) => state = WorkerState.Picking,
			Seq.Wait);
		selectingTaks.onEnter.OnData (() => {
			print ("Enter Selecting Task");
			SetView (null);
		});
			
		var picking = new StateBehaviour<WorkerState> (
			WorkerState.Picking,
			GetState,
			Seq.Wait);
		picking.onEnter.OnData(() => {
			LoadAndSet<Picking> (viewsPath + "Picking", null)();
			print ("ENTER PICKING");
		});
		
			
		var putting = new StateBehaviour<WorkerState> (
			WorkerState.Putting,
			GetState,
			Seq.Wait);
		putting.onEnter.OnData(() => {
			SetView(null);
		});
			
		var stateMachine = new StateMachine<WorkerState> (
			state,
			loggedOut, selectingTaks, picking, putting);
			
			
		stateMachine.Start (this);


	}
	
	void GetDependencies()
	{
		if (tts == null)
			tts = TTS.instance;
	}
	
	WorkerState GetState(WorkerState _)
	{
		return state;
	}
	
	void SetView (View newView) {
		if (view != null)
		{
			view.gameObject.SetActive (false);
			
			if (view.stateless)
				Destroy (view.gameObject);
		}
		
		view = newView;
	}
	
	void SetLogin ()
	{
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	Action LoadAndSet<A> (string path, object info) where A : View {
		return () => 
		{
			Utils.LoadGameObject<A> (path, this).Then ((A v) => {
				v.transform.RectTransform().SetParent(this.transform);
				v.transform.ResetCoordinates();
				v.New (info);
				SetView (v);
			});
		};
	}
}
	
	
public abstract class View : MonoBehaviour {
		public bool started {get; set;}
		public abstract bool stateless {get;}
		
		public abstract void New (object _info);
		
		public void Awake () {
			
			started = false;
			ViewAwake();
		} 
		public void Start () {
			started = true;
			ViewStart();
			ViewOnEnable();
		}
		
		public abstract void ViewStart();
		public abstract void ViewAwake();
		public abstract void ViewOnEnable();
		
		public virtual void OnEnable ()
		{
			if (! started)
				return;
				
			ViewOnEnable();
		}
	
}


public enum WorkerState
{
	LoggedOut,
	SelectingTask,
	Picking,
	Putting
}
