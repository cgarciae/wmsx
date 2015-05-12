using UnityEngine;
using System.Collections;
using Async;

public class WMSx : MonoBehaviour {

	public static WorkerState state = WorkerState.LoggedOut;
	public static View view;
	static string viewsPath = "views/prefabs/";
	
	// Use this for initialization
	void Start () 
	{
		//Set logout
		var loggedOut = new StateBehaviour<WorkerState> (
			WorkerState.LoggedOut,
			GetState,
			Seq.DoNothing, false);
		
		loggedOut.onEnter.OnData (SetLogin);
		
			
		var selectingTaks = new StateBehaviour<WorkerState> (
			WorkerState.SelectingTask,
			GetState,
			Seq.DoNothing);
			
		var picking = new StateBehaviour<WorkerState> (
			WorkerState.Picking,
			GetState,
			Seq.DoNothing);
			
		var putting = new StateBehaviour<WorkerState> (
			WorkerState.Putting,
			GetState,
			Seq.DoNothing);
			
		var stateMachine = new StateMachine<WorkerState> (
			state,
			loggedOut, selectingTaks, picking, putting);
	}
	
	WorkerState GetState(WorkerState _)
	{
		return state;
	}
	
	void SetView (View newView) {
		if (view != null)
		{
			view.gameObject.SetActive (false);
			Destroy (view);
		}
		
		view = newView;
	}
	
	void SetLogin ()
	{
		Utils.LoadGameObject<View> (viewsPath + "Login").Then ((View v) => {
			
		v.New (null);
		});
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public abstract class View : MonoBehaviour {
	public abstract void New (Object _info);
}


public enum WorkerState
{
	LoggedOut,
	SelectingTask,
	Picking,
	Putting
}
