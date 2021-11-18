using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectScript : GlobalClass
{
	//オーディオの名前
	public string AudioName;

	//持っているオーディオリスト
	public List<AudioClip> AudioList;

	//オーディオソース
	private AudioSource Source;

    void Start()
    {
		//オーディオソース取得
		Source = GetComponent<AudioSource>();
	}

	//指定されたインデックスの音を鳴らす
	public void PlaySoundEffect(int i)
	{
		if(!GameManagerScript.Instance.SoundOffSwicth)
		{
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
