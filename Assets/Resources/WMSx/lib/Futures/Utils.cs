using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Async
{
	public static class Utils
	{
		public static Func<A,A> ToFunc<A> (this Action<A> f)
		{
			return (A a) => {
				f (a);
				return a;
			};
		}
		
		public static Action<A> ToAction<A,B> (this Func<A,B> f)
		{
			return (A a) => {
				f (a);
			};
		}
		
		public static Func<A,C> Compose<A,B,C> (Func<B,C> f, Func<A,B> g)
		{
			return (A a) => f (g (a));
		}
		
		public static A Id<A> (A a) {
			return a;
		}
		
		public static Future<A> LoadGameObject<A> (String path, MonoBehaviour m) where A : MonoBehaviour
		{
			var future = new Completer<A> ();
			var request = Resources.LoadAsync (path);
			
			Seq.WaitWhile (()=> ! request.isDone).Then (()=> {
				var go = GameObject.Instantiate (request.asset) as GameObject;
				future.Complete (go.GetComponent<A>());
			})
			.Start(m);
			
			return future;
		}
		
		public static void ResetCoordinates (this Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localScale = Vector3.one;
			t.localRotation = Quaternion.identity;
		}
		
		public static RectTransform RectTransform (this Transform t)
		{
			return (RectTransform)t;
		}
	}
}
