using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WheelScript : MonoBehaviour
{
    private EntityController entityController;
    private TerrainController terrainController;
    private EntityScript entityScript;
    private ToggleableScript toggleableScript;
    private WheelableScript wheelableScript;

    private GameObject player;
    private BoxCollider2D boxCollider;

    public int totalWheelPower;//force with which tilemap is pushed
    public float wheelSpeed;

    public bool moving;

    private Vector3[] adjacentPositions = new Vector3[] { new Vector3(1, 0), new Vector3(0, 1), new Vector3(-1, 0), new Vector3(0, -1)};

    public GameObject grid;
    private Tilemap collisionTilemap;
    public Tile blankTile;

    void Awake()
    {
        InitializeEntityScript();
        toggleableScript = GetComponent<ToggleableScript>();
        boxCollider = GetComponent<BoxCollider2D>();
        entityController = GameObject.Find("EntityController").GetComponent<EntityController>();
        terrainController = GameObject.Find("TerrainController").GetComponent<TerrainController>();
        player = GameObject.Find("Player");
        InitializeWheelableScript();
    }

    private void InitializeWheelableScript()
    {
        wheelableScript = GetComponent<WheelableScript>();
        wheelableScript.ToggleMovingState = ToggleMovingState;
    }

    private void InitializeEntityScript()
    {
        entityScript = GetComponent<EntityScript>();
        entityScript.step = Step;
    }

    public void ToggleMovingState()
    {
        boxCollider.enabled = !boxCollider.enabled;
    }

    private List<EntityController.Entity> LookForFriends()
    {
        //does some kind of flood fill algorithm
        List<EntityController.Entity> friends = new List<EntityController.Entity>();

        HashSet<Vector3> searchedPositions = new HashSet<Vector3>() { transform.position };
        Queue<Vector3> positionsToSearchFrom = new Queue<Vector3>();
        positionsToSearchFrom.Enqueue(transform.position);

        Vector3 positionToSearchFrom;
        Vector3 adjacentPosition;
        EntityScript friendEntityScript;
        GameObject obj;

        while (positionsToSearchFrom.Count > 0)
        {
            positionToSearchFrom = positionsToSearchFrom.Dequeue();
            foreach (Vector3 offset in adjacentPositions)
            {
                adjacentPosition = offset + positionToSearchFrom;
                if (!searchedPositions.Contains(adjacentPosition))
                {
                    searchedPositions.Add(adjacentPosition);
                    obj = entityController.GetObjectAtPosition(adjacentPosition);

                    if (obj != null && (friendEntityScript = obj.GetComponent<EntityScript>()) != null && obj.GetComponent<WheelableScript>() != null)
                    {
                        friends.Add(friendEntityScript.selfEntity);
                        positionsToSearchFrom.Enqueue(adjacentPosition);
                        //Debug.LogFormat("added friend {0} at position {1}", obj.name, adjacentPosition);
                    }
                }
            }
        }

        return friends;
    }

    //Step function returns whether is moves or not during the step.
    public EntityController.EntityStepData Step()
    {
        //wheel
        moving = toggleableScript.state;
        if (moving && wheelableScript.disbanded)
        {
            List<EntityController.Entity> friends = LookForFriends();
            friends.Add(entityScript.selfEntity);
                
            Debug.Log("initiating car");

            Vector3 position = transform.position;
            GameObject gridObject = entityController.AddEntity(position.x, position.y, "car", false, friends).obj;
            if (gridObject == null)
            {
                Debug.Log("car not loaded");
            }

            toggleableScript.Toggle();
                

            Debug.Log("finished initiating car");
        }

        //will move whenever active
        return new EntityController.EntityStepData(moving, false);
    }
}
