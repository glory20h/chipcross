﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveBoi : MonoBehaviour
{
    /* 파랭이를 움직이게 만드는 Script */
    public Event eventChanger;

    public GameObject TileBoard;
    public Button GoToNextLevelBtn;
    public Button GoNFasterButton;
    public Button ResetButton;
    public Button HintButton;

    public GameObject PuzzleSolvedPanel;

    public Animator boyAnimator;

    [HideInInspector] public float speed;
    [HideInInspector] public float distanceBetweenTiles;

    [HideInInspector] public Vector3 initTargetPosition;
    Vector3 targetPosition; // 이거 중요함....
    Vector3 boiInitPos;

    [HideInInspector] public bool isMoving;
    bool isThereNextTile;
    bool metGirl;

    float fastForwardFactor;
    float flickForce;
    IEnumerator addFriction;

    char tileType;
    int xdir;
    int ydir;
    int temp;

    //Warp Tile용 변수
    bool warp;
    bool warpDone;

    int numOfSteps;

    void Start()
    {
        isMoving = false;
        metGirl = false;
        speed = 2f;
        warp = false;
        warpDone = false;
        addFriction = AddFriction();
        boyAnimator = gameObject.GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            BoyMove();
        }
    }

    void BoyMove()
    {
        //targetPosition(목표 타일의 중심)을 향해서 이동...
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * fastForwardFactor * flickForce * Time.deltaTime);
        //목표 지점에 도착
        if (transform.position == targetPosition)
        {
            if (isThereNextTile)     //파랭이가 아직 타일 위에 있는가?
            {
                if (warp)            //Warp 해야 하는가?
                {
                    boyAnimator.enabled = false;// -> Pause Animator
                    gameObject.GetComponent<SpriteRenderer>().sprite = (Sprite)Resources.Load("Arts/Nothing", typeof(Sprite));

                    if (tileType == '8')
                    {
                        if (GameObject.Find("Tile9(Clone)"))        //반대편 Warp 출구가 FixedTile 인지 Tile 인지 확인
                        {
                            gameObject.transform.position = GameObject.Find("Tile9(Clone)").transform.position;
                        }
                        else
                        {
                            gameObject.transform.position = GameObject.Find("FixedTile9(Clone)").transform.position;
                        }
                    }
                    else if (tileType == '9')
                    {
                        if (GameObject.Find("Tile8(Clone)"))       //반대편 Warp 출구가 FixedTile 인지 Tile 인지 확인
                        {
                            gameObject.transform.position = GameObject.Find("Tile8(Clone)").transform.position;
                        }
                        else
                        {
                            gameObject.transform.position = GameObject.Find("FixedTile8(Clone)").transform.position;
                        }
                    }

                    StartCoroutine(Waitsecond(0.3f));

                    warp = false;
                    warpDone = true;
                }

                //사운드 FX 재생
                if (tileType != '1')
                {
                    SoundFXPlayer.Play("flick");
                    flickForce = 2.5f;
                }

                //xdir, ydir로 targetPosition 갱신
                targetPosition = transform.position + new Vector3(xdir * distanceBetweenTiles, ydir * distanceBetweenTiles, 0);
                isThereNextTile = false;
            }
            else                    //Finish moving; Reached Girl? Or try try again??
            {
                eventChanger.ResetGoNFaster();
                isMoving = false;
                StopCoroutine(addFriction);

                if (metGirl) //Correct Solution
                {
                    //Debug.Log("Puzzle solved!! Congrats!! :)");
                    StartCoroutine(PuzzleSolved());
                    GoNFasterButton.interactable = false;
                    ResetButton.interactable = false;
                    HintButton.interactable = false;
                    metGirl = false;
                }
                else //Wrong Solution
                {
                    SoundFXPlayer.Play("fail");
                    StartCoroutine(DelayBoyFail(1.5f));
                    //Debug.Log("Try try again!");
                }
            }

            numOfSteps++;
        }
    }

    public void MoveDaBoi()
    {
        boiInitPos = transform.position;
        isThereNextTile = true;
        fastForwardFactor = 0.5f;
        xdir = 1;
        ydir = 0;
        targetPosition = initTargetPosition;
        GetComponent<BoxCollider2D>().enabled = true;
        isMoving = true;
        warp = false;
        StartCoroutine(addFriction);
        flickForce = 2.5f;
        numOfSteps = 0;
    }

    public void FastForward()
    {
        fastForwardFactor = 2f;
    }

    public void BackToNormalSpeed()
    {
        fastForwardFactor = 0.5f;
    }

    public void ResetBoyMove()
    {
        //Make the boy stop moving and return to its original position
        eventChanger.ResetGoNFaster();
        eventChanger.MovePieceMode = true;
        ResetBoyPosition();
        isMoving = false;
        StopCoroutine(addFriction);
    }

    public void ResetBoyPosition()
    {
        transform.position = boiInitPos;
        //NEED something more???
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Tile" || collision.tag == "Hint")
        {
            if (warpDone)           //워프에서 반대편으로 나왔을때 바로 타일 탐지하는것 방지
            {
                warpDone = false;
            }
            else
            {
                isThereNextTile = true;
                tileType = collision.gameObject.name[4];

                switch (tileType)
                {
                    case '1':
                        break;
                    case '2':
                        xdir = -1;
                        ydir = 0;
                        break;
                    case '3':
                        xdir = 1;
                        ydir = 0;
                        break;
                    case '4':
                        xdir = 0;
                        ydir = -1;
                        break;
                    case '5':
                        xdir = 0;
                        ydir = 1;
                        break;
                    case '6':
                        temp = xdir;
                        xdir = -ydir;
                        ydir = temp;
                        break;
                    case '7':
                        temp = xdir;
                        xdir = ydir;
                        ydir = -temp;
                        break;
                    case '8':
                        /*
                        gameObject.transform.position = GameObject.Find("Tile9(Clone)").transform.position;
                        targetPosition = GameObject.Find("Tile9(Clone)").transform.position;
                        warpDone = true;
                        */
                        warp = true;
                        break;
                    case '9':
                        /*
                        gameObject.transform.position = GameObject.Find("Tile8(Clone)").transform.position;
                        targetPosition = GameObject.Find("Tile8(Clone)").transform.position;
                        warpDone = true;
                        */
                        warp = true;
                        break;
                }
            }

            /*if (warpDone && (tileType == '8' || tileType == '9'))
            {
                tileType = '1';
                warpDone = false;
            }*/

        }
        else if(collision.tag == "FixedTile")
        {
            if (warpDone)           //워프에서 반대편으로 나왔을때 바로 타일 탐지하는것 방지
            {
                warpDone = false;
            }
            else
            {
                isThereNextTile = true;
                tileType = collision.gameObject.name[9];

                switch (tileType)
                {
                    case '1':
                        break;
                    case '2':
                        xdir = -1;
                        ydir = 0;
                        break;
                    case '3':
                        xdir = 1;
                        ydir = 0;
                        break;
                    case '4':
                        xdir = 0;
                        ydir = -1;
                        break;
                    case '5':
                        xdir = 0;
                        ydir = 1;
                        break;
                    case '6':
                        temp = xdir;
                        xdir = -ydir;
                        ydir = temp;
                        break;
                    case '7':
                        temp = xdir;
                        xdir = ydir;
                        ydir = -temp;
                        break;
                    case '8':
                        /*
                        gameObject.transform.position = GameObject.Find("Tile9(Clone)").transform.position;
                        targetPosition = GameObject.Find("FixedTile9(Clone)").transform.position;
                        warpDone = true;
                        */
                        warp = true;
                        break;
                    case '9':
                        /*
                        gameObject.transform.position = GameObject.Find("Tile8(Clone)").transform.position;
                        targetPosition = GameObject.Find("FixedTile8(Clone)").transform.position;
                        warpDone = true;
                        */
                        warp = true;
                        break;
                }
            }

            /*if (warpDone && (tileType == '8' || tileType == '9'))
            {
                tileType = '1';
                warpDone = false;
            }*/

        }
        else if(collision.gameObject.name == "Girl")
        {
            xdir = 0;
            ydir = 0;
            metGirl = true;
        }
    }

    IEnumerator AddFriction()
    {
        while(true)
        {
            if(flickForce >= 1.5f)
            {
                flickForce -= Time.deltaTime * 1.2f;
            }
            yield return null;
        }
    }

    IEnumerator DelayBoyFail(float time)
    {
        yield return new WaitForSeconds(time);
        eventChanger.MovePieceMode = true;
        transform.position = boiInitPos;
        eventChanger.timeCount = true;
    }

    //퍼즐 완료시(When Boi successfully met girl)
    IEnumerator PuzzleSolved()
    {
        /* This part related to CoinAnimation -> Disabled for now
        if (eventChanger.coinChangeToggle == false)
        {
            StopCoroutine(eventChanger.CoinIncreaseAnimation());
        }
        */
        yield return new WaitForSeconds(0.6f);

        eventChanger.DisplayPlayData();
        eventChanger.DisplayTime();
        eventChanger.ChangeRating(numOfSteps - 1);
        PuzzleSolvedPanel.SetActive(true);
        eventChanger.finger1.SetActive(false);
        SoundFXPlayer.Play("positiveVibe");
        //yield return StartCoroutine(eventChanger.CoinIncreaseAnimation()); //This part is related to CoinAnimation -> Disabled for now
    }

    IEnumerator Waitsecond(float time)
    {
        speed = 0f;
        yield return new WaitForSeconds(time);
        boyAnimator.enabled = true;
        speed = 2f;
    }
}
