using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSoundEffectScript : GlobalClass
{
	//持っているオーディオリスト
	public List<AudioClip> AudioList { get; set; }

	//オーディオソース
	private AudioSource Source;

	//ボリューム初期値
	private float VolumeNum;

    void Start()
    {
		//オーディオソース取得
		Source = GetComponent<AudioSource>();

		//ボリュームキャッシュ
		VolumeNum = Source.volume;
	}

	//指定されたインデックスの音を鳴らす
	public void PlaySoundEffect(int i , float v)
	{
		if(!GameManagerScript.Instance.SoundOffSwicth)
		{
			Source.volume = VolumeNum + v;

			Source.PlayOneShot(AudioList[i]);
		}
	}

	//持っている音の中からランダムで再生する
	public void PlayRandomList()
	{
		if (!GameManagerScript.Instance.SoundOffSwicth)
		{
			Source.PlayOneShot(AudioList[Random.Range(0, AudioList.Count)]);
		}
	}
}
