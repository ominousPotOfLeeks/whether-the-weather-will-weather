using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarScript : MonoBehaviour
{

    private EntityController entityController;
    private EntityScript entityScript;
    private ToggleableScript toggleableScript;
    private Rigidbody2D tilemapRigidbody2D;
    private GameObject tilemapObject;
    private Tilemap collisionTilemap;

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
        friends = entityScript.selfEntity.childEntities;
        AssignComponents();
        tilemapObject.GetComponent<TilemapRenderer>().enabled = false;
        PrepareFriends();
        entityScript.selfEntity.hasNonStandardPosition = true;
        entityScript.selfEntity.position = tilemapObject.transform.position;
    }

    private void InitializeEntityScript()
    {
        entityScript = GetComponent<EntityScript>();
        entityScript.step = Step;
        entityScript.initialize = Initialize;
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
        }
    }

    private void AssignComponents()
    {
        tilemapObject = transform.GetChild(0).gameObject;
        collisionTilemap = tilemapObject.GetComponent<Tilemap>();
        tilemapRigidbody2D = tilemapObject.GetComponent<Rigidbody2D>();
        toggleableScript = tilemapObject.GetComponent<ToggleableScript>();
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

        //Debug.Log("goodbye yall");
    }
}
