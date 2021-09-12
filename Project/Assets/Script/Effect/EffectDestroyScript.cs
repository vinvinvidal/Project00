using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDestroyScript : GlobalClass
{
	public float DestTime;

	bool AllStop = false;

    void Update()
    {
		AllStop = true;

		foreach (ParticleSystem i in GetComponentsInChildren<ParticleSystem>())
		{
			if(i.isPlaying || i.isPaused)
			{
				AllStop = false;

				break;
			}
		}

		if(AllStop)
		{
			StartCoroutine(DestroyEffect());
		}
	}

	IEnumerator DestroyEffect()
	{
		yield return new WaitForSeconds(DestTime);

		Destroy(transform.gameObject);
	}	
}
