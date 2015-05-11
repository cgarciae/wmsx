using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Futures;

namespace Streams {

//	public interface Stream<A>  {
//		
//		Stream OnData (Func<A> f);
//		Future<A> First ();
//		
//		Stream<B> Map<B> (Func<A,B> f);
//		Stream<B> Expand<B> (Func<A,Stream<B>> f);
//		}
//	
//	public interface Stream  {
//		
//		Stream OnData (Action f);
//		Future First ();
//		
//		Stream<A> Map<A> (Func<A> f);
//		Stream<A> Expand<A> (Func<Stream<A>> f);
//		bool done {get;}
//		
////		Stream RemoveDataHandler (Action f);
////		Stream RemoveErrorHandler (Action<Exception> f);
////		Stream RemoveDoneHandler (Action f);
////		Stream OnError (Action<Exception> f);
////		Stream OnDone (Action f);
////		void BroadcastDone ();
//	}
//	
//	
//	public class StreamController : Stream {
//		
//		bool _done;
//		
//		public void Broadcast ()
//		{
//			if (done)
//				return;
//			
//			foreach (var f in dataHandlers.ToList())
//			{
//				f ();
//			}	
//		}
//		
//		
//		public HashSet<Action> dataHandlers = new HashSet<Action>();
////		public HashSet<Action<Exception>> errorHandlers = new HashSet<Action<Exception>>();
////		public HashSet<Action> doneHandlers = new HashSet<Action>();
//		
//		
////		public virtual void BroadcastDone ()
////		{
////			if (done)
////				return;
////			
////			doneHandlers.ToList().ForEach(f => f());
////			
////			
////			done = true;
////		}
//		
//		public void OnData (Action f)
//		{
//			dataHandlers.Add (f);
//			return this;
//		}
//		
//		public Future First ()
//		{
//			var completer = new Completer();
//			bool broadcasted = false;
//			OnData (() => {
//				if (broadcasted)
//					return;
//				broadcasted = true;
//				completer.Complete();
//			});
//			return completer;
//		}
//		
//		
//		#region Stream implementation
//		public Stream<A> Map<A> (Func<A> f)
//		{
//			var streamController = new StreamController<A>();
//			
//			Action onData = () => {
//				streamController.Broadcast(f());
//			};
//			
//			return streamController;
//		}
//		public Stream<A> Expand<A> (Func<Stream<A>> f)
//		{
//			throw new NotImplementedException ();
//		}
//		public bool done {
//			get {
//				return _done;
//			}
//		}
//		#endregion
//	}
//	
//
//	public class StreamPort
//	{
//		public Action onData;
//		public Action<Exception> onError;
//		public Action onDone;
//		
//		public Stream stream;
//		
//		
//		public StreamPort (Stream stream)
//		{
//			this.stream = stream;
//		}
//		
//		public StreamPort OnData (Action f)
//		{
//			stream
//				.RemoveDataHandler (onData)
//				.OnData (f);
//			
//			onData = f;
//			return this;
//		}
//		
//		public StreamPort OnError (Action<Exception> f)
//		{
//			stream
//				.RemoveErrorHandler (onError)
//				.OnError (f);
//			
//			onError = f;
//			return this;
//		}
//		
//		public StreamPort OnDone (Action f)
//		{
//			stream
//				.RemoveDoneHandler (onDone)
//				.OnDone (f);
//			
//			onDone = f;
//			return this;
//		}
//		
//		public StreamPort Unsubscribe ()
//		{
//			stream
//				.RemoveDataHandler (onData)
//				.RemoveErrorHandler (onError)
//				.RemoveDoneHandler (onDone);
//			
//			return this;
//		}
//
//		public static StreamPort operator + (StreamPort s, Action f)
//		{
//			return s.OnData (f);
//		}
//	}
//
//	public class Stream<A>  {
//
//		public HashSet<Action<A>> dataHandlers = new HashSet<Action<A>>();
//		public HashSet<Action<Exception>> errorHandlers = new HashSet<Action<Exception>>();
//		public HashSet<Action> doneHandlers = new HashSet<Action>();
//
//		public bool done = false;
//
//		public virtual void Broadcast (A a)
//		{
//			if (done)
//				return;
//
//			foreach (var f in dataHandlers.ToList())
//			{
//				f (a);
//			}	
//		}
//
//		public virtual void BroadcastError (Exception e)
//		{
//			if (done)
//				return;
//
//			foreach (var f in errorHandlers.ToList()) 
//			{
//				f (e);
//			}
//		}
//
//		public virtual void BroadcastDone ()
//		{
//			if (done)
//				return;
//
//			foreach (var f in doneHandlers.ToList()) 
//			{
//				f ();
//			}
//
//			done = true;
//		}
//
//		public Stream<A> OnData (Action<A> f)
//		{
//			dataHandlers.Add (f);
//			return this;
//		}
//
//		public Stream<A> OnError (Action<Exception> f)
//		{
//			errorHandlers.Add (f);
//			return this;
//		}
//
//		public Stream<A> OnDone (Action f)
//		{
//			doneHandlers.Add (f);
//			return this;
//		}
//
//		public Stream<A> RemoveDataHandler (Action<A> f)
//		{
//			dataHandlers.Remove (f);
//			return this;
//		}
//
//		public Stream<A> RemoveErrorHandler (Action<Exception> f)
//		{
//			errorHandlers.Remove (f);
//			return this;
//		}
//
//		public Stream<A> RemoveDoneHandler (Action f)
//		{
//			doneHandlers.Remove (f);
//			return this;
//		}
//
//		public MapStream<A,B> FMap<B> (Func<A,B> f)
//		{
//			return new MapStream<A,B> (f, this);
//		}
//
//		public StreamPort<A> Listen ()
//		{
//			return new StreamPort<A> (this);	
//		}
//
//		public static Stream<A> operator + (Stream<A> s, Action<A> f)
//		{
//			return s.OnData (f);
//		}
//		
//		public static Stream<A> operator - (Stream<A> s, Action<A> f)
//		{
//			return s.RemoveDataHandler (f);
//		}
//
//		public Future<A> First {
//			get {
//				var broadcasted = false;
//				var completer = new Completer<A>();
//
//				OnData ((val) => {
//					if (broadcasted)
//						return;
//					
//					broadcasted = true;
//					completer.Complete(val);
//				});
//
//				return completer;
//			}
//		}
//	}
//
//	public class MapStream<A,B> : Stream<B> 
//	{
//		public Action<A> onData;
//
//		public Stream<A> origin;
//
//		public MapStream (Func<A, B> f, Stream<A> origin)
//		{
//			this.onData = a => this.Broadcast (f(a));
//			this.origin = origin;
//
//			origin.OnData (this.onData);
//			origin.OnError (this.BroadcastError);
//		}
//		
//		public void BreakStream ()
//		{
//			origin
//				.RemoveDataHandler (onData)
//				.RemoveErrorHandler (BroadcastError)
//				.RemoveDoneHandler (BroadcastDone);
//
//			BroadcastDone ();
//		}
//	}
//
//	public class StreamPort<A>
//	{
//		public Action<A> onData;
//		public Action<Exception> onError;
//		public Action onDone;
//
//		public Stream<A> stream;
//
//
//		public StreamPort (Stream<A> stream)
//		{
//			this.stream = stream;
//		}
//		
//		public StreamPort<A> OnData (Action<A> f)
//		{
//			stream
//				.RemoveDataHandler (onData)
//				.OnData (f);
//
//			onData = f;
//			return this;
//		}
//
//		public StreamPort<A> OnError (Action<Exception> f)
//		{
//			stream
//				.RemoveErrorHandler (onError)
//				.OnError (f);
//
//			onError = f;
//			return this;
//		}
//
//		public StreamPort<A> OnDone (Action f)
//		{
//			stream
//				.RemoveDoneHandler (onDone)
//				.OnDone (f);
//
//			onDone = f;
//			return this;
//		}
//
//		public StreamPort<A> Unsubscribe ()
//		{
//			stream
//				.RemoveDataHandler (onData)
//				.RemoveErrorHandler (onError)
//				.RemoveDoneHandler (onDone);
//
//			return this;
//		}
//
//		public static StreamPort<A> operator + (StreamPort<A> s, Action<A> f)
//		{
//			return s.OnData (f);
//		}
//	}
}
