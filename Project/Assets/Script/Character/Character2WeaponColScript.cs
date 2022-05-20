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

	//ワイヤーか爆弾か
	public int WeaponIndex;

	// Start is called before the first frame update
	void Start()
	{
		//コライダ取得
		WeaponCol = GetComponent<SphereCollider>();

		//キャラクターオブジェクト取得
		CharacterOBJ = gameObject.transform.root.gameObject;
	}

	//攻撃コライダーが当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//コライダを無効化
		WeaponCol.enabled = false;

		//ワイヤー
		if(WeaponIndex == 0)
		{
			//コライダが敵に当たった
			if (Hit.gameObject.layer == LayerMask.NameToLayer("EnemyDamageCol"))
			{
				//架空の技を作成
				ArtsClass TempArts = MakeInstantArts(new List<Color>() { new Color(0, 0, 0, 0) }, new List<float>() { 0 }, new List<int>() { 41 }, new List<int>() { 0 }, new List<int>() { 8 });

				//攻撃判定bool
				bool TempBool = false;

				//攻撃が有効か判定
				ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => TempBool = reciever.AttackEnable(TempArts, 0));

				//有効なら処理実行
				if (TempBool)
				{
					//当たった敵をロック対象にする
					CharacterOBJ.GetComponent<Character2WeaponMoveScript>().LockEnemy = Hit.gameObject.transform.root.gameObject;

					//敵当たりフラグを立てる
					CharacterOBJ.GetComponent<Character2WeaponMoveScript>().EnemyHitFlag = true;

					//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
					ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(TempArts, 0));
				}
				//無効なら壁に当たった事にして巻き戻す
				else
				{
					//壁当たりフラグを立てる
					CharacterOBJ.GetComponent<Character2WeaponMoveScript>().WallHitFlag = true;
				}

			}
			//コライダが壁に当たった
			else if (Hit.gameObject.layer == LayerMask.NameToLayer("TransparentFX"))
			{
				//壁当たりフラグを立てる
				CharacterOBJ.GetComponent<Character2WeaponMoveScript>().WallHitFlag = true;
			}
		}
		//爆弾
		else if (WeaponIndex == 1)
		{
			//コライダが敵に当たった
			if (Hit.gameObject.layer == LayerMask.NameToLayer("EnemyDamageCol"))
			{
				//敵当たりフラグを立てる
				CharacterOBJ.GetComponent<Character2WeaponMoveScript>().EnemyHitFlag = true;

				//架空の技を作成
				ArtsClass TempArts = MakeInstantArts(new List<Color>() { new Color(0, 2, 0, 0.1f) }, new List<float>() { 0 }, new List<int>() { 11 }, new List<int>() { 1 }, new List<int>() { 0 });

				//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
				ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(TempArts, 0));
			}
		}
	}

	//コライダのアクティブを切り替える
	public void SwitchCol(bool b)
	{
		WeaponCol.enabled = b;
	}
}
