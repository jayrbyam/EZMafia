using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour {

	public void Fade(float speed, float target) {
		if (GetComponent<RawImage>().color.a == 1f)
			return;

		StopAllCoroutines();
		StartCoroutine(FadeCR(speed, target));
	}

	private IEnumerator FadeCR(float speed, float target)
	{
		Vector4 current = GetComponent<RawImage> ().color;
		float fPhase = 0f;

		while (fPhase < 1f)
		{
			fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed);
			float easedF = iTween.easeOutQuad(0f, 1f, fPhase);
			GetComponent<RawImage> ().color = Vector4.Lerp (current, new Vector4 (current.x, current.y, current.z, target), easedF);
			yield return null;
		}
	}
}
