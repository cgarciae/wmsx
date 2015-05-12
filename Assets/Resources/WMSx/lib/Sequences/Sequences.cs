using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Async {
	public static class Seq {
	
		public static IEnumerable Wait {get {
			while (true)
				yield return null;
		}}
		
		public static IEnumerable Do (Action f)
		{
			f();
			yield return null;
		}
		
		public static IEnumerable KeepDoing (Action f)
		{
			while (true) {
				f();
				yield return null;
			}
		}
		
		public static void Nothing (){}
		public static IEnumerable DoNothing = Do (Nothing);
		
		public static IEnumerable If (Func<bool> cond, IEnumerable ifTrue) {
			return If (cond, ifTrue, DoNothing);
		}
		
		public static IEnumerable If (Func<bool> cond, IEnumerable ifTrue, IEnumerable ifFalse)
		{
			IEnumerable seleted = cond()? ifTrue: ifFalse;
			
			var ator = seleted.GetEnumerator();
			while (ator.MoveNext())
				yield return ator.Current;
		}
		
		public static IEnumerable WaitWhile (Func<bool> cond)
		{
			while (cond()) {
				yield return null;
			}
		}
		
		public static IEnumerable While (Func<bool> cond, IEnumerable e)
		{
			var ator = e.GetEnumerator();
			while (cond()) {
				ator.MoveNext();
				yield return ator.Current;
			}
		}
		
		public static IEnumerable Then (this IEnumerable a, IEnumerable b)
		{
			var atorA = a.GetEnumerator();
			var atorB = b.GetEnumerator();
			
			while (atorA.MoveNext())
				yield return atorA.Current;
				
			while (atorB.MoveNext())
				yield return atorB.Current;
		}
		
		public static IEnumerable WaitForSeconds (float t)
		{
			yield return new UnityEngine.WaitForSeconds (t);
		}
		
		public static IEnumerable DelayN (int n)
		{
			while (n-- > 0)
			{
				yield return null;
			}
		}
		
		public static IEnumerable Delay1 {get{
			yield return null;
		}}
		
		public static Future GetFuture (this IEnumerable e, MonoBehaviour m) {
			
			var completer = new Completer();
			
			e.Then (Do (() => {
				completer.Complete();
			}))
			.Start(m);
			
			return completer;
		}
		
		public static Future<A> GetFuture<A> (this IEnumerable e, MonoBehaviour m) {
			
			var completer = new Completer<A>();
			
			e.Then<A> ((A a) => {
				completer.Complete (a);
			})
			.Start(m);
			
			return completer;
		}
		
		public static IEnumerable Then<A> (this IEnumerable e, Action<A> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			f ((A)ator.Current);
		}
		
		public static IEnumerable Then (this IEnumerable e, Action f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			f ();
		}
		
		public static IEnumerable Then<A> (this IEnumerable e, Func<A,IEnumerable> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			
			ator = f ((A)ator.Current).GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
		}
		
		public static IEnumerable Then (this IEnumerable e, Func<IEnumerable> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			
			ator = f ().GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
		}
		
		public static IEnumerable Expand<A> (this IEnumerable<A> e, Func<A,IEnumerable> f)
		{
			foreach (A a in e)
				foreach (var _ in f(a))
					yield return _;
		}
		
		public static IEnumerable Expand<A,B> (this IEnumerable<A> e, Func<A,IEnumerable<B>> f)
		{
			foreach (A a in e)
				foreach (B b in f(a))
					yield return b;
		}
		
		
		
		public static Coroutine Start (this IEnumerable e, MonoBehaviour m)
		{
			return m.StartCoroutine (e.GetEnumerator());
		}
	}
	
	
	public class StateMachine<A> : IEnumerable, Stream<A>
	{
		public A initialState;
		A _state;
		public Dictionary<A,StateBehaviour<A>> map;
		
		public StateMachine (A initialState, params StateBehaviour<A>[] behaviours)
		{
			this.initialState = this._state = initialState;
			this.map = new Dictionary<A, StateBehaviour<A>> ();
			
			foreach (var m in behaviours)
				map.Add (m.key, m);
		}
		
		public IEnumerator GetEnumerator ()
		{
			StateBehaviour<A> actual = map [initialState];
			StateBehaviour<A> nextBehaviour = null;
			IEnumerator enu = actual.GetEnumerator();
			bool move;
			
			actual.onEnter.Broadcast ();
			Action changeState = () => {
				//Debug.Log (_state);
				
				actual.onExit.Broadcast();
				nextBehaviour.onEnter.Broadcast();
				
				actual = nextBehaviour;
				enu = actual.GetEnumerator();
				
				stateStream.Broadcast (_state);
			};
			
			while ((move = enu.MoveNext()) || actual.transitive) 
			{
				if (! move)
				{
					_state = actual.onFinish();
					nextBehaviour = map [_state];
					
					changeState();
					
					continue;
				}
				
				yield return enu.Current;
				
				_state = actual.transitionFunction (_state);
				nextBehaviour = map [_state];
				
				
				if (nextBehaviour != actual)
				{
					changeState();
				}
			}
		}
		
		public A state {get {return _state;}}
		
		public static StateMachine<A> _ (A initialState, params StateBehaviour<A>[] behaviours)
		{
			return new StateMachine<A> (initialState, behaviours);	
		}
		
		#region Stream implementation
		
		StreamController<A> stateStream = new StreamController<A>();
		
		public Stream<A> OnData (Action<A> f)
		{
			stateStream.OnData (f);
			return this;
		}
		
		public Stream<A> RemoveDataHandler (Action<A> f)
		{
			stateStream.RemoveDataHandler (f);
			return this;
		}
		
		public Stream<A> RemoveErrorHandler (Action<Exception> f)
		{
			stateStream.RemoveErrorHandler (f);
			return this;
		}
		
		public Stream<A> RemoveDoneHandler (Action f)
		{
			stateStream.RemoveDoneHandler (f);
			return this;
		}
		
		public Stream<B> Map<B> (Func<A, B> f)
		{
			return stateStream.Map (f);
		}
		
		public Stream<B> Expand<B> (Func<A, Stream<B>> f, MonoBehaviour m)
		{
			return stateStream.Expand (f, m);
		}
		
		public bool done {
			get {
				return stateStream.done;
			}
		}
		
		#endregion
	}
	
	public class StateBehaviour<A> : IEnumerable
	{
		public A key;
		public Func<A, A> transitionFunction;
		public Func<A> onFinish;
		public bool restartOnEnter;
		
		public StreamController onEnter = new StreamController();
		public StreamController onExit = new StreamController();
		
		public bool transitive = false;
		
		IEnumerable behaviour;
		IEnumerator _enumerator;
		
		public IEnumerator GetEnumerator()
		{
			return restartOnEnter ? behaviour.GetEnumerator() : _enumerator;
		}
		
		public StateBehaviour (A key, Func<A, A> transitionFunction, IEnumerable behaviour, bool restartOnEnter = false)
		{
			this.key = key;
			this.behaviour = behaviour;
			this.transitionFunction = transitionFunction;
			this.restartOnEnter = restartOnEnter;
			
			_enumerator = behaviour.GetEnumerator ();
		}
		
		public StateBehaviour (A key, Func<A, A> transitionFunction, IEnumerable behaviour, Func<A> onFinish, bool restartOnEnter = false) : this (key, transitionFunction, behaviour, restartOnEnter)
		{
			this.transitive = true;
			this.onFinish = onFinish;
		}
		
		public static StateBehaviour<A> _ (A key, Func<A, A> transitionFunction, IEnumerable behaviour, bool restartOnEnter = false)
		{
			return new StateBehaviour<A> (key, transitionFunction, behaviour, restartOnEnter);
		}
		
		public static StateBehaviour<A> _ (A key, Func<A, A> f, IEnumerable behaviour, Func<A> onFinish, bool restartOnEnter = false)
		{
			return new StateBehaviour<A> (key, f, behaviour, onFinish, restartOnEnter);
		}
	}
	
	public class TerminalState<A> : StateBehaviour<A>
	{
		
		public TerminalState (A key) : base (key, Utils.Id<A>, Seq.DoNothing)
		{
			
		}
		
		public static TerminalState<A> _ (A key)
		{
			return new TerminalState<A> (key);	
		}
		
	}
	
	public class AbsorvingState<A> : StateBehaviour<A>
	{
		public AbsorvingState (A key, IEnumerable behaviour) : base (key, Utils.Id<A>, behaviour) {}
		
		public static AbsorvingState<A> _ (A key, IEnumerable behaviour)
		{
			return new AbsorvingState<A> (key, behaviour);	
		}
	}
}

