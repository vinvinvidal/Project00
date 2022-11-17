using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

//他のスクリプトから関数を呼ぶ為のインターフェイス
public interface SpecialArtsScriptInterface : IEventSystemHandler
{
	//特殊攻撃の処理を返すインターフェイス
	List<Action<GameObject, GameObject, GameObject, SpecialClass>> GetSpecialAct(int c, int i);

	//超必殺技の処理を返すインターフェイス
	List<Action<GameObject, GameObject>> GetSuperAct(int c, int i);

	//特殊攻撃の対象を返すインターフェイス
	GameObject SearchSpecialTarget(int i);

	//特殊攻撃中に攻撃を喰らった時に呼び出すインターフェイス
	void SpecialAttackMiss(int i, GameObject Weapon);

	//ストックしている武器を落とすインターフェイス
	void DropStockWeapon();
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
	bool SpecialAction0100Flag = false;

	bool SpecialAction110Flag = false;

	bool SpecialAction210Flag = false;

	//超必殺技制御フラグ
	bool SuperAction000Flag = false;
	bool SuperAction001Flag = false;
	bool SuperAction002Flag = false;

	//超必殺技エフェクト
	private GameObject SuperLightEffect;
	private GameObject SuperChargeEffect;
	private GameObject SuperFireWorkEffect;

	//桃花の武器に付けている敵飛び道具
	public GameObject StockWeapon = null;

	//桃花の周囲にいる敵オブジェクトList
	public List<GameObject> AroundEnemyList = new List<GameObject>();

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
					re = ii.GetComponent<ThrowWeaponScript>().Enemy;
				}
			}
		}
		//泉用処理
		else if (i == 2)
		{
			//現在ロックしている敵を取得
			ExecuteEvents.Execute<PlayerScriptInterface>(gameObject, null, (reciever, eventData) => re = reciever.GetLockEnemy());

			//ロックしていなかったら攻撃と同じように対象を探す
			if (re == null)
			{
				gameObject.GetComponent<PlayerScript>().OnLockOn(new UnityEngine.InputSystem.InputValue());
			}

			//武器スクリプトにもロック対象を渡す
			gameObject.GetComponent<Character2WeaponMoveScript>().LockEnemy = re;
		}

		//出力
		return re;
	}

	//特殊攻撃の途中で喰らった時の処理
	public void SpecialAttackMiss(int i, GameObject Weapon)
	{
		//御命
		if (i == 0)
		{

		}
		//桃花
		else if (i == 1)
		{
			if(Weapon != null)
			{
				//特殊攻撃失敗処理呼び出し
				ExecuteEvents.Execute<ThrowWeaponScript>(Weapon, null, (reciever, eventData) => reciever.BrokenWeapon());
			}
		}
		//泉
		else if (i == 2)
		{
			//特殊攻撃失敗処理呼び出し
			ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.SpecialAttackMiss());
		}
	}

	//ストックしている武器を落とすインターフェイス
	public void DropStockWeapon()
	{
		if(StockWeapon != null)
		{
			StockWeapon.GetComponent<ThrowWeaponScript>().BrokenWeapon();

			StockWeapon = null;
		}
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
						GameManagerScript.Instance.TimeScaleChange(0.5f, 0.05f, () => { });
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
						SuperFireWorkEffect.transform.position = Player.transform.position - HorizontalVector(GameManagerScript.Instance.GetMainCameraOBJ(), Player) * 2.5f;
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

						//エフェクト削除スクリプトを追加、これをしないと削除したあとにOnDestroyが呼ばれずメモリリークする
						SuperLightEffect.AddComponent<EffectDestroyScript>();

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
			//冪穿
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
						ExecuteEvents.Execute<EnemyCharacterInterface>(Enemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0.5f, 15, 0.1f) }, new List<float>() { Arts.Damage }, new List<int>() { 1 }, new List<int>() { Arts.DamageIndex }, new List<int>() { 0 }, new List<int>() { 0 }), 0));
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
			//ガード
			else if (i == 10)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//特殊攻撃制御フラグを立てる
						SpecialAction0100Flag = true;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction0100(Player, Enemy));
					}
				);

				//プレイヤー行動コルーチン
				IEnumerator PlayerSpecialAction0100(GameObject Player, GameObject Enemy)
				{
					//フラグが降りるまでループ
					while (SpecialAction0100Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -Player.transform.forward * 15f;

						//1フレーム待機
						yield return null;
					}
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//特殊攻撃制御フラグを下す
						SpecialAction0100Flag = false;

						//プレイヤーのフラグを下す
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;

						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;
					}
				);
			}
		}

		//桃花
		else if(c == 1)
		{
			//伍経反し
			if (i == 0)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//飛び道具の関数を呼び出して無害化
						Weapon.GetComponent<ThrowWeaponScript>().PhysicOBJ();

						//飛び道具のRigidBody無効化
						Weapon.GetComponent<Rigidbody>().isKinematic = true;

						//飛び道具を剣先にアタッチ
						foreach (WeaponSettingScript ii in Player.GetComponentsInChildren<WeaponSettingScript>())
						{
							if (ii.name.Contains("_0"))
							{
								Weapon.transform.parent = ii.transform;

								Weapon.transform.localRotation = Quaternion.Euler(new Vector3(180, 0, 180));

								Weapon.transform.localPosition = Weapon.GetComponent<ThrowWeaponScript>().StockPosition;
							}
						}

						//ストックしている武器があれば投げる
						if (StockWeapon != null)
						{
							StartCoroutine(PlayerSpecialAction110(Player, Enemy));
						}
					}
				);

				//ストック武器投げコルーチン
				IEnumerator PlayerSpecialAction110(GameObject Player, GameObject Enemy)
				{
					//チョイ待機
					yield return new WaitForSeconds(0.05f);

					//親を解除
					StockWeapon.transform.parent = null;

					//飛び道具のストックフラグを下ろす
					//StockWeapon.GetComponent<ThrowWeaponScript>().StockFlag = false;

					//飛び道具のコライダー有効化
					StockWeapon.GetComponent<MeshCollider>().enabled = true;

					//飛び道具のRigidBody有効化
					StockWeapon.GetComponent<Rigidbody>().isKinematic = false;

					//飛び道具の関数を呼び出してこちらの攻撃にする
					StockWeapon.GetComponent<ThrowWeaponScript>().PlyaerAttack();

					//飛び道具にヒットエフェクトを渡す
					StockWeapon.GetComponent<ThrowWeaponScript>().HitEffect = GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect12").ToArray()[0];

					//飛び道具を敵に飛ばす
					StockWeapon.GetComponent<Rigidbody>().AddForce(((Enemy.transform.position + Vector3.up) - StockWeapon.transform.position).normalized * 30, ForceMode.Impulse);

					//飛び道具を回転させる
					StockWeapon.GetComponent<Rigidbody>().AddTorque(Player.transform.right, ForceMode.Impulse);

					//くっつけた飛び道具を解放
					StockWeapon = null;
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//親を解除
						Weapon.transform.parent = null;

						//飛び道具のRigidBody有効化
						Weapon.GetComponent<Rigidbody>().isKinematic = false;

						//飛び道具の関数を呼び出してこちらの攻撃にする
						Weapon.GetComponent<ThrowWeaponScript>().PlyaerAttack();

						//飛び道具にヒットエフェクトを渡す
						Weapon.GetComponent<ThrowWeaponScript>().HitEffect = GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect12").ToArray()[0];

						//飛び道具を敵に飛ばす
						Weapon.GetComponent<Rigidbody>().AddForce(((Enemy.transform.position + (Vector3.up * 0.75f)) - Weapon.transform.position).normalized * 30, ForceMode.Impulse);

						//飛び道具を回転させる
						Weapon.GetComponent<Rigidbody>().AddTorque(Player.transform.right, ForceMode.Impulse);
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;

						//プレイヤーのフラグを下ろす
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;
					}
				);
			}
			//賎祓い
			else if (i == 1)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//飛び道具の関数を呼び出して無害化
						Weapon.GetComponent<ThrowWeaponScript>().PhysicOBJ();

						//飛び道具のRigidBody無効化
						Weapon.GetComponent<Rigidbody>().isKinematic = true;

						//周囲の敵List初期化
						AroundEnemyList = new List<GameObject>();

						//全ての敵を回す
						foreach (GameObject ii in GameManagerScript.Instance.AllActiveEnemyList)
						{
							//nullチェック
							if (ii != null && !ii.GetComponent<EnemyCharacterScript>().DownFlag)
							{
								//近くの敵を検出
								if ((Player.transform.position - ii.transform.position).sqrMagnitude < 6.5f)
								{
									//ListにAdd
									AroundEnemyList.Add(ii);
								}
							}
						}

						//エフェクトのインスタンス生成
						GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect12").ToArray()[0]);

						//エフェクトを飛び道具の場所に移動
						HitEffect.transform.position = Weapon.transform.position;

						//見つかった敵に賎祓い発動
						if (AroundEnemyList.Count > 0)
						{
							//飛び道具にヒットエフェクトを渡す
							Weapon.GetComponent<ThrowWeaponScript>().HitEffect = GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect12").ToArray()[0];

							//賎祓い処理呼び出し
							Weapon.GetComponent<ThrowWeaponScript>().SpecialArts110(AroundEnemyList);
						}
						//周囲に敵がいない場合は投げてきた敵に返す
						else
						{
							//親を解除
							Weapon.transform.parent = null;

							//飛び道具のRigidBody有効化
							Weapon.GetComponent<Rigidbody>().isKinematic = false;

							//飛び道具の関数を呼び出してこちらの攻撃にする
							Weapon.GetComponent<ThrowWeaponScript>().PlyaerAttack();

							//飛び道具にヒットエフェクトを渡す
							Weapon.GetComponent<ThrowWeaponScript>().HitEffect = GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect12").ToArray()[0];

							//飛び道具を敵に飛ばす
							Weapon.GetComponent<Rigidbody>().AddForce(((Enemy.transform.position + (Vector3.up * 0.75f)) - Weapon.transform.position).normalized * 30, ForceMode.Impulse);

							//飛び道具を回転させる
							Weapon.GetComponent<Rigidbody>().AddTorque(Player.transform.right, ForceMode.Impulse);
						}
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;

						//プレイヤーのフラグを下ろす
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;
					}
				);

			}
			//幣帛召し
			else if (i == 2)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//特殊攻撃制御フラグを立てる
						SpecialAction110Flag = true;

						//プレイヤー移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction110(Player));
					}
				);
				//プレイヤー行動コルーチン
				IEnumerator PlayerSpecialAction110(GameObject Player)
				{
					//フラグが降りるまでループ
					while (SpecialAction110Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -Player.transform.forward * 10f;

						//1フレーム待機
						yield return null;
					}

					//プレイヤーの移動ベクトル初期化
					Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//特殊攻撃制御フラグを立てる
						SpecialAction110Flag = false;

						//エフェクトのインスタンス生成
						GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect12").ToArray()[0]);

						//エフェクトを飛び道具の場所に移動
						HitEffect.transform.position = Weapon.transform.position;

						//ストックしてる飛び道具がなければストック
						if (StockWeapon == null)
						{
							//くっつけた飛び道具をキャッシュ
							StockWeapon = Weapon;

							//プレイヤーのフラグを立てる
							Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

							//飛び道具のストックフラグを立てる
							//Weapon.GetComponent<ThrowWeaponScript>().StockFlag = true;

							//飛び道具の関数を呼び出して無害化
							Weapon.GetComponent<ThrowWeaponScript>().PhysicOBJ();

							//飛び道具のコライダー無効化
							Weapon.GetComponent<MeshCollider>().enabled = false;

							//飛び道具のRigidBody無効化
							Weapon.GetComponent<Rigidbody>().isKinematic = true;

							//飛び道具を剣先にアタッチ
							foreach (WeaponSettingScript ii in Player.GetComponentsInChildren<WeaponSettingScript>())
							{
								if (ii.name.Contains("_0"))
								{
									Weapon.transform.parent = ii.transform;

									Weapon.transform.localRotation = Quaternion.Euler(new Vector3(180, 0, 180));

									Weapon.transform.localPosition = Weapon.GetComponent<ThrowWeaponScript>().StockPosition;
								}
							}

							//ゲームマネージャーのリストから自身を削除
							GameManagerScript.Instance.AllEnemyWeaponList.Remove(Weapon);
						}
						//ストックしている飛び道具があったら賎祓いにする
						else
						{
							//親を解除
							Weapon.transform.parent = null;

							//飛び道具のRigidBody有効化
							Weapon.GetComponent<Rigidbody>().isKinematic = false;

							//飛び道具の関数を呼び出してこちらの攻撃にする
							Weapon.GetComponent<ThrowWeaponScript>().PlyaerAttack();

							//飛び道具にヒットエフェクトを渡す
							Weapon.GetComponent<ThrowWeaponScript>().HitEffect = GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect12").ToArray()[0];

							//飛び道具を敵に飛ばす
							Weapon.GetComponent<Rigidbody>().AddForce(((Enemy.transform.position + Vector3.up) - Weapon.transform.position).normalized * 30, ForceMode.Impulse);

							//飛び道具を回転させる
							Weapon.GetComponent<Rigidbody>().AddTorque(Player.transform.right, ForceMode.Impulse);
						}
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーの移動ベクトル初期化
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;

						//プレイヤーのフラグを下ろす
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;
					}
				);
			}
		}

		//泉
		else if (c == 2)
		{
			//蝦蟇喰
			if (i == 0)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//移動ベクトルを設定
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -gameObject.transform.forward * 5;
					}
				);

				//敵行動コルーチン
				IEnumerator EnemySpecialAction200(GameObject Player, GameObject Enemy)
				{
					//敵をキャラクターに向ける
					Enemy.transform.LookAt(new Vector3(Player.transform.position.x, Enemy.transform.position.y, Player.transform.position.z));

					//移動開始時間をキャッシュ
					float temptime = Time.time;

					//時間が過ぎるまでループ、途中で敵が死んだりしたら抜ける
					while (temptime + 0.75f > Time.time && Enemy != null)
					{
						//目的地まで移動
						Enemy.GetComponent<EnemyCharacterScript>().SpecialMoveVec = HorizontalVector(Player, Enemy) * 2f;

						//1フレーム待機
						yield return null;
					}

					//移動値リセット
					Enemy.GetComponent<EnemyCharacterScript>().SpecialMoveVec *= 0;

					//敵のフラグを下ろす
					Enemy.GetComponent<EnemyCharacterScript>().SpecialFlag = false;
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//移動ベクトルをリセット
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//移動ベクトルを設定
						Player.GetComponent<PlayerScript>().SpecialMoveVector = -gameObject.transform.forward * 2;

						//敵のフラグを立てる
						Enemy.GetComponent<EnemyCharacterScript>().SpecialFlag = true;

						//敵のアニメーター遷移フラグを立てる
						Enemy.GetComponent<Animator>().SetBool("Special", true);

						//使用するモーションに差し替え
						Enemy.GetComponent<EnemyCharacterScript>().OverRideAnimator["Special_void"] = Enemy.GetComponent<EnemyCharacterScript>().DamageAnimList[Arts.DamageIndex];

						//アニメーターを上書きしてアニメーションクリップを切り替える
						Enemy.GetComponent<Animator>().runtimeAnimatorController = Enemy.GetComponent<EnemyCharacterScript>().OverRideAnimator;

						//敵移動コルーチン呼び出し
						StartCoroutine(EnemySpecialAction200(Player, Enemy));

						//武器巻き戻し処理呼び出し
						ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.MoveWire(2));
					}
				);

				re.Add
				(					
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//移動ベクトルをリセット
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;

						//プレイヤーのフラグを下ろす
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;
					}
				);
			}
			//躙蜘蛛
			else if (i == 1)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//移動ベクトルを設定
						Player.GetComponent<PlayerScript>().SpecialMoveVector = gameObject.transform.forward * 4;
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//制御フラグを立てる
						SpecialAction210Flag = true;

						//移動コルーチン呼び出し
						StartCoroutine(PlayerSpecialAction210(Player, Enemy));
					}
				);

				//移動コルーチン
				IEnumerator PlayerSpecialAction210(GameObject Player, GameObject Enemy)
				{
					//モーションをゆっくりにする
					Player.GetComponent<Animator>().SetFloat("SpecialAttackSpeed", 0.25f);

					//ある程度近付くまでループ
					while (HorizontalVector(Player,Enemy).sqrMagnitude > 5)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = transform.forward * (((Enemy.transform.position - (gameObject.transform.forward * 0.5f)) - gameObject.transform.position).magnitude) * 2.5f;

						//1フレーム待機
						yield return null;
					}

					//モーション速度を戻す
					Player.GetComponent<Animator>().SetFloat("SpecialAttackSpeed", 1);

					//移動ベクトルを設定
					//Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;
					/*
					while (SpecialAction210Flag)
					{
						//目的地まで移動
						Player.GetComponent<PlayerScript>().SpecialMoveVector = transform.forward * (((Enemy.transform.position - (gameObject.transform.forward * 0.5f)) - gameObject.transform.position).magnitude) * 2.5f;
	
						//1フレーム待機
						yield return null;
					}
					
					//移動ベクトルを設定
					Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;
					*/
				}

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//制御フラグを下す
						SpecialAction210Flag = false;

						//移動ベクトルを設定
						Player.GetComponent<PlayerScript>().SpecialMoveVector = gameObject.transform.forward;

						//武器巻き戻し処理呼び出し
						ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.MoveWire(2));
					}
				);

				re.Add
				(					
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//移動ベクトルをリセット
						Player.GetComponent<PlayerScript>().SpecialMoveVector *= 0;

						//ヒットエフェクトインスタンス生成
						GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect20").ToList()[0]);

						//プレイヤーの子にする
						HitEffect.transform.parent = Player.transform;

						//PRS設定
						HitEffect.transform.position = Enemy.transform.position + new Vector3(0, 0.75f, 0);
						HitEffect.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Enemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0, 1, 0.1f) }, new List<float>() { 10 }, new List<int>() { 1 }, new List<int>() { 1 }, new List<int>() { 0 }, new List<int>() { 0 }), 0));
					}
				);


				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを下ろす
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;
					}
				);
			}
			//蔓燐糞
			else if (i == 2)
			{
				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを立てる
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = true;

						//武器移動処理呼び出し
						ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.MoveWire(3));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//ワイヤー波打ちアニメ呼び出し
						ExecuteEvents.Execute<WireShaderScriptInterface>(gameObject.GetComponent<Character2WeaponMoveScript>().WireOBJ, null, (reciever, eventData) => reciever.WireWave(0.2f));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//ヒットエフェクトインスタンス生成
						GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect20").ToList()[0]);

						//プレイヤーの子にする
						HitEffect.transform.parent = Player.transform;

						//PRS設定
						HitEffect.transform.position = Enemy.transform.position + new Vector3(0, 1.5f, 0);
						HitEffect.transform.localRotation = Quaternion.Euler(new Vector3(45, 0, 0));

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Enemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0, 1, 0.1f) }, new List<float>() { 0 }, new List<int>() { 0 }, new List<int>() { 3 }, new List<int>() { 0 }, new List<int>() { 0 }), 0));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//ヒットエフェクトインスタンス生成
						GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect20").ToList()[0]);

						//プレイヤーの子にする
						HitEffect.transform.parent = Player.transform;

						//PRS設定
						HitEffect.transform.position = Enemy.transform.position + new Vector3(0, 1.5f, 0);
						HitEffect.transform.localRotation = Quaternion.Euler(new Vector3(-45, 0, 0));

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Enemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0, 1, 0.1f) }, new List<float>() { 0 }, new List<int>() { 0 }, new List<int>() { 4 }, new List<int>() { 0 }, new List<int>() { 0 }), 0));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//ワイヤー波打ちアニメ呼び出し
						ExecuteEvents.Execute<WireShaderScriptInterface>(gameObject.GetComponent<Character2WeaponMoveScript>().WireOBJ, null, (reciever, eventData) => reciever.WireWave(-0.1f));

						//ヒットエフェクトインスタンス生成
						GameObject HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == "HitEffect21").ToList()[0]);

						//敵の子にする
						HitEffect.transform.parent = Enemy.transform;

						//PRS設定
						HitEffect.transform.localPosition = new Vector3(0, 0, 0);
						HitEffect.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));

						//敵側の処理呼び出し、架空の技を渡して技が当たった事にする
						ExecuteEvents.Execute<EnemyCharacterInterface>(Enemy, null, (reciever, eventData) => reciever.PlayerAttackHit(MakeInstantArts(new List<Color>() { new Color(0, 0, 0, 0.1f) }, new List<float>() { 0 }, new List<int>() { 0 }, new List<int>() { 6 }, new List<int>() { 0 }, new List<int>() { 0 }), 0));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//燐糞を生成
						ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.CreateBomb());

						//左手に持たせる
						ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.SettingBombPos(DeepFind(gameObject, "L_HandBone").transform, new Vector3(-0.02f, 0.05f, -0.07f)));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//右手に持たせる
						ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.SettingBombPos(DeepFind(gameObject, "R_HandBone").transform, new Vector3(0, 0.07f, -0.07f)));
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//燐糞を敵に向かって移動させる
						ExecuteEvents.Execute<Character2WeaponMoveInterface>(gameObject, null, (reciever, eventData) => reciever.SpecialBombMove());
					}
				);

				re.Add
				(
					(GameObject Player, GameObject Enemy, GameObject Weapon, SpecialClass Arts) =>
					{
						//プレイヤーのフラグを下す
						Player.GetComponent<PlayerScript>().SpecialAttackFlag = false;
					}
				);
			}
		}

		//出力
		return re;
	}
}
