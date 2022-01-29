using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeScript : GlobalClass
{
    void Start()
    {
		gameObject.transform.localRotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);

		gameObject.transform.localScale = new Vector3(Random.Range(0.75f,1), Random.Range(1, 1.5f), Random.Range(0.75f, 1));
	}
}
