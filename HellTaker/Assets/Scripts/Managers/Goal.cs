using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Goal : MonoBehaviour
{
    private GameObject Player;
    private bool isInGoal,sendLog; // TODO : 변수 대신 프로퍼티 사용해보기

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        isInGoal = false;
        sendLog = false;
    }
    // Update is called once per frame
    void Update()
    {
        CheckPlayerGoal();
        CheckIsGoal();
    }

    void CheckPlayerGoal()
    {
        //if (Player == null) return;
        
        Vector2 playerPos = Player.transform.position;
        Vector2 goalPos = transform.position;

        float goalDistance = Mathf.Abs(playerPos.x - goalPos.x) + Mathf.Abs(playerPos.y - goalPos.y);

        if(goalDistance <= 1f)
        {
            isInGoal = true;
        }
        else
        {
            isInGoal = false;
            sendLog = false;
        }
    }
    
    void CheckIsGoal()
    {
        if (isInGoal && !sendLog)
        {
            Debug.Log("클리어!"); // TODO : 들어왔을 때 한 번만 메시지를 띄우지만, 약간의 딜레이 있음
            sendLog = true;
        }
    }
}
