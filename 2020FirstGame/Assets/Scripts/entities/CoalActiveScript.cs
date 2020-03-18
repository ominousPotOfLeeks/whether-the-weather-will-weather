using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalActiveScript : MonoBehaviour
{
    private EntityScript myEntityScript;

    public EntityController entityController;

    public int coalAmount;

    void Awake()
    {
        InitializeEntityScript();
        coalAmount = entityController.coalResourceAmount;
    }

    private void InitializeEntityScript()
    {
        myEntityScript = GetComponent<EntityScript>();
        myEntityScript.step = Step;
    }

    public bool Step()
    {
        return false;
    }
}
