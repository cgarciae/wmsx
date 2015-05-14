using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Async {

	public interface Stream<A>  {
		
		Stream<A> OnData (Action<A> f);
		//Future<A> First ();
		
		Stream<A> RemoveDataHandler (Action<A> f);
		Stream<A> RemoveErrorHandler (Action<Exception> f);
		Stream<A> RemoveDoneHandler (Action f);
		
		Stream<B> Map<B> (Func<A,B> f);
		Stream<A> Filter (Func<A,bool> cond);
		Stream<B> Expand<B> (Func<A,Stream<B>> f, MonoBehaviour m);
		bool done {get;}
		}
	
	public interface Stream  {
		
		Stream OnData (Action f);
		//Future First ();
		
		Stream RemoveDataHandler (Action f);
		Stream RemoveErrorHandler (Action<Exception> f);
		Stream RemoveDoneHandler (Action f);
		
		Stream<A> Map<A> (Func<A> f);
		Stream<A> Expand<A> (Func<Stream<A>> f, MonoBehaviour m);
		bool done {get;}
		
//		Stream RemoveDataHandler (Action f);
//		Stream RemoveErrorHandler (Action<Exception> f);
//		Stream RemoveDoneHandler (Action f);
//		Stream OnError (Action<Exception> f);
//		Stream OnDone (Action f);
//		void BroadcastDone ();
	}
	
	
	public class StreamController : Stream {
		
		bool _done;
		
		public void Broadcast ()
		{
			if (done)
				return;
			
			foreach (var f in dataHandlers)
			{
				f ();
			}	
		}
		
		
		public HashSet<Action> dataHandlers = new HashSet<Action>();
		public HashSet<Action<Exception>> errorHandlers = new HashSet<Action<Exception>>();
		public HashSet<Action> doneHandlers = new HashSet<Action>();
		
		
		public Stream RemoveDataHandler (Action f)
		{
			dataHandlers.Remove (f);
			return this;
		}

		public Stream RemoveErrorHandler (Action<Exception> f)
		{
			errorHandlers.Remove (f);
			return this;
		}

		public Stream RemoveDoneHandler (Action f)
		{
			doneHandlers.Remove (f);
			return this;
		}
		
//		public virtual void BroadcastDone ()
//		{
//			if (done)
//				return;
//			
//			doneHandlers.ToList().ForEach(f => f());
//			
//			
//			done = true;
//		}
		
		public Stream OnData (Action f)
		{
			dataHandlers.Add (f);
			return this;
		}
		
		
		
		#region Stream implementation
		public Stream<A> Map<A> (Func<A> f)
		{
			var streamController = new StreamController<A>();
			
			OnData (() => {
				streamController.Broadcast(f());
			});
			
			return streamController;
		}
		public Stream<A> Expand<A> (Func<Stream<A>> f, MonoBehaviour m)
		{
			var streamController = new StreamController<A>();
			
			OnData(()=> {
				f().OnData (streamController.Broadcast);
			});
			
			return streamController;
		}
		
		public bool done {
			get {
				return _done;
			}
		}
		#endregion
	}
	
	/*
	public class StreamPort
	{
		public Action onData;
		public Action<Exception> onError;
		public Action onDone;
		
		public Stream stream;
		
		
		public StreamPort (Stream stream)
		{
			this.stream = stream;
		}
		
		public StreamPort OnData (Action f)
		{
			stream
				.RemoveDataHandler (onData)
				.OnData (f);
			
			onData = f;
			return this;
		}
		
		public StreamPort OnError (Action<Exception> f)
		{
			stream
				.RemoveErrorHandler (onError)
				.OnError (f);
			
			onError = f;
			return this;
		}
		
		public StreamPort OnDone (Action f)
		{
			stream
				.RemoveDoneHandler (onDone)
				.OnDone (f);
			
			onDone = f;
			return this;
		}
		
		public StreamPort Unsubscribe ()
		{
			stream
				.RemoveDataHandler (onData)
				.RemoveErrorHandler (onError)
				.RemoveDoneHandler (onDone);
			
			return this;
		}

		public static StreamPort operator + (StreamPort s, Action f)
		{
			return s.OnData (f);
		}
	}
	*/

	public class StreamController<A> : Stream<A>  {

		public HashSet<Action<A>> dataHandlers = new HashSet<Action<A>>();
		public HashSet<Action<Exception>> errorHandlers = new HashSet<Action<Exception>>();
		public HashSet<Action> doneHandlers = new HashSet<Action>();

		public bool _done = false;

		public virtual void Broadcast (A data)
		{
			if (_done)
				return;

			foreach (var f in dataHandlers.ToList())
			{
				f (data);
			}	
		}

		public virtual void BroadcastError (Exception e)
		{
			if (_done)
				return;

			foreach (var f in errorHandlers.ToList()) 
			{
				f (e);
			}
		}

		public virtual void BroadcastDone ()
		{
			if (_done)
				return;

			foreach (var f in doneHandlers.ToList()) 
			{
				f ();
			}

			_done = true;
		}

		public Stream<A> OnData (Action<A> f)
		{
			dataHandlers.Add (f);
			return (Stream<A>)this;
		}

		public Stream<A> OnError (Action<Exception> f)
		{
			errorHandlers.Add (f);
			return (Stream<A>)this;
		}

		public Stream<A> OnDone (Action f)
		{
			doneHandlers.Add (f);
			return (Stream<A>)this;
		}

		public Stream<A> RemoveDataHandler (Action<A> f)
		{
			dataHandlers.Remove (f);
			return (Stream<A>)this;
		}

		public Stream<A> RemoveErrorHandler (Action<Exception> f)
		{
			errorHandlers.Remove (f);
			return (Stream<A>)this;
		}

		public Stream<A> RemoveDoneHandler (Action f)
		{
			doneHandlers.Remove (f);
			return (Stream<A>)this;
		}

		public static Stream<A> operator + (StreamController<A> s, Action<A> f)
		{
			return s.OnData (f);
		}
		
		public static Stream<A> operator - (StreamController<A> s, Action<A> f)
		{
			return s.RemoveDataHandler (f);
		}

		public Future<A> First {
			get {
				var broadcasted = false;
				var completer = new Completer<A>();

				OnData ((val) => {
					if (broadcasted)
						return;
					
					broadcasted = true;
					completer.Complete(val);
				});

				return completer;
			}
		}
		
		public Stream<B> Map<B> (Func<A, B> f)
		{
			var streamController = new StreamController<B>();
			
			OnData ((A a) => {
				streamController.Broadcast(f(a));
			});
			
			return streamController;
		}

		public Stream<B> Expand<B> (Func<A, Stream<B>> f, MonoBehaviour m)
		{
			var streamController = new StreamController<B>();
			
			OnData((A a)=> {
				f(a).OnData (streamController.Broadcast);
			});
			
			return streamController;
		}
		
		public Stream<A> Filter (Func<A, bool> cond)
		{
			var controller = new StreamController<A>();
			OnData ((A a) => {
				if (cond(a))
					controller.Broadcast (a);
			});
			return controller;
		}

		public bool done {
			get {
				return _done;
			}
		}
	}

	/*
	public class StreamPort<A>
	{
		public Action<A> onData;
		public Action<Exception> onError;
		public Action onDone;

		public Stream<A> stream;


		public StreamPort (Stream<A> stream)
		{
			this.stream = stream;
		}
		
		public StreamPort<A> OnData (Action<A> f)
		{
			stream
				.RemoveDataHandler (onData)
				.OnData (f);

			onData = f;
			return this;
		}

		public StreamPort<A> OnError (Action<Exception> f)
		{
			stream
				.RemoveErrorHandler (onError)
				.OnError (f);

			onError = f;
			return this;
		}

		public StreamPort<A> OnDone (Action f)
		{
			stream
				.RemoveDoneHandler (onDone)
				.OnDone (f);

			onDone = f;
			return this;
		}

		public StreamPort<A> Unsubscribe ()
		{
			stream
				.RemoveDataHandler (onData)
				.RemoveErrorHandler (onError)
				.RemoveDoneHandler (onDone);

			return this;
		}

		public static StreamPort<A> operator + (StreamPort<A> s, Action<A> f)
		{
			return s.OnData (f);
		}
	}
	*/
}
