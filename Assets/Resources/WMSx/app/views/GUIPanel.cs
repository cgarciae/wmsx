using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIPanel : MonoBehaviour {

	public static GUIPanel instance;

	public Text text;
	public Image image;
	public Text status;
	public Text quantity;
	public RectTransform imageBackground;
	public Image blackBackground;
	public Image bigImage;

	public Sprite sprite {
		get {return image.sprite;}
		set {image.sprite = bigImage.sprite = value;}
	}

	bool _showImage = true;
	public bool showImage {
		get {return _showImage;}
		set {
			_showImage = value;

			blackBackground.gameObject.SetActive (false);
			imageBackground.gameObject.SetActive (value);
		}
	}


	public void Click () {
		if (!showImage)
			return;

		var cond = imageBackground.gameObject.activeSelf && sprite != null;

		imageBackground.gameObject.SetActive (!cond);
		blackBackground.gameObject.SetActive (cond);
	}

	void Awake () {
		instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
