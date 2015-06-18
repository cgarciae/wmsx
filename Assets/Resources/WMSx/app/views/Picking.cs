using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Async;
using ZXing;


public class Picking : View {
	
	
	public GUIPanel gui;
	
	WMSx wmsx;
	ITTS tts;
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
		GetDependencies();

		int remainingLocations = 0;
		
		var findingLocation = new StateBehaviour<PickingState> (
			PickingState.FindingLocation,
			GetState,
			findingLocationBehaviour,
			true);
		findingLocation.onEnter.OnData (() => {
			print ("FINDIND LOCATION");
			gui.status.text = "Buscando Ubicacion";
			gui.quantity.text = remainingLocations.ToString();
			//Siguiente Taks, salirse si era la ultima
			if (! locations.MoveNext())
			{
				print ("NEXT FIND PUT LOCATION");
				state = PickingState.FindingPutLocation;
				return;
			}
			//Set View
			gui.text.text = String.Format("Ve a la ubicacion {0}", currentLocation.id);
			tts.Say (gui.text.text);
			gui.sprite = Resources.Load<Sprite> ("WMSx/view/materials/mapas/" +  currentLocation.id);
		});

		findingLocation.onExit.OnData (() => {
			remainingLocations--;
		});
		
		var findingProduct = new StateBehaviour<PickingState> (
			PickingState.FindingProduct,
			GetState,
			findingProductsBehaviour,
			true);

		findingProduct.onEnter.OnData (() => {
			gui.status.text = "Buscando Producto";
		});
		
		var findingPutLocation = new AbsorvingState<PickingState> (
			PickingState.FindingPutLocation,
			findingPutLocationBehaviour);

		findingPutLocation.onEnter.OnData(() => {
			gui.text.text = String.Format("Ve a la ubicacion {0}", task.putLocation.id);

			gui.status.text = "Putting";
			gui.quantity.text = "";

			tts.Say (gui.text.text);
			gui.sprite = Resources.Load<Sprite> ("WMSx/view/materials/mapas/" +  task.putLocation.id);
		});
			
		
		stateMachine = new StateMachine<PickingState> (
			PickingState.FindingLocation,
			findingLocation, findingProduct, findingPutLocation);
			
		
		GetTask().Then ((ts) => {
			task = ts;
			locations = ts.locations.GetEnumerator();
			remainingLocations = ts.locations.Count;
			stateMachine.Start(this);
		});
	}
	
	void GetDependencies()
	{
		if (wmsx == null)
			wmsx = WMSx.instance;
			
		if (decoder == null)
			decoder = DecoderQR.instance;
			
		if (tts == null)
			tts = TTS.instance;

		if (gui == null)
			gui = GUIPanel.instance;
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


		print (pattern);
		print (lastRecognition);
		print (pattern == lastRecognition);
		print (cond);

			
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
		
		return Seq.DoNothing.Then (() => {
			var n = currentLocation.products.Count;
			return currentLocation.products.Expand ((Product product) => {
				gui.text.text = String.Format("Busca el producto {0} - {1}", product.name, product.id);
				gui.quantity.text = (n--).ToString();
				tts.Say (gui.text.text);
				gui.sprite = Resources.Load<Sprite> ("WMSx/view/materials/productos/" +  product.id);
				
				return Seq.WaitWhile(() => ! RecentlySaw (product.id, 0.5f));
				})
				.Then(() => {
					print ("FIND NEXT LOCATION");
					state = PickingState.FindingLocation;
				})
				.Then(Seq.Wait);
			});
	}}
	
	IEnumerable findingPutLocationBehaviour {get{
		return Seq.KeepDoing(() => {
			if (RecentlySaw (task.putLocation.id, 0.5f))
			{
				wmsx.state = WorkerState.LoggedOut;
			}
		});
	}}
	
	Future<Task> GetTask ()
	{
		var p1 = new Product ("7706634002948", "Guante");
		var p2 = new Product ("7702184030035", "Jeringa");
		var p3 = new Product ("7706634000494", "Jabon");
		
		var l1 = new Location ("07010606D01", new List<Product>(){p1});
		var l2 = new Location ("07011204B01", new List<Product>(){p2, p3});
		
		var locations = new List<Location> (){l1, l2};
		var putLocation = new Location ("07052606F00");
		
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

