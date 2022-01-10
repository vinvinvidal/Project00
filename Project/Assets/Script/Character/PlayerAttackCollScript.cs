using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//メッセージシステムでイベントを受け取るためのインターフェイス
public interface PlayerAttackCollInterface : IEventSystemHandler
{
	//コライダ出現処理
	void ColStart(int n, ArtsClass arts);

	//超必殺技コライダ出現処理
	void StartSuperCol(Vector3 pos, Vector3 size);

	//コライダ終了処理
	void ColEnd();

	//ポーズフラグを受け取るインターフェイス
	void SetPauseFlag(bool b);
}

public class PlayerAttackCollScript : GlobalClass, PlayerAttackCollInterface
{
	//変数宣言

	//プレイヤーが操作するキャラクター
	private GameObject PlayerCharacter;

	//ヒットエフェクト
	public GameObject HitEffect { get; set; }

	//攻撃用コライダ
	private BoxCollider AttackCol;

	//プレイヤーが放った攻撃
	ArtsClass HitArts;

	//当たった攻撃のタイプを判別するインデックス番号
	int AttackIndex;

	//超必殺技フラグ
	bool SuperArtsFlag = false;

	//コライダ移動ベクトル
	Vector3 ColVec;

	//ポーズフラグ
	bool PauseFlag = false;

	private void Start()
	{
		//プレイヤーが操作するキャラクター
		PlayerCharacter = transform.root.gameObject;

		//プレイヤーが放った攻撃初期化
		HitArts = null;

		//攻撃用コライダ取得
		//AttackCol = GetComponent<SphereCollider>();
		AttackCol = GetComponent<BoxCollider>();

		//当たった攻撃のタイプを判別するインデックス番号初期化
		AttackIndex = 0;

		//コライダ移動フラグ初期化
		//KeepColMove = true;

		//コライダ移動ベクトル初期化
		ColVec *= 0;
	}

	//攻撃コライダーが敵に当たった時に呼び出される
	private void OnTriggerEnter(Collider Hit)
	{
		//攻撃が有効か判定する変数宣言
		bool AttackEnable = false;

		//超必殺技が当たった
		if (SuperArtsFlag)
		{
			//有効なシチュエーションか判定
			ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => AttackEnable = reciever.SuperEnable(PlayerCharacter.GetComponent<PlayerScript>().SuperArts.Location));
			
			//有効なら処理
			if (AttackEnable)
			{
				//攻撃が当たった時のプレイヤー側の処理を呼び出す
				ExecuteEvents.Execute<PlayerScriptInterface>(PlayerCharacter, null, (reciever, eventData) => reciever.HitAttack(Hit.gameObject.transform.root.gameObject, 0));

				//コライダを無効化
				AttackCol.enabled = false;
			}
		}
		//通常技が当たった
		else
		{
			//攻撃が有効か判定
			ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => AttackEnable = reciever.AttackEnable(HitArts, AttackIndex));

			//攻撃が有効なら処理実行
			if (AttackEnable)
			{
				//巻き込み攻撃でなければコライダを非アクティブ化
				if (HitArts.ColType[AttackIndex] != 7 && HitArts.ColType[AttackIndex] != 8)
				{
					AttackCol.enabled = false;
				}

				//攻撃が当たった時の敵側の処理を呼び出す
				ExecuteEvents.Execute<EnemyCharacterInterface>(Hit.gameObject.transform.root.gameObject, null, (reciever, eventData) => reciever.PlayerAttackHit(HitArts, AttackIndex));

				//攻撃が当たった時のプレイヤー側の処理を呼び出す
				ExecuteEvents.Execute<PlayerScriptInterface>(PlayerCharacter, null, (reciever, eventData) => reciever.HitAttack(Hit.gameObject.transform.root.gameObject, AttackIndex));

				//ちゃんと技がある場合処理実行
				if (HitArts.HitEffectList[AttackIndex] != "null")
				{
					//使用するヒットエフェクトのインスタンス生成
					HitEffect = Instantiate(GameManagerScript.Instance.AllParticleEffectList.Where(a => a.name == HitArts.HitEffectList[AttackIndex]).ToArray()[0]);

					//通常の攻撃、プレイヤーの前にヒットエフェクトを出す
					if (HitArts.ColType[AttackIndex] == 0 || HitArts.ColType[AttackIndex] == 1 || HitArts.ColType[AttackIndex] == 6)
					{
						//キャラクターの子にする
						HitEffect.transform.parent = gameObject.transform.root.transform;

						//ローカル座標で位置と回転を設定
						HitEffect.transform.localRotation = Quaternion.Euler(HitArts.HitEffectAngleList[AttackIndex]);

						//ローカル座標で位置を設定
						HitEffect.transform.localPosition = HitArts.HitEffectPosList[AttackIndex];
					}
					//飛び道具、敵の座標に出す
					else if (HitArts.ColType[AttackIndex] == 2 || HitArts.ColType[AttackIndex] == 3)
					{
						//キャラクターから敵までのベクトルを正面とする
						Vector3 EffectForward = (Hit.gameObject.transform.root.gameObject.transform.position - PlayerCharacter.transform.position).normalized;

						//ローカル座標で回転を設定
						HitEffect.transform.rotation = Quaternion.LookRotation(EffectForward);

						//位置を指定、敵からの相対位置で出す
						HitEffect.transform.position = Hit.gameObject.transform.root.gameObject.transform.position + (EffectForward * HitArts.HitEffectPosList[AttackIndex].z) + new Vector3(0, HitArts.HitEffectPosList[AttackIndex].y, 0);
					}

					//親を解除する
					HitEffect.transform.parent = null;
				}
			}
		}
	}

	//コライダ出現処理、メッセージシステムから呼び出される
	public void ColStart(int n, ArtsClass arts)
	{
		//まれにnullが入るのでエラー回避
		if (arts == null)
		{
			print("ColStartでArtsがnull");
		}
		else
		{
			//攻撃のインデックス番号をキャッシュ
			AttackIndex = n;

			//攻撃をキャッシュ
			HitArts = arts;

			//コライダをアクティブ化
			AttackCol.enabled = true;

			//コライダ移動コルーチン呼び出し
			StartCoroutine(ColMove(HitArts.ColVec[n], HitArts.ColType[n], PlayerCharacter.GetComponent<PlayerScript>().ChargeLevel));
		}
	}

	//コライダを移動させるコルーチン
	IEnumerator ColMove(Color c, int t, int l)
	{
		//コライダ移動ベクトルに値を入れる
		ColVec = new Vector3(c.r, c.g, c.b);

		//コライダの大きさを入れる
		AttackCol.size = new Vector3(0.5f, c.a, c.a);

		//固定位置
		if (t == 0 || t == 1 || t == 6)
		{
			//コライダを出現位置に移動
			AttackCol.center = ColVec;
		}
		//直線移動・上段
		else if (t == 2)
		{
			//コライダを出現位置に移動
			AttackCol.center = new Vector3(0, 1, 1f);

			//コライダ拡大
			while (AttackCol.size.z < ColVec.z * (l * 0.5f + 1))
			{
				if (!PauseFlag)
				{
					AttackCol.size += new Vector3(0, 0, ColVec.z * (l * 0.5f + 1)) * Time.deltaTime;

					AttackCol.center += new Vector3(0, 0, ColVec.z * 0.5f * (l * 0.5f + 1)) * Time.deltaTime;
				}

				yield return null;
			}
		}
		//範囲発生
		else if (t == 7 || t == 8)
		{
			//コライダを出現位置に移動
			AttackCol.center = ColVec;

			//コライダの大きさを入れる
			AttackCol.size = new Vector3(c.a, c.a, c.a);
		}

		//1フレーム待機
		yield return null;
	}

	//コライダ終了処理処理、メッセージシステムから呼び出される
	public void ColEnd()
	{
		//コライダを非アクティブ化
		AttackCol.enabled = false;

		//超必殺技フラグを下ろす
		SuperArtsFlag = false;

		//コライダを待機位置に移動
		AttackCol.center *= 0;

		//コライダの半径をデフォルトに戻す
		AttackCol.size = new Vector3(1, 1, 1);

		//当たった攻撃のタイプを判別するインデックス番号初期化
		AttackIndex = 0;

		//当たった攻撃を初期化
		HitArts = null;
	}

	//超必殺技コライダ出現処理
	public void StartSuperCol(Vector3 pos, Vector3 size)
	{
		//超必殺技フラグを立てる
		SuperArtsFlag = true;

		//コライダをアクティブ化
		AttackCol.enabled = true;

		//コライダを出現位置に移動
		AttackCol.center = pos;

		//コライダを大きさを設定
		AttackCol.size = size;
	}

	//ポーズフラグを受け取る
	public void SetPauseFlag(bool b)
	{
		PauseFlag = b;
	}
}
