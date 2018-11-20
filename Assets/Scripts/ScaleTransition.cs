using UnityEngine;
using System.Collections;

public class ScaleTransition : MonoBehaviour
{
    Transform localTrans;
    public bool bIsAnimating { get; set; }

    public bool bRubber = false;

    void Awake()
    {
        bIsAnimating = false;
    }

    public void ScaleImmediate(Vector3 scale)
    {
        StopAllCoroutines();
        transform.localScale = scale;
        bIsAnimating = false;
    }

    public void ScaleToZero(float speed)
    {
        if (transform.localScale == Vector3.zero)
            return;

        StopAllCoroutines();
        bIsAnimating = true;
        StartCoroutine(ScaleToZeroCR(speed));
    }

    public void ScaleToOne(float speed)
    {
        if (transform.localScale == Vector3.one)
            return;

        StopAllCoroutines();
        bIsAnimating = true;
        StartCoroutine(ScaleToOneCR(speed));
    }

	public void ScaleTo(float target, float speed) {
		if (transform.localScale == new Vector3(target, target, target))
			return;

		StopAllCoroutines ();
		bIsAnimating = true;
		StartCoroutine (ScaleToCR(target, speed));
	}

    IEnumerator ScaleToZeroCR(float speed)
    {
        Vector3 current = transform.localScale;
        float fPhase = 0f;
        while(fPhase < 1f)
        {
            fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed);
            float easedF = iTween.easeInQuad(0f, 1f, fPhase);
            transform.localScale = Vector3.Lerp(current, Vector3.zero, easedF);
            yield return null;
        }

        bIsAnimating = false;
    }

    IEnumerator ScaleToOneCR(float speed)
    {
        Vector3 current = transform.localScale;
        float fPhase = 0f;
        if(bRubber)
        {
            Vector3 rubber = new Vector3(1.2f, 1.2f, 1.2f);
            while (fPhase < 1f)
            {
                fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed);
                float easedF = iTween.easeOutQuad(0f, 1f, fPhase);
                transform.localScale = Vector3.Lerp(current, rubber, easedF);
                yield return null;
            }

            current = transform.localScale;
            fPhase = 0f;
            while (fPhase < 1f)
            {
                fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed * 2);
                float easedF = iTween.easeOutQuad(0f, 1f, fPhase);
                transform.localScale = Vector3.Lerp(current, Vector3.one, easedF);
                yield return null;
            }

            bIsAnimating = false;
        }
        else
        {
            while (fPhase < 1f)
            {
                fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed);
                float easedF = iTween.easeOutQuad(0f, 1f, fPhase);
                transform.localScale = Vector3.Lerp(current, Vector3.one, easedF);
                yield return null;
            }
        }

        bIsAnimating = false;
    }

	IEnumerator ScaleToCR(float target, float speed)
	{
		Vector3 current = transform.localScale;
		float fPhase = 0f;
		if(bRubber)
		{
			Vector3 rubber = new Vector3(1.2f * target, 1.2f * target, 1.2f * target);
			while (fPhase < 1f)
			{
				fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed);
				float easedF = iTween.easeOutQuad(0f, 1f, fPhase);
				transform.localScale = Vector3.Lerp(current, rubber, easedF);
				yield return null;
			}

			current = transform.localScale;
			fPhase = 0f;
			while (fPhase < 1f)
			{
				fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed * 2);
				float easedF = iTween.easeOutQuad(0f, 1f, fPhase);
				transform.localScale = Vector3.Lerp(current, new Vector3(target, target, target), easedF);
				yield return null;
			}

			bIsAnimating = false;
		}
		else
		{
			while (fPhase < 1f)
			{
				fPhase = Mathf.Clamp01(fPhase + Time.deltaTime * speed);
				float easedF = iTween.easeOutQuad(0f, 1f, fPhase);
				transform.localScale = Vector3.Lerp(current, new Vector3(target, target, target), easedF);
				yield return null;
			}
		}

		bIsAnimating = false;
	}
}