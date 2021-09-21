using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface SpecialArtsScriptInterface : IEventSystemHandler
{
	//特殊攻撃の処理を返すインターフェイス
	List<Action<GameObject, GameObject, SpecialClass>> GetSpecialAct(int c, int i);

	//特殊攻撃の対象を返すインターフェイス
	GameObject SearchSpecialTarget(int i);
}

public class SpecialArtsScript : GlobalClass, SpecialArtsScriptInterface
{
	//特殊攻撃制御フラグ
	bool SpecialAction000Flag = false;
	bool SpecialAction010Flag = false;

	//特殊攻撃の対象を返すインターフェイス
	public GameObject SearchSpecialTarget(int i)
	{
		//出力用変数宣言
		GameObject re = null;

		//攻撃判定出現までの残り時間
		float AttckTime = 100000;

		//御命用処理
		if(i == 0)
		{
			//全ての敵を回す
			foreach(GameObject e in GameManagerScript.Instance.AllActiveEnemyList)
			{
				//nullチェック
				if(e != null)
				{ 
					//アニメーター取得
					Animator tempanim = e.GetComponent<Animator>();

					//攻撃してきているか判別
					if(tempanim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
					{
						//アニメーションイベントを回す
						foreach(var ii in tempanim.GetCurrentAnimatorClipInfo(0)[0].clip.events)
						{
							//攻撃判定発生イベントを判別
							if(ii.functionName == "StartAttackCol")
							{
								//攻撃判定発生までの時間を計測
								float temptime = ii.time - (tempanim.GetCurrentAnimatorStateInfo(0).length * tempanim.GetCurrentAnimatorStateInfo(0).normalizedTime);

								//一番早い奴をキャッシュ
								if(AttckTime > temptime && temptime > 0)
								{
									//出力用変数に代入
									re = e;

									//攻撃発生時間をキャッシュ
									AttckTime = temptime;
								}
							}
						}					
					}
				}
			}
		}

		//出力
		return re;
	}

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

				//プレイヤー行動コルーチン
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

				//敵行動コルーチン
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
			//可穿
			else if (i == 1)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy , SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//特殊攻撃制御フラグを立てる
						SpecialAction010Flag = true;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction010(Player,Enemy));
					}
				);

				//プレイヤー行動コルーチン
				IEnumerator PlayerSpecialAction010(GameObject Player, GameObject Enemy)
				{
					//フラグが降りるまでループ
					while (SpecialAction010Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = Player.transform.forward * 7.5f;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy , SpecialClass Arts) =>
					{
						//特殊行動制御フラグを下す
						SpecialAction010Flag = false;

						//プレイヤーのフラグを下す
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;

						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;

						//ヒットエフェクトインスタンス生成
						GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect01").ToList()[0]);

						//プレイヤーの子にする
						HitEffect.transform.parent = Player.transform;

						//PRS設定
						HitEffect.transform.localPosition = new Vector3(0,0.75f,0.5f);
						HitEffect.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));

						//代入用List
						List<Color> KBV = new List<Color>();
						List<int> ATI = new List<int>();
						List<int> DML = new List<int>();
						List<int> CML = new List<int>();

						//代入用Listにノックバックベクトルを入れる
						KBV.Add(new Color(0, 0.5f, 15, 0.1f));

						//代入用Listにダメージインデックスを入れる
						ATI.Add(Arts.DamageIndex);

						//代入用Listにダメージを入れる
						DML.Add(Arts.Damage);

						//代入用Listにチャージダメージを入れる
						CML.Add(0);

						//架空の技Classを作る
						ArtsClass temparts = new ArtsClass
						(
							"",
							"",
							0,
							"",
							new List<Color>(),
							DML,
							CML,
							new List<Color>(),
							KBV,
							"",
							new List<int>(),
							new List<int>(),
							ATI,
							new List<int>(),
							CML,
							new List<int>(),
							false,
							new List<string>(),
							new List<Vector3>(),
							new List<Vector3>(),
							new List<float>(),
							CML,
							new List<Vector3>()
						);

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Enemy, null, (reciever, eventData) => reciever.PlayerAttackHit(temparts, 0));
					}
				);
			}
		}

		return re;
	}
}
