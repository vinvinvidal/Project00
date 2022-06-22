using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombEffectScript : GlobalClass
{

	Renderer BombRenderer;

	Material BombMaterial;

	float VertNum;

	void Start()
    {
		BombRenderer = GetComponent<Renderer>();

		BombMaterial = BombRenderer.material;

		StartCoroutine(BombCoroutine());
	}

	private IEnumerator BombCoroutine()
	{
		VertNum = 2f;

		while (VertNum > 0)
		{
			BombMaterial.SetVector("OBJPos", BombRenderer.bounds.center);			

			BombMaterial.SetFloat("VertNum", VertNum);

			VertNum -= 8f * Time.deltaTime;

			yield return null;
		}

		BombMaterial.SetFloat("VertNum", 0);
	}
}
