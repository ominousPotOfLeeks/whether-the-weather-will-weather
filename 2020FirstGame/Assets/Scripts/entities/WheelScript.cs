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
    private Rigidbody2D tilemapRigidbody2D;
    private GameObject gridObject;
    private GameObject tilemapObject;

    public int totalWheelPower;//force with which tilemap is pushed
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
        terrainController = GameObject.Find("TerrainController").GetComponent<TerrainController>();
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
        if (moving && wheelableScript.isParent)
        {
            if (!doneLookingForFriends)
            {
                LookForFriends();
                doneLookingForFriends = true;
                Debug.Log("initiating car");


                Vector3 position = transform.position;
                Vector3Int roundedPosition = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), 0);
                Vector3 offset = new Vector3((position.x - roundedPosition.x), (position.y - roundedPosition.y), 0);
                gridObject = Instantiate(grid, Vector3.zero + new Vector3(offset.x, offset.y, player.transform.position.z), Quaternion.identity);
                tilemapObject = gridObject.transform.GetChild(0).gameObject;

                transform.parent = tilemapObject.transform;
                collisionTilemap = tilemapObject.GetComponent<Tilemap>();
                tilemapRigidbody2D = tilemapObject.GetComponent<Rigidbody2D>();
                ToggleableScript tilemapToggleableScript = tilemapObject.GetComponent<ToggleableScript>();
                tilemapToggleableScript.Toggle();
                tilemapObject.GetComponent<TilemapRenderer>().enabled = false;

                
                //Tile tile = terrainController.GetTile(terrainController.GetTileID(name));
                collisionTilemap.SetTile(roundedPosition, blankTile);

                wheelableScript.ToggleMovingState();
                wheelableScript.parentToggleableScript = tilemapToggleableScript;
                tilemapRigidbody2D.mass = wheelableScript.mass;
                totalWheelPower = wheelableScript.power;


                Vector3 friendPosition;
                Vector3Int roundedFriendPosition;
                string friendName;
                string name = entityScript.selfEntity.objName;
                //make self parent of all friends
                foreach (EntityController.Entity entity in friends)
                {
                    WheelableScript friendWheelableScript = entity.obj.GetComponent<WheelableScript>();
                    friendWheelableScript.parentEntity = entityScript.selfEntity;
                    friendWheelableScript.parentToggleableScript = tilemapToggleableScript;
                    entity.obj.transform.parent = tilemapObject.transform;
                    friendWheelableScript.isParent = false;
                    friendWheelableScript.ToggleMovingState();

                    friendPosition = entity.obj.transform.position;
                    roundedFriendPosition = new Vector3Int(Mathf.RoundToInt(friendPosition.x), Mathf.RoundToInt(friendPosition.y), 0);
                    friendName = entity.objName;
                    //tile = terrainController.GetTile(terrainController.GetTileID(friendName));
                    collisionTilemap.SetTile(roundedFriendPosition, blankTile);

                    tilemapRigidbody2D.mass += friendWheelableScript.mass;
                    totalWheelPower += friendWheelableScript.power;
                }

                Debug.Log("finished initiating car");

            } else
            {

                
                //move the tilemap parent so all entities in the car move
                if (tilemapRigidbody2D.velocity.magnitude < wheelSpeed)
                {
                    Debug.Log("moving car");
                    tilemapRigidbody2D.AddForce(new Vector2(totalWheelPower, 0));
                }

                //update position of entities in the chunk dictionary
                foreach (EntityController.Entity friend in friends)
                {
                    entityController.EntityMovedSoUpdateChunk(friend);
                }
                entityController.EntityMovedSoUpdateChunk(entityScript.selfEntity);
            }
        } else
        {
            if (doneLookingForFriends)
            {
                if (!wheelableScript.isParent)
                {
                    Debug.LogError("only parent should have this variable be true");
                }
                else
                {

                    Debug.Log("un-initiating car");
                    //unparent all parts of whole
                    foreach (EntityController.Entity entity in friends)
                    {
                        WheelableScript friendWheelableScript = entity.obj.GetComponent<WheelableScript>();
                        friendWheelableScript.parentEntity = entity;
                        friendWheelableScript.parentToggleableScript = entity.obj.GetComponent<ToggleableScript>();
                        entity.obj.transform.parent = null;
                        friendWheelableScript.isParent = true;
                        friendWheelableScript.ToggleMovingState();
                    }
                    friends.Clear();

                    //unparent self
                    transform.parent = null;
                    wheelableScript.parentToggleableScript = toggleableScript;
                    toggleableScript.Toggle();
                    wheelableScript.ToggleMovingState();

                    tilemapRigidbody2D = null;
                    //remove all tilemap stuff
                    Destroy(gridObject);

                    tilemapObject = null;
                    gridObject = null;
                    doneLookingForFriends = false;
                }
            }
        }

        //will move whenever active
        return moving;
    }
}
