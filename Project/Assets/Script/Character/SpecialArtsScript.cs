using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface SpecialArtsScriptInterface : IEventSystemHandler
{
	//特殊攻撃の処理を返すインターフェイス
	List<Action<GameObject, GameObject, SpecialClass>> GetSpecialAct(int c, int i);
}

public class SpecialArtsScript : GlobalClass, SpecialArtsScriptInterface
{
	//特殊攻撃制御フラグ
	bool SpecialAction000Flag = false;

	//特殊攻撃の処理を返す
	public List<Action<GameObject, GameObject, SpecialClass>> GetSpecialAct(int c, int i)
	{
		List<Action<GameObject, GameObject, SpecialClass>> re = new List<Action<GameObject, GameObject, SpecialClass>>();

		//御命
		if(c == 0)
		{
			//鍔掃
			if(i==0)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy , SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//敵のフラグを立てる
						Enemy.GetComponent<EnemyCharacterScript>().SpecialFlag = true;

						//敵のアニメーター遷移フラグを立てる
						Enemy.GetComponent<Animator>().SetBool("Special", true);

						//使用するモーションに差し替え
						Enemy.GetComponent<EnemyCharacterScript>().OverRideAnimator["Special_void"] = Enemy.GetComponent<EnemyCharacterScript>().DamageAnimList[Arts.DamageIndex];

						//アニメーターを上書きしてアニメーションクリップを切り替える
						Enemy.GetComponent<Animator>().runtimeAnimatorController = Enemy.GetComponent<EnemyCharacterScript>().OverRideAnimator;

						//特殊攻撃制御フラグを立てる
						SpecialAction000Flag = true;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction000(Player,Enemy));

						//敵移動コルーチン呼び出し
						StartCoroutine(EnemySpecialAction000(Player, Enemy));
					}
				);

				//プレイヤー移動コルーチン
				IEnumerator PlayerSpecialAction000(GameObject Player, GameObject Enemy)
				{
					//移動目的地をキャッシュ
					Vector3 TargetPos = Enemy.transform.position - (Enemy.transform.right * 0.5f);

					//フラグが降りるまでループ
					while (SpecialAction000Flag)
					{
						//敵を向く
						Player.transform.rotation = Quaternion.LookRotation(HorizontalVector(Enemy, Player));

						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = (TargetPos - Player.transform.position) * 10;
			
						//1フレーム待機
						yield return null;
					}
				}

				//敵移動コルーチン
				IEnumerator EnemySpecialAction000(GameObject Player, GameObject Enemy)
				{
					//移動目的地をキャッシュ
					Vector3 TargetPos = Player.transform.position - Player.transform.forward * 1.25f;

					//移動開始時間をキャッシュ
					float temptime = Time.time;

					//時間が過ぎるまでループ、途中で敵が死んだりしたら抜ける
					while (temptime + 1.5f > Time.time && Enemy != null)
					{
						//目的地まで移動
						Enemy.GetComponent<EnemyCharacterScript>().SpecialMoveVec = (TargetPos - Enemy.transform.position) * 5;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, SpecialClass Arts) =>
					{
						//特殊攻撃制御フラグを下す
						SpecialAction000Flag = false;

						//プレイヤーのフラグを下す
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;

						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;

						if(Enemy != null)
						{
							//敵のフラグを下す
							Enemy.GetComponent<EnemyCharacterScript>().SpecialFlag = false;

							//敵のアニメーター遷移フラグを下す
							Enemy.GetComponent<Animator>().SetBool("Special", false);

							//敵の移動ベクトル初期化
							Enemy.GetComponent<EnemyCharacterScript>().SpecialMoveVec *= 0;
						}
					}
				);
			}
		}

		return re;
	}
}
