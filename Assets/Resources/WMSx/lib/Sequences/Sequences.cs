using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Futures;

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
	
	public static Coroutine Start (this IEnumerable e, MonoBehaviour m)
	{
		return m.StartCoroutine (e.GetEnumerator());
	}
}


public class StateMachine<A> : IEnumerable
{
	public A initialState;
	A _state;
	public Dictionary<A,StateBehaviour<A>> map;
	
	public StateMachine (A initialState, params StateBehaviour<A>[] behaviours)
	{
		this.initialState = this._state = initialState;
		this.map = new Dictionary<A, StateBehaviour<A>> ();
		
		behaviours.ForEach (m => map.Add (m.key, m));
	}
	
	internal override IEnumerable GetEnumerable ()
	{
		StateBehaviour<A> actual = map [initialState];
		StateBehaviour<A> nextBehaviour = null;
		IEnumerator enu = actual.enumerator;
		bool move;
		
		actual.onEnter.Broadcast ();
		Action changeState = () => {
			//Debug.Log (_state);
			
			actual.onExit.Broadcast();
			nextBehaviour.onEnter.Broadcast();
			
			actual = nextBehaviour;
			enu = actual.enumerator;
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
}

public class StateBehaviour<A> : Atom
{
	public A key;
	public Func<A, A> transitionFunction;
	public Func<A> onFinish;
	public Atom atom;
	public bool restartOnEnter;
	
	public Stream onEnter = new Stream();
	public Stream onExit = new Stream();
	
	public bool transitive = false;
	
	IEnumerator enu;
	public IEnumerator enumerator
	{
		get 
		{
			return restartOnEnter ? (atom.copy as Atom).GetEnumerator() : enu;
		}
	}
	
	public StateBehaviour (A key, Func<A, A> f, Atom atom, bool restartOnEnter = false)
	{
		this.key = key;
		this.atom = atom.copy as Atom;
		this.transitionFunction = f;
		this.restartOnEnter = restartOnEnter;
		
		enu = atom.GetEnumerator ();
	}
	
	public StateBehaviour (A key, Func<A, A> f, Atom atom, Func<A> onFinish, bool restartOnEnter = false) : this (key, f, atom, restartOnEnter)
	{
		this.transitive = true;
		this.onFinish = onFinish;
	}
	
	internal override IEnumerable GetEnumerable ()
	{
		return atom;
	}
	
	public static StateBehaviour<A> _ (A key, Func<A, A> transitionFunction, Atom atom, bool restartOnEnter = false)
	{
		return new StateBehaviour<A> (key, transitionFunction, atom, restartOnEnter);
	}
	
	public static StateBehaviour<A> _ (A key, Func<A, A> f, Atom atom, Func<A> onFinish, bool restartOnEnter = false)
	{
		return new StateBehaviour<A> (key, f, atom, onFinish, restartOnEnter);
	}
}

public class TerminalState<A> : StateBehaviour<A>
{
	
	public TerminalState (A key) : base (key, Fn.Id<A> (), Atom.NullAtom)
	{
		
	}
	
	public static TerminalState<A> _ (A key)
	{
		return new TerminalState<A> (key);	
	}
	
}

public class AbsorvingState<A> : StateBehaviour<A>
{
	
	public AbsorvingState (A key, Atom atom) : base (key, Fn.Id<A> (), atom)
	{
		
	}
	
	public static AbsorvingState<A> _ (A key, Atom atom)
	{
		return new AbsorvingState<A> (key, atom);	
	}
}

