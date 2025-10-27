using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Push
{

    public override bool Move(Vector2 direction)
    {
        if(ObjToBlocked(transform.position, direction))
        {
            Destroy(gameObject);
            return false;
        }
        else
        {
            transform.Translate(direction);
            return true;
        }
    }

}
