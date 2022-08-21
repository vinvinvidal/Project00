using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyIKScript : GlobalClass
{
	//リグビルダー
	private RigBuilder R_Builder;

    void Start()
    {
		//リグビルダー取得
		R_Builder = GetComponent<RigBuilder>();

	}

	// Update is called once per frame
	private void LateUpdate()
	{

	}
}
