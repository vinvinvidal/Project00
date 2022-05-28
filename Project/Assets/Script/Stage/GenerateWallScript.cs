using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWallScript : GlobalClass
{
	private Rigidbody R_body;

	private void Start()
	{
		R_body = GetComponent<Rigidbody>();

		StartCoroutine(GenerateWallCoroutine());
	}

	private IEnumerator GenerateWallCoroutine()
	{
		yield return new WaitForSeconds(0.1f);

		R_body.AddForce(Vector3.down * 10, ForceMode.Impulse);

		while(R_body.velocity.sqrMagnitude > 0.05f)
		{
			yield return null;			
		}

		R_body.isKinematic = true;
	}
}
