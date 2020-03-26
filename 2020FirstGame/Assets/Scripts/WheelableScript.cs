using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelableScript : MonoBehaviour
{
    public EntityController.Entity parentEntity;

    [HideInInspector]
    public ToggleableScript parentToggleableScript;
    [HideInInspector]
    public bool isParent;

    public int power;//power to do wheel things (0 for non-wheel entities)
    public int mass;

    public Action ToggleMovingState;
}
