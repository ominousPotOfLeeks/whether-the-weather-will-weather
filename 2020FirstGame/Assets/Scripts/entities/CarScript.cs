using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarScript : MonoBehaviour
{

    private EntityController entityController;
    private EntityScript entityScript;

    public GameObject tilemapObject;
    public Tilemap collisionTilemap;
    public Rigidbody2D tilemapRigidbody2D;
    public ToggleableScript toggleableScript;

    public Tile blankTile;

    public float totalWheelPower;//force with which tilemap is pushed
    public float totalMass;
    public float wheelSpeed;

    public List<EntityController.Entity> friends = new List<EntityController.Entity>();

    private void Awake()
    {
        InitializeEntityScript();
        entityController = GameObject.Find("EntityController").GetComponent<EntityController>();
    }

    public void Initialize()
    {
        foreach (var entity in entityScript.selfEntity.childEntities)
        {
            friends.Add(entity);
        }
        tilemapObject.GetComponent<TilemapRenderer>().enabled = false;
        PrepareFriends();
        entityScript.selfEntity.hasNonStandardPosition = true;
        //tilemapObject.transform.localPosition = new Vector3(0, 0, 0);
        toggleableScript.state = false;
        entityScript.selfEntity.position = tilemapObject.transform.position;
    }

    private void InitializeEntityScript()
    {
        entityScript = GetComponent<EntityScript>();
        entityScript.step = Step;
        entityScript.initialize = Initialize;
        entityScript.unInitialize = UnInitialize;
        entityScript.remove = Remove;
    }

    private void PrepareFriends()
    {
        totalWheelPower = 0;
        totalMass = 0;

        Vector3 position = transform.position;
        Vector3Int roundedPosition = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), 0);

        Vector3 friendPosition;
        Vector3Int roundedFriendPosition;
        WheelableScript friendWheelableScript;
        foreach (EntityController.Entity entity in friends)
        {
            friendWheelableScript = entity.obj.GetComponent<WheelableScript>();
            friendWheelableScript.disbanded = false;
            entity.obj.transform.parent = tilemapObject.transform;
            friendWheelableScript.ToggleMovingState();
            entity.hasParent = true;

            friendPosition = entity.obj.transform.position;
            roundedFriendPosition = new Vector3Int(Mathf.RoundToInt(friendPosition.x), Mathf.RoundToInt(friendPosition.y), 0);
            collisionTilemap.SetTile(roundedFriendPosition - roundedPosition, blankTile);

            totalMass += friendWheelableScript.mass;
            totalWheelPower += friendWheelableScript.power;
            Debug.LogFormat("prepared friend {0}", entity.objName);
        }
    }
    //
    private EntityController.EntityStepData Step()
    {
        if (!toggleableScript.state)
        {
            //move the tilemap parent so all entities in the car move
            if (tilemapRigidbody2D.velocity.magnitude < wheelSpeed)
            {
                tilemapRigidbody2D.mass = totalMass;
                tilemapRigidbody2D.AddForce(new Vector2(totalWheelPower, 0));
            }
            entityScript.selfEntity.position = tilemapObject.transform.position;
        }

        return new EntityController.EntityStepData(true, toggleableScript.state);
    }

    private void UnInitialize()
    {
        WheelableScript friendWheelableScript;
        foreach (EntityController.Entity entity in friends)
        {
            friendWheelableScript = entity.obj.GetComponent<WheelableScript>();
            friendWheelableScript.disbanded = true;
            entity.obj.transform.parent = null;
            friendWheelableScript.ToggleMovingState();
        }
        friends.Clear();
        tilemapRigidbody2D.velocity = Vector2.zero;
        tilemapObject.transform.localPosition = Vector3.zero;
        transform.position = Vector3.zero;
        collisionTilemap.ClearAllTiles();
        Debug.Log("cleared friends");
    }

    private void Remove()
    {
        WheelableScript friendWheelableScript;
        foreach (EntityController.Entity entity in friends)
        {
            friendWheelableScript = entity.obj.GetComponent<WheelableScript>();
            friendWheelableScript.disbanded = true;
            entity.obj.transform.parent = null;
            friendWheelableScript.ToggleMovingState();
        }
        friends.Clear();
        tilemapRigidbody2D.velocity = Vector2.zero;
        tilemapObject.transform.localPosition = Vector3.zero;
        transform.position = Vector3.zero;
        collisionTilemap.ClearAllTiles();

        tilemapRigidbody2D.velocity.Set(0, 0);
        //Debug.Log("goodbye yall");
    }
}
