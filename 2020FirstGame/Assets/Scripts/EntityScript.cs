using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityScript : MonoBehaviour
{
    //This class holds any data which ALL entities must have

    public Func<EntityController.EntityStepData> step;
    public EntityController.Entity selfEntity;

    public Action initialize = () => { };
    public bool hasParent = false;

}
