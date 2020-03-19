using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleableScript : MonoBehaviour
{
    //Script to attach to any object which needs to toggle states when clicked on
    public bool state = false;

    public bool Toggle()
    {
        if (state)
        {
            state = false;
        }
        else
        {
            state = true;
        }
        return state;
    }
}
