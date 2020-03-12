using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    //This class holds any data which ALL entities must have

    public Func<bool> step;
    public EntityController.Entity selfEntity;

}
