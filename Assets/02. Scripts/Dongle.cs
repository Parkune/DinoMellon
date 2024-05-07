using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    // Start is called before the first frame update

    public GameManager manager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;


    Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
    }


    private void OnEnable()
    {
        print(level);
        anim.SetInteger("Level", level);
        
    }


    // Update is called once per frame
    void Update()
    {

        if (isDrag == true)
        {
            //동글의 마우스 포지션을 고정하는 것
            Vector3 mousePose = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            if (mousePose.x < leftBorder)
            {
                mousePose.x = leftBorder;
            }
            else if (mousePose.x > rightBorder)
            {
                mousePose.x = rightBorder;
            }

            mousePose.y = 8;
            //2D기 때문에 3차원 좌표값을 고정해주는 것
            mousePose.z = 0;
            //마우스를 따라가는 속도
            transform.position = Vector3.Lerp(transform.position, mousePose, 0.2f);
        }
    }


    public void Drag()
    {
        isDrag = true;
        rigid.simulated = false;
    }


    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //동글 합치기 로직 
                //나와 상대편 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;  
                // 1. 내가 아래에 있을 때 
                // 2. 동일한 높이일 때, 내가 오른쪽에 있을 때
                if (meY < otherY || (meY == otherY && meX > otherX))
                {
                    //상대방은 숨기기
                    other.Hide(transform.position);
                    //나는 레벨업
                    LevelUp();
                }
            }
        }
    }




    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rigid.simulated = false;
        circle.enabled = false;

        StartCoroutine(HideRoutine(targetPos));

    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        while (frameCount < 20)
        {
            frameCount++;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            yield return  null;
        }
        isMerge = false;  
        gameObject.SetActive(false);
    }


    public void LevelUp()
    {

        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());

    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level+1);
        EffectPlay();
        
        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);
        
        isMerge = false;
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}
