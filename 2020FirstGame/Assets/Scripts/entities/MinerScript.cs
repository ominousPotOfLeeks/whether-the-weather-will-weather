using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerScript : MonoBehaviour
{

    private EntityScript myEntityScript;

    private int counter = 0;
    private bool isOn = false;

    void Awake()
    {
        InitializeEntityScript();
    }

    private void InitializeEntityScript()
    {
        myEntityScript = GetComponent<EntityScript>();
        myEntityScript.step = Step;
    }

    public bool Step()
    {
        //check if coal underneath
        //if there is no coal entity, make one
        //decrease amount of coal in coal entity

        if (isOn)
        {
            counter++;
            if (counter > 50)
            {
                Debug.Log("Mine");
            }
        }

        //never moves
        return false;
    }
}
