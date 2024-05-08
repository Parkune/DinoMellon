using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{


    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;

    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1,30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;

    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx {LevelUp, Next, Attach, Button, Over}
    int sfxCursor;

    public int maxLevel;
    public int score;
    public bool isOver;



    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++)
        {
            MakeDongle();
        }
    }


    void Start()
    {
        bgmPlayer.Play();
        NextDongle();
    }

    Dongle MakeDongle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 동글 생성
        GameObject instant = Instantiate(donglePrefab, dongleGroup);
        instantEffect.name = "Dongle" + donglePool.Count;
        Dongle instantDongle = instant.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }
    


    Dongle GetDongle()
    {
        for(int index = 0; index < donglePool.Count; index++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }

    void NextDongle()
    {
        if (isOver)
        {
            return;
        }
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }


    IEnumerator WaitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2.5f);

        NextDongle();
    }


    public void TouchDown()
    {
        if (lastDongle == null)
        return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if (isOver)
        {
            return;
        }

        isOver = true;

        StartCoroutine("GameOverRoutine");
    }


    IEnumerator GameOverRoutine()
    {
        //1. 장면에 남은 모든 동글 리스트에 담기
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 5000);
            dongles[index].rigid.simulated = false;
        }


        //2. 1의 결과에 따라 스코어에 점수 더해주기 그리고 지우기
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 5000);
            yield return new WaitForSeconds(0.1f);
        }


        yield return new WaitForSeconds(1f);

        SfxPlay(Sfx.Over);
    }

    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0,3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor= (sfxCursor +1) % sfxPlayer.Length;
    }

}
