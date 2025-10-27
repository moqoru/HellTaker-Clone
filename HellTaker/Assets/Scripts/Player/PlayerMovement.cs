using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private GameObject[] Obstacles, ObjToPush, ObjMonster;

    private int MoveCount;
    private bool ReadyToMove;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Obstacles = GameObject.FindGameObjectsWithTag("Obstacles");
        ObjToPush = GameObject.FindGameObjectsWithTag("ObjToPush");
        ObjMonster = GameObject.FindGameObjectsWithTag("ObjMonster");
        MoveCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveInput.Normalize();
        
        if(moveInput.sqrMagnitude > 0.5)
        {
            if(ReadyToMove)
            {
                ReadyToMove = false;
                if (Move(moveInput))
                {
                    MoveCount += 1;
                    Debug.Log($"움직인 횟수: {MoveCount}"); // TODO : 몬스터 파괴시, 움직이진 않지만 움직인 횟수 포함
                }
            }
        }
        else
        {
            ReadyToMove = true;
        }
    }

    public bool Move(Vector2 direction)
    {
        if(Mathf.Abs(direction.x) < 0.5)
        {
            direction.x = 0;
        }
        else
        {
            direction.y = 0;
        }
        direction.Normalize();

        if(Blocked(transform.position, direction))
        {
            return false;
        }
        else
        {
            transform.Translate(direction);
            return true;
        }
    }

    public bool Blocked(Vector3 position, Vector2 direction)
    {
        Vector2 newPos = new Vector2(position.x, position.y) + direction;

        foreach (var obj in Obstacles)
        {
            if (obj.transform.position.x == newPos.x && obj.transform.position.y == newPos.y)
            {
                return true;
            }
        }

        foreach (var objToPush in ObjToPush)
        {
            if (objToPush.transform.position.x == newPos.x && objToPush.transform.position.y == newPos.y)
            {
                Push oPush = objToPush.GetComponent<Push>();
                if (oPush && oPush.Move(direction))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        foreach (var objMonster in ObjMonster)
        {
            if (objMonster != null && objMonster.transform.position.x == newPos.x && objMonster.transform.position.y == newPos.y)
            {
                Monster oMonster = objMonster.GetComponent<Monster>();
                if (oMonster && oMonster.Move(direction))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }

}
