using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{

    [Header("----------[Core]")]
    public bool isOver;
    public int maxLevel;
    public int score;
    public bool isStart;
    public bool IsPlayerDrag;
    public Vector3 padPosition;
    public int[] dongleDataList = new int[3];

    [Header("----------[Object Pooling]")]
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
    public GameObject lunchPad;

    [Header("----------[Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx {LevelUp, Next, Attach, Button, Over}
    int sfxCursor;

    [Header("----------[UI]")]
    //public Text scoreText;
    public GameObject startGroup;
    public GameObject endGroup;
    public TMP_Text scoreText;
    public TMP_Text maxScoreText;
    public TMP_Text subScoreText;
    public TMP_Text startScoreText;
    public GameObject playerPad;
    private LineRenderer guideLine;
    public GameObject nextImage;

    [Header("----------[BG]")]
    public GameObject line;
    public GameObject bottom;


    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++)
        {
            MakeDongle();
        }

        if (PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
        startScoreText.text = "최고:" + PlayerPrefs.GetInt("MaxScore").ToString();
    }



    public void GameStart()
    {
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        playerPad.SetActive(true);
        guideLine = playerPad.GetComponent<LineRenderer>();
        guideLine.positionCount = 2;
        isStart = true;
        bgmPlayer.Play();
        for(int index = 0;index < 3; index++) 
        {
            int num = Random.Range(0, maxLevel);
            dongleDataList[index] = num;
        }
        SfxPlay(Sfx.Button);
        nextImage.SetActive(true); 
        Invoke("NextDongle", 0.4f);
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
    //첫 시작 시 3개의 동글 생성
    //그걸 리스트에 담아라.
    //그걸 첫 동글로 지급해라
    //동글을 지급했다면 리스트를 재정렬해라.
    //만약 동글을 사용했으면 새로 마지막 순서로 뽑아라.

    void NextDongle()
    {
        if (isOver)
        {
            return;
        }
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.level = dongleDataList[0];
        lastDongle.gameObject.SetActive(true);
        GetDongleImage(dongleDataList[1]);
        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }

    //넥스트 동글이 호출되면, 미리 동글레벨을 알게된다. 
    //동글 레벨에 위치한 이미지를 호출해준다.

    public Image nextDongleImage;
    public Sprite[] dongleList;


    void GetDongleImage(int level)
    {
        nextDongleImage.sprite = dongleList[level];
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }

        dongleDataList[0] = dongleDataList[1];
        dongleDataList[1] = dongleDataList[2];
        dongleDataList[2] = Random.Range(0, maxLevel);

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
            dongles[index].rigid.simulated = false;
            //dongles[index].Hide(Vector3.up * 500000);
        }

        //2. 1의 결과에 따라 스코어에 점수 더해주기 그리고 지우기
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 500000);
            SfxPlay(Sfx.LevelUp);
            yield return new WaitForSeconds(0.1f);
        }


        yield return new WaitForSeconds(2f);

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        //게임 오버 UI 표시
        subScoreText.text = scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }


    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Main");
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

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    void LateUpdate()
    {
        scoreText.text = "Score:" + score.ToString();
        if(isStart == true && isOver == false)
        { 
            GuideLine();
        }
    }


    //너와 나 사이에 30개의 거리를 나누고
    //내 기준에서 1~2면 


    void GuideLine()
    {

        //동글의 마우스 포지션을 고정하는 것
        Vector3 mousePose = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float leftBorder = -4.2f + playerPad.transform.localScale.x / 2f;
        float rightBorder = 4.2f - playerPad.transform.localScale.x / 2f;

        if (mousePose.x < leftBorder)
        {
            mousePose.x = leftBorder;
        }
        else if (mousePose.x > rightBorder)
        {
            mousePose.x = rightBorder;
        }

        mousePose.y = 6.5f;
        //2D기 때문에 3차원 좌표값을 고정해주는 것
        mousePose.z = 0;
        //마우스를 따라가는 속도
        playerPad.transform.position = Vector3.Lerp(playerPad.transform.position, mousePose, 0.2f);


        Vector2 startPos = playerPad.transform.position;
        guideLine.SetPosition(0, startPos);

        RaycastHit2D hit;
        int layerMask = LayerMask.GetMask( "Guide", "Ball" );
        Vector3 rayOrigin = playerPad.transform.position;
        if(hit = Physics2D.Raycast(rayOrigin, Vector2.down, float.MaxValue, layerMask))
        {


            guideLine.SetPosition(1, hit.point);
            //guideLine.material.SetTextureScale(name, rayOrigin);
            //Vector2 endPos = hit.point;
            //Vector2 dotPos;
            //float dist = Vector2.Distance(startPos, endPos);
            // float dotDist = dist / 20;
            

            //int index = 0;
            /* for( int i = 0; i < 20; ++i)
             {
                 dotPos = startPos - new Vector2(0, + dotDist * i);

                 if( i % 2 == 1 )
                     guideLine.SetPosition(index++, dotPos);
             }
            */
        }
    }

}
