using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Async;
using ZXing;


public class Picking : View {
	
	
	public Image imagen;
	public Text text;
	public GameObject background;
	
	DecoderQR decoder;
	PickingState state = PickingState.FindingLocation;
	StateMachine<PickingState> stateMachine;
	Task task;
	IEnumerator<Location> locations;
	String lastRecognition = "";
	float timeLastRecognition;
	
	Location currentLocation {get {return locations.Current;}}

	#region implemented abstract members of View

	public override void New (object _info)
	{
		
	}

	public override void ViewStart ()
	{
		var findingLocation = new StateBehaviour<PickingState> (
			PickingState.FindingLocation,
			GetState,
			findingLocationBehaviour,
			true);
		findingLocation.onEnter.OnData (() => {
			print ("FINDIND LOCATION");
			//Siguiente Taks, salirse si era la ultima
			if (! locations.MoveNext())
			{
				print ("NEXT FIND PUT LOCATION");
				state = PickingState.FindingPutLocation;
				return;
			}
			//Set View
			text.text = String.Format("Ve a la ubicacion {0}", currentLocation.id);
			imagen.sprite = Resources.Load<Sprite> ("WMSx/view/materials/mapas/" +  currentLocation.id);
		});
		
		var findingProduct = new StateBehaviour<PickingState> (
			PickingState.FindingProduct,
			GetState,
			findingProductsBehaviour,
			true);
		
		
		var findingPutLocation = new AbsorvingState<PickingState> (
			PickingState.FindingPutLocation,
			findingPutLocationBehaviour);
		findingPutLocation.onEnter.OnData(() => {
			text.text = String.Format("Ve a la ubicacion {0}", task.putLocation.id);
			imagen.sprite = Resources.Load<Sprite> ("WMSx/view/materials/mapas/" +  task.putLocation.id);
		});
			
		
		stateMachine = new StateMachine<PickingState> (
			PickingState.FindingLocation,
			findingLocation, findingProduct, findingPutLocation);
			
		
		GetTask().Then ((ts) => {
			task = ts;
			locations = ts.locations.GetEnumerator();
			stateMachine.Start(this);
		});
	}

	public override void ViewAwake ()
	{
		decoder = DecoderQR.instance;
		decoder.onDetection.OnData ((data) => {
			lastRecognition = data.Text;
			timeLastRecognition = Time.time;
		});
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

	PickingState GetState(PickingState _)
	{
		return state;
	}
	
	bool RecentlySaw (String pattern, float span)
	{
		var cond = 
			lastRecognition == pattern &&
			Time.time <= timeLastRecognition+ span;
			
		if (cond) {
			print ("Saw " + pattern);
			WMSx.instance.click.Play();
		}
		
		return cond;
	}
	
	IEnumerable findingLocationBehaviour {get{
		return Seq.KeepDoing(() => {
			if (RecentlySaw (currentLocation.id, 0.5f))
			{
				state = PickingState.FindingProduct;
			}
		});
	}}
	
	IEnumerable findingProductsBehaviour {get{
		return Seq.DoNothing.Then (() => currentLocation.products.Expand ((Product product) => {
			text.text = String.Format("Busca el producto {0} - {1}", product.name, product.id);
			imagen.sprite = Resources.Load<Sprite> ("WMSx/view/materials/productos/" +  product.id);
			
			return Seq.WaitWhile(() => ! RecentlySaw (product.id, 0.5f));
			})
			.Then(() => {
				print ("FIND NEXT LOCATION");
				state = PickingState.FindingLocation;
			})
			.Then(Seq.Wait)
		);
	}}
	
	IEnumerable findingPutLocationBehaviour {get{
		return Seq.KeepDoing(() => {
			if (RecentlySaw (task.putLocation.id, 0.5f))
			{
				WMSx.state = WorkerState.LoggedOut;
			}
		});
	}}
	
	Future<Task> GetTask ()
	{
		var p1 = new Product ("p1", "Cerveza");
		var p2 = new Product ("p2", "Chocolate");
		
		var l1 = new Location ("l1", new List<Product>(){p1});
		var l2 = new Location ("l2", new List<Product>(){p2});
		
		var locations = new List<Location> (){l1, l2};
		var putLocation = new Location ("put");
		
		return new Completer<Task> (new Task (locations, putLocation));
	}
	
}

public enum PickingState
{
	FindingLocation,
	FindingProduct,
	FindingPutLocation
}

public class Task
{
	
	public List<Location> locations;
	public Location putLocation;
	
	
	public Task () {}
	public Task (List<Location> locations, Location putLocation)
	{
		this.locations = locations;
		this.putLocation = putLocation;
	}
	
	
}

public class Location
{
	public Location (string idUbicacion) : this (idUbicacion, new List<Product>())
	{
		this.id = idUbicacion;
	}
	
	public Location (string idUbicacion, List<Product> products)
	{
		this.id = idUbicacion;
		this.products = products;
	}
	
	
	public String id;
	public List<Product> products;
}

public class Product
{
	public String id;
	public String name;
	
	public Product (string productId, string productName)
	{
		this.id = productId;
		this.name = productName;
	}
	
}

