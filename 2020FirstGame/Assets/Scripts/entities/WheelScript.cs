using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WheelScript : MonoBehaviour
{
    private EntityController entityController;
    private EntityScript entityScript;
    private ToggleableScript toggleableScript;
    private WheelableScript wheelableScript;

    private GameObject player;
    private BoxCollider2D boxCollider;

    public float wheelSpeed;

    public bool moving;

    private Vector3[] adjacentPositions = new Vector3[] { new Vector3(1, 0), new Vector3(0, 1), new Vector3(-1, 0), new Vector3(0, -1)};
    private List<EntityController.Entity> friends = new List<EntityController.Entity>();

    private bool doneLookingForFriends = false;

    public GameObject grid;
    private Tilemap collisionTilemap;
    public Tile blankTile;

    void Awake()
    {
        InitializeEntityScript();
        toggleableScript = GetComponent<ToggleableScript>();
        boxCollider = GetComponent<BoxCollider2D>();
        entityController = GameObject.Find("EntityController").GetComponent<EntityController>();
        player = GameObject.Find("Player");
        InitializeWheelableScript();
    }

    private void InitializeWheelableScript()
    {
        wheelableScript = GetComponent<WheelableScript>();
        wheelableScript.parentEntity = entityScript.selfEntity;//set parent to self until part of a group
        wheelableScript.parentToggleableScript = toggleableScript;
        wheelableScript.isParent = true;
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

    private void LookForFriends()
    {
        //does some kind of flood fill algorithm

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
    }

    //Step function returns whether is moves or not during the step.
    public bool Step()
    {
        //wheel
        moving = wheelableScript.parentToggleableScript.state;
        if (wheelableScript.parentToggleableScript.state)
        {
            if (wheelableScript.isParent)
            {
                if (!doneLookingForFriends)
                {
                    LookForFriends();
                    doneLookingForFriends = true;

                    GameObject gridObject = Instantiate(grid, Vector3.zero + new Vector3(0, 0, player.transform.position.z), Quaternion.identity);
                    gridObject.transform.parent = transform;
                    collisionTilemap = gridObject.GetComponentInChildren<Tilemap>();
                    //gridObject.GetComponentInChildren<TilemapRenderer>().enabled = false;
                    
                    Vector3 friendPosition;

                    wheelableScript.ToggleMovingState();
                    Vector3 position = transform.position;
                    Vector3Int roundedPosition = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), 0);
                    collisionTilemap.SetTile(roundedPosition, blankTile);

                    //make self parent of all friends
                    foreach (EntityController.Entity entity in friends)
                    {
                        WheelableScript friendWheelableScript = entity.obj.GetComponent<WheelableScript>();
                        friendWheelableScript.parentEntity = entityScript.selfEntity;//parentToggleableScript
                        friendWheelableScript.parentToggleableScript = toggleableScript;
                        entity.obj.transform.parent = transform;
                        friendWheelableScript.isParent = false;

                        friendWheelableScript.ToggleMovingState();
                        friendPosition = entity.obj.transform.position;
                        Vector3Int roundedFriendPosition = new Vector3Int(Mathf.RoundToInt(friendPosition.x), Mathf.RoundToInt(friendPosition.y), 0);
                        collisionTilemap.SetTile(roundedFriendPosition, blankTile);
                    }

                    
                } else
                {
                    //move
                    transform.position += new Vector3(wheelSpeed, 0);
                }
            }
            
        } else
        {
            doneLookingForFriends = false;
        }

        //will move whenever active
        return toggleableScript.state;
    }
}
