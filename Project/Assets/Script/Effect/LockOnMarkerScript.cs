using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class LockOnMarkerScript : GlobalClass
{
	private void Start()
	{
		ConstraintSource Source = new ConstraintSource();

		Source.sourceTransform = GameObject.Find("MainCamera").transform;

		Source.weight = 1;

		gameObject.GetComponent<LookAtConstraint>().SetSource(0, Source);
	}
}
