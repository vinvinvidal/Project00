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
	List<Action<GameObject, GameObject, GameObject, SpecialClass>> GetSpecialAct(int c, int i);

	//超必殺技の処理を返すインターフェイス
	List<Action<GameObject, GameObject>> GetSuperAct(int c, int i);

	//特殊攻撃の対象を返すインターフェイス
	GameObject SearchSpecialTarget(int i);
}

public class SpecialArtsScript : GlobalClass, SpecialArtsScriptInterface
{
	//特殊攻撃制御フラグ
	bool SpecialAction000Flag = false;
	bool SpecialAction010Flag = false;
	bool SpecialAction020Flag = false;
	bool SpecialAction021Flag = false;
	bool SpecialAction022Flag = false;
	bool SpecialAction023Flag = false;

	//超必殺技制御フラグ
	bool SuperAction000Flag = false;
	bool SuperAction001Flag = false;
	bool SuperAction002Flag = false;

	//超必殺技エフェクト
	private GameObject SuperLightEffect;
	private GameObject SuperChargeEffect;
	private GameObject SuperFireWorkEffect;

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
					if(tempanim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || tempanim.GetCurrentAnimatorStateInfo(0).IsName("H_Attack"))
					{
						//アニメーションイベントを回す
						foreach(var ii in tempanim.GetCurrentAnimatorClipInfo(0)[0].clip.events)
						{
							//攻撃判定発生イベントを判別
							if(ii.functionName == "StartAttackCol" || ii.functionName == "StartH_AttackCol")
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
		//桃花用処理
		else if (i == 1)
		{
			//飛び道具との距離
			float WeaponDistance = 10000;

			//全ての飛び道具を回す
			foreach(GameObject ii in GameManagerScript.Instance.AllEnemyWeaponList)
			{
				//一番近い物を探す
				if(Vector3.SqrMagnitude(ii.transform.position - gameObject.transform.position) < WeaponDistance)
				{
					//距離をキャッシュ
					WeaponDistance = Vector3.SqrMagnitude(ii.transform.position - gameObject.transform.position);

					//投げきてきた敵を出力用変数に代入
					re = ii.GetComponent<EnemyWeaponColScript>().Enemy;
				}
			}
		}

		//出力
		return re;
	}

	//超必殺技の処理を返す
	public List<Action<GameObject, GameObject>> GetSuperAct(int c, int i)
	{
		List<Action<GameObject, GameObject>> re = new List<Action<GameObject, GameObject>>();

		//御命
		if (c == 0)
		{
			//怒傑
			if (i == 0)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//照明エフェクトインスタンス生成
						SuperLightEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "PartyLightEffect").ToArray()[0]);
						SuperLightEffect.transform.position = Player.transform.position;
						SuperLightEffect.transform.rotation = Quaternion.Euler(Vector3.zero);

						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SuperFlag = true;

						//ロック対象に対する回転を止める
						Player.GetComponent<PlayerScript>().RotateControl(1);

						//超必殺技制御フラグを立てる
						SuperAction000Flag = true;

						//敵移動コルーチン呼び出し
						StartCoroutine(EnemySuperAction000(Player, Enemy));

						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect01").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect.transform.parent = Enemy.transform;

						//位置を設定
						TempAttackEffect.transform.localPosition = new Vector3(0, 0.75f, 0.25f);

						//回転値を設定
						TempAttackEffect.transform.localRotation = Quaternion.Euler(new Vector3(180, 0, 0));
					}
				);

				//敵行動コルーチン
				IEnumerator EnemySuperAction000(GameObject Player, GameObject Enemy)
				{
					//移動目的地をキャッシュ
					Vector3 TargetPos = Player.transform.position + Player.transform.forward * 1.5f;

					//フラグが降りるまでループ
					while (SuperAction000Flag)
					{
						//目的地まで移動
						Enemy.GetComponent<EnemyCharacterScript>().SuperMoveVec = (TargetPos - Enemy.transform.position) * 5;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//超必殺技制御フラグを下す
						SuperAction000Flag = false;

						//超必殺技制御フラグを立てる
						SuperAction001Flag = true;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSuperlAction001(Player, Enemy));
					}
				);

				//プレイヤー行動コルーチン
				IEnumerator PlayerSuperlAction001(GameObject Player, GameObject Enemy)
				{
					//移動目的地をキャッシュ
					Vector3 TargetPos = Enemy.transform.position + (Enemy.transform.forward * 0.5f);

					//フラグが降りるまでループ
					while (SuperAction001Flag)
					{
						//敵を向く
						Player.transform.rotation = Quaternion.LookRotation(HorizontalVector(Enemy, Player));

						//目的地まで移動
						Player.GetComponent<PlayerScript>().SuperMoveVector = (TargetPos - Player.transform.position) * 10;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//スローモーション
						GameManagerScript.Instance.TimeScaleChange(0.5f, 0.05f);
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect0 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect00").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect0.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect0.transform.localPosition = new Vector3(0,0.8f,1);

						//回転値を設定
						TempAttackEffect0.transform.localRotation = Quaternion.Euler(Vector3.zero);
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect0 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect02").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect0.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect0.transform.localPosition = new Vector3(0,1.6f,0.75f);

						//回転値を設定
						TempAttackEffect0.transform.localRotation = Quaternion.Euler(new Vector3(-40,0,0));

						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect1 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "AttackEffect00").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect1.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect1.transform.localPosition = new Vector3(0, 0.2f, 0.3f);

						//回転値を設定
						TempAttackEffect1.transform.localRotation = Quaternion.Euler(new Vector3(-40, 0, 0));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect0 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect02").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect0.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect0.transform.localPosition = new Vector3(0, 1.6f, 0.75f);

						//回転値を設定
						TempAttackEffect0.transform.localRotation = Quaternion.Euler(new Vector3(-40, 0, 0));

						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect1 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "AttackEffect00").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect1.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect1.transform.localPosition = new Vector3(0, 0.2f, 0.3f);

						//回転値を設定
						TempAttackEffect1.transform.localRotation = Quaternion.Euler(new Vector3(-40, 0, 0));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect0 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect02").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect0.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect0.transform.localPosition = new Vector3(0, 1.6f, 0.75f);

						//回転値を設定
						TempAttackEffect0.transform.localRotation = Quaternion.Euler(new Vector3(-40, 0, 0));

						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect1 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "AttackEffect00").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect1.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect1.transform.localPosition = new Vector3(0, 0.2f, 0.3f);

						//回転値を設定
						TempAttackEffect1.transform.localRotation = Quaternion.Euler(new Vector3(-40, 0, 0));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//エフェクトのインスタンスを生成
						GameObject TempAttackEffect0 = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "AttackEffect03").ToArray()[0]);

						//キャラクターの子にする
						TempAttackEffect0.transform.parent = Player.transform;

						//位置を設定
						TempAttackEffect0.transform.localPosition *= 0;

						//回転値を設定
						TempAttackEffect0.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));

						//タメエフェクト取得
						SuperChargeEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(e => e.name == "ChargePower").ToArray()[0]);

						//タメエフェクトポジション設定して再生
						SuperChargeEffect.transform.position = Player.transform.position;
						DeepFind(SuperChargeEffect, "ChargePower2").GetComponent<ParticleSystem>().Play();
						DeepFind(SuperChargeEffect, "ChargePower3").GetComponent<ParticleSystem>().Play();

						//花火エフェクトインスタンス生成
						SuperFireWorkEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "SuperArtsFireWorkEffect").ToArray()[0]);
						SuperFireWorkEffect.transform.position = Player.transform.position - HorizontalVector(GameObject.Find("MainCamera"), Player) * 2.5f;
						SuperFireWorkEffect.transform.LookAt(Player.transform);

						//揺れ物をバタバタさせる
						Player.GetComponent<PlayerScript>().StartClothShake(3);
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//超必殺技制御フラグを下す
						SuperAction001Flag = false;

						//超必殺技制御フラグを立てる
						SuperAction002Flag = true;

						//タメループエフェクトを止める
						foreach (ParticleSystem ii in SuperChargeEffect.GetComponentsInChildren<ParticleSystem>())
						{
							//タメループエフェクトをアクティブにする、これをしないと以下の処理が走らずオブジェクトが消えない
							SuperChargeEffect.SetActive(true);

							//アクセサを取り出す
							ParticleSystem.MainModule tempMain = ii.main;

							//ループを止めてパーティクルの発生を停止
							tempMain.loop = false;
						}

						//敵移動コルーチン呼び出し
						StartCoroutine(EnemySuperAction002(Player, Enemy));
					}
				);

				//敵行動コルーチン
				IEnumerator EnemySuperAction002(GameObject Player, GameObject Enemy)
				{
					//移動目的地をキャッシュ
					Vector3 TargetPos = Player.transform.position + (-Player.transform.right * 1.5f);

					//フラグが降りるまでループ
					while (SuperAction002Flag)
					{
						//目的地まで移動
						Enemy.GetComponent<EnemyCharacterScript>().SuperMoveVec = (TargetPos - Enemy.transform.position);

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy) =>
					{
						//超必殺技制御フラグを下す
						SuperAction002Flag = false;

						//揺れ物をバタバタを止める
						Player.GetComponent<PlayerScript>().EndClothShake();

						//エフェクトインスタンス削除
						Destroy(SuperLightEffect);
						Destroy(SuperChargeEffect);

						//花火エフェクトはちゃんとパーティクルを止めて消す
						foreach(var ii in SuperFireWorkEffect.GetComponentsInChildren<ParticleSystem>())
						{
							//アクセサを取り出す
							ParticleSystem.MainModule ChildMain = ii.main;

							//ループを止めてパーティクルの発生を停止
							ChildMain.loop = false;
						}

						//アクセサを取り出す
						ParticleSystem.MainModule tempMain = SuperFireWorkEffect.GetComponent<ParticleSystem>().main;

						//ループを止めてパーティクルの発生を停止
						tempMain.loop = false;

						//プレイヤーのフラグを下ろす
						Player.GetComponent<PlayerScript>().SuperFlag = false;
					}
				);
			}
		}

		return re;
	}

	//特殊攻撃の処理を返す
	public List<Action<GameObject, GameObject, GameObject, SpecialClass>> GetSpecialAct(int c, int i)
	{
		List<Action<GameObject, GameObject, GameObject, SpecialClass>> re = new List<Action<GameObject, GameObject, GameObject, SpecialClass>>();

		//御命
		if(c == 0)
		{
			//鍔掃
			if(i==0)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy , GameObject Weapon, SpecialClass Arts) =>
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
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
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
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
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
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
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

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Enemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0.5f, 15, 0.1f) } , new List<int>() { Arts.Damage }, new List<int>() { Arts.DamageIndex }), 0));
					}
				);
			}
			//眩箔
			else if (i == 2)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//特殊攻撃制御フラグを立てる
						SpecialAction020Flag = true;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction020(Player, Enemy));
					}
				);

				//プレイヤー行動コルーチン
				IEnumerator PlayerSpecialAction020(GameObject Player, GameObject Enemy)
				{
					//フラグが降りるまでループ
					while (SpecialAction020Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -Player.transform.forward * 7.5f;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//特殊行動制御フラグを下す
						SpecialAction020Flag = false;

						//特殊攻撃制御フラグを立てる
						SpecialAction021Flag = true;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction021(Player, Enemy));
					}
				);

				//プレイヤー行動コルーチン
				IEnumerator PlayerSpecialAction021(GameObject Player, GameObject Enemy)
				{
					//フラグが降りるまでループ
					while (SpecialAction021Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -Player.transform.forward * 2f;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//特殊攻撃制御フラグを下す
						SpecialAction021Flag = false;

						//敵の興奮値を上げる
						Enemy.GetComponent<EnemyCharacterScript>().Excite += 0.2f;

						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//特殊攻撃制御フラグを立てる
						SpecialAction022Flag = true;

						//特殊攻撃制御フラグを下す
						SpecialAction023Flag = false;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction022(Player, Enemy));
					}
				);

				//プレイヤー行動コルーチン
				IEnumerator PlayerSpecialAction022(GameObject Player, GameObject Enemy)
				{
					//フラグが降りるまでループ
					while (SpecialAction022Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -Player.transform.forward * 0.5f;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//特殊攻撃制御フラグを立てる
						SpecialAction023Flag = true;

						//特殊攻撃制御フラグを下す
						SpecialAction022Flag = false;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction023(Player, Enemy));
					}
				);

				//プレイヤー行動コルーチン
				IEnumerator PlayerSpecialAction023(GameObject Player, GameObject Enemy)
				{
					//フラグが降りるまでループ
					while (SpecialAction023Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -Player.transform.forward * 1.5f;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//特殊攻撃制御フラグを下す
						SpecialAction022Flag = false;

						//特殊攻撃制御フラグを下す
						SpecialAction023Flag = false;

						//プレイヤーのフラグを下す
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;

						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;
					}
				);
			}
		}

		//桃花
		if (c == 1)
		{
			//賎祓い
			if (i == 0)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//飛び道具をくるくる
						Weapon.GetComponent<Rigidbody>().AddTorque(Player.transform.up * 100, ForceMode.Impulse);
						
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//飛び道具を敵に飛ばす
						Weapon.GetComponent<Rigidbody>().AddForce(((Enemy.transform.position + Vector3.up) - Weapon.transform.position) * 5, ForceMode.Impulse);
					}
				);
			}
		}

		//出力
		return re;
	}
}
