﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerScript : MonoBehaviour
{
    public TerrainController terrainController;
    private EntityScript entityScript;
    private ToggleableScript toggleableScript;

    public int mineIncrementSize;

    private int counter = 0;

    void Awake()
    {
        InitializeEntityScript();
        toggleableScript = GetComponent<ToggleableScript>();
        terrainController = GameObject.Find("TerrainController").GetComponent<TerrainController>();
    }

    private void InitializeEntityScript()
    {
        entityScript = GetComponent<EntityScript>();
        entityScript.step = Step;
    }

    private Tuple<int, int> MineResource(int totalResourceAmount, int increment)
    {
        //returns amount remaining and amount removed
        int result = totalResourceAmount - increment;
        if (result > 0)
        {
            return new Tuple<int, int> (result, increment);
        }
        else
        {
            return new Tuple<int, int>(0, totalResourceAmount);
        }
    }

    private void CollectCoal()
    {
        int tileID = terrainController.GetTileAtPosition(transform.position);
        if (tileID == terrainController.GetTileID("coal"))
        {
            //we are on coal
            Vector2Int roundedPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            int tileData = terrainController.terrainArray.GetData(roundedPosition.x, roundedPosition.y);
            Tuple<int, int> mineData;
            if (tileData == -1)
            {
                mineData = MineResource(terrainController.coalResourceAmount, mineIncrementSize);
            }
            else
            {
                mineData = MineResource(tileData, mineIncrementSize);
            }
            if (mineData.Item1 == 0)
            {
                mineData = new Tuple<int, int>(-1, mineData.Item2);
                terrainController.terrainArray.Set(roundedPosition.x, roundedPosition.y, terrainController.GetTileID("dirt"));
            }
            terrainController.terrainArray.SetData(roundedPosition.x, roundedPosition.y, mineData.Item1);

            Debug.LogFormat("Mined {0} coal, leaving {1}", mineData.Item2, mineData.Item1);
        } else
        {
            Debug.LogFormat("Can't mine, we are on {0}", tileID);
            toggleableScript.Toggle();
        }
    }

    public bool Step()
    {
        //check if coal underneath
        //if there is no coal entity, make one
        //decrease amount of coal in coal entity

        if (toggleableScript.state)
        {
            counter++;
            if (counter > 50)
            {
                CollectCoal();
                counter = 0;
            }
        }

        //never moves
        return false;
    }
}
