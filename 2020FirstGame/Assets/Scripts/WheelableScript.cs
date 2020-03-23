using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelableScript : MonoBehaviour
{
    public EntityController.Entity parentEntity;
    public ToggleableScript parentToggleableScript;
    public bool isParent;

    public Action ToggleMovingState;
}
