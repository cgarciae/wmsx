using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Async {

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
			
			Debug.Log (actual.key);
			
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
		
		public Stream<A> Filter (Func<A, bool> cond)
		{
			return stateStream.Filter (cond);
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
