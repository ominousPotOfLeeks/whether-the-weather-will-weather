using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelableScript : MonoBehaviour
{
    [HideInInspector]
    public bool disbanded = true;

    public int power;//power to do wheel things (0 for non-wheel entities)
    public int mass;

    public Action ToggleMovingState;
}
