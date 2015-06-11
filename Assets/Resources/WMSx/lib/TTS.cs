using UnityEngine;
using System.Collections;
using Async;
using System;
/*
 * PhaaxTTS 1.7 for Unity
 * Samuel Johansson 2014
 * samuel@phaaxgames.com
 * 
 * Free to use as long as you give some credit in your application/game.
 * 
 * Works on mobile devices, uses Google TTS.
 * There's a limit of 100 characters (by Google).
 * This obviously uses network traffic.
 * 
 * Usage:
 * Drop it into your assets folder somewhere.
 * Don't attach it to any object.
 *
 * You need to call PhaaxTTS.Say like this: 
 * PhaaxTTS.Say("en_gb", "Text-to-speech is silly. I wear a hat, sometimes. Sometimes!", 1.0f, 1.0f);
 * 
 * First parameter is your language code (en_us = US english, en_gb = british english, sv = swedish)
 * Second parameter is what you want it to say.
 * Third parameter is pitch, which is a float value.
 * Fourth parameter is volume, which is a float value (0.0f-1.0f).
 * 
 * Use PhaaxTTS.IsPlaying() to see if anything is playing at the moment (returns true or false).
 * Use PhaaxTTS.IsLoading() to see if anything is loading at the moment (returns true or false).
 * 
 */

public class TTS : MonoBehaviour, ITTS {

	public static TTS instance;
	
	public AudioSource source;
	
	bool loading = false;
	
	public void Awake ()
	{
		instance = this;
	}
	
	public IEnumerable Speak (string words, string lang = "es", float pitch = 1f, float volume = 1f)
	{
		WWW www = null;

		Action<String> f = (String s) => {};

		return Seq.WaitWhile(() => loading)
		.Then(() => {
			loading = true;
			var query = WWW.EscapeURL (words);
			var url = String.Format(@"http://translate.google.com/translate_tts?ie=UTF-8&tl={0}&q={1}", lang, query);
			print (url);
			www = new WWW (url);
		})
		.Then (Seq.WaitWhile (() => ! www.isDone))
		.Then (() => {
			print (String.Format("Saying: {0}", words));
			instance.source.clip = www.GetAudioClip(false, true, AudioType.MPEG);
			instance.source.Play();
			return Seq.WaitForSeconds (0.1f);
		})
		.Then (() => {
			loading = false;
		});
	}
	
	public void Say (string words, string lang = "es", float pitch = 1f, float volume = 1f)
	{
		Speak (words, lang, pitch, volume).Start (this);
	}
}

public interface ITTS
{
	IEnumerable Speak (string words, string lang = "es", float pitch = 1f, float volume = 1f);
	void Say (string words, string lang = "es", float pitch = 1f, float volume = 1f);
}