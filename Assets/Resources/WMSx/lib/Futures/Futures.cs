using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Async
{
	public interface Future<A> : IEnumerable
	{
		
		Future<B> Then<B> (Func<A,B> f);
		Future<B> Then<B> (Func<B> f);
		Future<B> Then<B> (Func<A,Future<B>> f);
		Future<B> Then<B> (Func<Future<B>> f);	
		Future<A> Then (Action<A> f);
		Future<A> Then (Action f);
		
		//TODO: Future<A> CatchError<E> (Action<E> g);
		
		bool completed {get;}
		
	}
	
	public abstract class FutureBase<A> : Future<A>
	{
		public abstract Future<C> Return<C> (C value);
		
		public abstract Future<B> Then<B> (Func<A,Future<B>> f);
		
		public Future<B> Then<B> (Func<A,B> f)
		{
			return Then<B> ((a) => Return (f(a)));
		}
		
		public Future<B> Then<B> (Func<B> f)
		{
			return Then ((A _) => Return (f()));
		}
		
		public Future<B> Then<B> (Func<Future<B>> f)
		{
			return Then ((A _) => f ());
		}
		
		public Future<A> Then (Action<A> f)
		{
			return Then (f.ToFunc());
		}
		
		public Future<A> Then (Action f)
		{
			return Then ((A _) => f());
		}
		
		public abstract bool completed {get;}


		#region IEnumerable implementation

		public virtual IEnumerator GetEnumerator ()
		{
			while (! completed)
				yield return null;
		}

		#endregion
	}
	
	public class Completer<A> : FutureBase<A>
	{
		bool _completed = false;
		A _value;
		
		List<Action> actions = new List<Action>();
		
		public Completer () {}
		
		public Completer (A value)
		{
			this._completed = true;
			this._value = value;
		}
		
		public override Future<C> Return<C> (C value)
		{
			return new Completer<C> (value);
		}
		
		public override Future<B> Then<B> (Func<A, Future<B>> f)
		{
			if (completed)
				return f (_value);
			
			var completer = new Completer<B> ();
			
			Action onComplete = () => {
				
				Future<B> future = f (_value);
				future.Then ((B b) => completer.Complete(b));
			};
			
			actions.Add (onComplete);
			
			return (Future<B>) completer;
		}
		
		public override bool completed {
			get {
				return _completed;
			}
		}
		
		public void Complete (A value)
		{
			this._value = value;
			this._completed = true;
			actions.ForEach ((Action f) => f());
		}
		
		public override IEnumerator GetEnumerator ()
		{
			while (! completed)
				yield return null;
				
			yield return _value;
		}
	}
	
	public interface Future : IEnumerable
	{
		Future<A> Then<A> (Func<A> f);
		Future<A> Then<A> (Func<Future<A>> f);
		Future Then (Action f);
		
		bool completed {get;}
	}
	
	public abstract class FutureBase : Future
	{
		public abstract Future<A> Return<A> (A a);
		public abstract Future<A> Then<A> (Func<Future<A>> f);
		
		Future<A> Future.Then<A> (Func<A> f)
		{
			return Then<A> (() => Return (f()));
		}
		
		public abstract Future Then (Action f);
		
		public abstract bool completed {get;}
		
		public virtual IEnumerator GetEnumerator ()
		{
			while (! completed)
				yield return null;
		}
	}
	
	
	
	public class Completer : FutureBase {
		
		bool _completed = false;
		List<Action> actions = new List<Action>();
		
		#region implemented abstract members of FutureBase
		public override Future<A> Return<A> (A a)
		{
			return new Completer<A> (a);
		}
		public override Future<A> Then<A> (Func<Future<A>> f)
		{
			if (completed)
				return f ();
			
			var completer = new Completer<A> ();
			Action onComplete = () => {
				
				Future<A> future = f ();
				future.Then ((A a) => completer.Complete(a));
			};
			
			actions.Add (onComplete);
			return (Future<A>) completer;
		}
		public override Future Then (Action f)
		{
			var completer = new Completer ();
			Action onComplete = () => {
				f ();
				completer.Complete();
			};
			actions.Add (onComplete);
			return (Future) completer;
		}
		public override bool completed {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
		
		public void Complete ()
		{
			this._completed = true;
			actions.ForEach ((Action f) => f());
		}
	}
}
