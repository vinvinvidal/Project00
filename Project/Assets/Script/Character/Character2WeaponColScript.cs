using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface Character2WeaponColInterface : IEventSystemHandler
{
	//コライダのアクティブを切り替える
	void SwitchCol(bool b);
}

public class Character2WeaponColScript : GlobalClass, Character2WeaponColInterface
{
	//コライダ
	private SphereCollider WeaponCol;

	//キャラクターオブジェクト
	private GameObject CharacterOBJ;
	
	// Start is called before the first frame update
	void Start()
	{
		//コライダ取得
		WeaponCol = GetComponent<SphereCollider>();

		//キャラクターオブジェクト取得
		CharacterOBJ = gameObject.transform.root.gameObject;
	}

	// Update is called once per frame
	void Update()
	{

	}

	//攻撃コライダーが敵に当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//コライダを無効化
		WeaponCol.enabled = false;

		//プレイヤー側の処理を呼び出す
		ExecuteEvents.Execute<PlayerScriptInterface>(CharacterOBJ, null, (reciever, eventData) => reciever.HitCharacter2SpecialAttack(Hit.gameObject.transform.root.gameObject));
	}

	//コライダのアクティブを切り替える
	public void SwitchCol(bool b)
	{
		WeaponCol.enabled = b;
	}

}
