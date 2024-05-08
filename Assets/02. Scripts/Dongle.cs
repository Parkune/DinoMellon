using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Dongle : MonoBehaviour
{
    // Start is called before the first frame update

    public GameManager manager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;


    public Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isDrag = false;
    }


    void OnEnable()
    {
     
        anim.SetInteger("Level", level);
        
    }

    void OnDisable()
    {

        //���� �Ӽ� �ʱ�ȭ
        level = 0;
        isDrag = false; 
        isMerge = false;
        isAttach = false;

        //���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        // ���� ���� �ʱ�ȭ
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }


    // Update is called once per frame
    void Update()
    {

        if (isDrag == true)
        {
            print(isDrag);
            //������ ���콺 �������� �����ϴ� ��
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
            //2D�� ������ 3���� ��ǥ���� �������ִ� ��
            mousePose.z = 0;
            //���콺�� ���󰡴� �ӵ�
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine("AttachRoutine");
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //���� ��ġ�� ���� 
                //���� ����� ��ġ ��������
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                // 1. ���� �Ʒ��� ���� �� 
                // 2. ������ ������ ��, ���� �����ʿ� ���� ��
                if (meY < otherY || (meY == otherY && meX > otherX))
                {
                    //������ �����
                    other.Hide(transform.position);
                    //���� ������
                    LevelUp();
                }
            }
        }
    }

    IEnumerator AttachRoutine()
    {
        if(isAttach)
        {
            yield break;
        }

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.8f);
        isAttach = false;
    }





    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //���� ��ġ�� ���� 
                //���� ����� ��ġ ��������
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;  
                // 1. ���� �Ʒ��� ���� �� 
                // 2. ������ ������ ��, ���� �����ʿ� ���� ��
                if (meY < otherY || (meY == otherY && meX > otherX))
                {
                    //������ �����
                    other.Hide(transform.position);
                    //���� ������
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

        if(targetPos == Vector3.up * 5000)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
        if (manager.isOver == true)
         return;
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        
        
        int frameCount = 0;

        while (frameCount < 20)
        {
            frameCount++;
            if(targetPos != Vector3.up * 5000)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }else if(targetPos == Vector3.up * 5000)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }

            yield return  null;
        }

        manager.score += (int)Mathf.Pow(2, level);

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
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);
        
        isMerge = false;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                spriteRenderer.color = new Color(0.77f, 0.2f, 0.2f);
            }
            if(deadTime > 5)
            {
                manager.GameOver();
            }

        }
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }


    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}
