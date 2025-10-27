using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Push : MonoBehaviour
{
    private GameObject[] Obstacles, ObjToPush, ObjMonster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Obstacles = GameObject.FindGameObjectsWithTag("Obstacles");
        ObjToPush = GameObject.FindGameObjectsWithTag("ObjToPush");
        ObjMonster = GameObject.FindGameObjectsWithTag("ObjMonster");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual bool Move(Vector2 direction)
    {
        if(ObjToBlocked(transform.position, direction))
        {
            return false;
        }
        else
        {
            transform.Translate(direction);
            return true;
        }
    }

    public bool ObjToBlocked(Vector3 position, Vector2 direction)
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
                return true;
            }
        }

        foreach (var objMonster in ObjMonster)
        {
            if (objMonster != null && objMonster.transform.position.x == newPos.x && objMonster.transform.position.y == newPos.y)
            {
                return true;
            }
        }

        return false;
    }
}
