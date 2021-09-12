using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysicScript : GlobalClass
{
	//リジッドボディ
	Rigidbody Rig;

	//自身を持っているキャラクター
	GameObject Character;

    void Start()
    {
		//リジッドボディ取得
		Rig = transform.gameObject.GetComponent<Rigidbody>();

		//自身を持っているキャラクター取得
		Character = transform.root.gameObject;
	}

    void FixedUpdate()
    {
		//常にキャラクターに向かって力をかけ続けて追従させる
		Rig.velocity = (Character.transform.position - transform.position) * 10;
    }
}
