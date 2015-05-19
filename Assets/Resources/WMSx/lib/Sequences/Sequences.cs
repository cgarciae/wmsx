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
		
		public static IEnumerable Then<A> (this IEnumerable e, Func<A> f)
		{
			var ator = e.GetEnumerator();
			
			while (ator.MoveNext())
			{
				yield return ator.Current;
			}
			yield return f ();
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

		public static Coroutine Start (this IEnumerator e, MonoBehaviour m)
		{
			return m.StartCoroutine (e);
		}
		
		public static IEnumerable Cycle (IEnumerable e)
		{
			while (true)
				foreach (var _ in e)
					yield return _;
		}
	}
}

