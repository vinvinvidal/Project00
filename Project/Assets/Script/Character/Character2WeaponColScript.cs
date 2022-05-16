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

	//攻撃コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//コライダを無効化
		WeaponCol.enabled = false;

		//コライダが敵に当たった
		if (Hit.gameObject.layer == LayerMask.NameToLayer("EnemyDamageCol"))
		{
			//敵当たりフラグを立てる
			CharacterOBJ.GetComponent<SpecialArtsScript>().WeaponCollEnemyFlag = true;

			//特殊攻撃成功処理呼び出し
			CharacterOBJ.GetComponent<PlayerScript>().SpecialAttackHit(Hit.gameObject.transform.root.gameObject);
			CharacterOBJ.GetComponent<SpecialArtsScript>().Character2SpecialAttackHit(Hit.gameObject.transform.root.gameObject);
		}
		//コライダが壁に当たった
		else if (Hit.gameObject.layer == LayerMask.NameToLayer("TransparentFX"))
		{
			//壁当たりフラグを立てる
			CharacterOBJ.GetComponent<SpecialArtsScript>().WeaponCollWallFlag = true;
		}
	}

	//コライダのアクティブを切り替える
	public void SwitchCol(bool b)
	{
		WeaponCol.enabled = b;
	}

}
