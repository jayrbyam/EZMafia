using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Confirmation : MonoBehaviour {

	public GameObject panel;
	public Text message;
	public System.Action onConfirm;

	public void Show() {
		gameObject.SetActive(true);
		GetComponent<Fader> ().Fade (2f, 0.9f);
		panel.GetComponent<MoveTo> ().MoveToPos (Vector3.zero, 2f);
	}

	public void Hide() {
		StartCoroutine(HideCR());
	}
	
	public void Confirm() {
		onConfirm();
		Hide();
	}

	private IEnumerator HideCR() {
		GetComponent<Fader> ().Fade (2f, 0f);
		panel.GetComponent<MoveTo> ().GoHome (2f);
		while (panel.GetComponent<MoveTo> ().bIsAnimating) yield return null;
		gameObject.SetActive (false);
	}
}
