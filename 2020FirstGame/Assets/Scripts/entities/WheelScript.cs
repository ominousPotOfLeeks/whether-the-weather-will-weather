using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelScript : MonoBehaviour
{
    private EntityController entityController;
    private EntityScript entityScript;
    private ToggleableScript toggleableScript;
    private WheelableScript wheelableScript;

    private BoxCollider2D boxCollider;

    public int wheelSpeed = 5;

    private Vector3[] adjacentPositions = new Vector3[] { new Vector3(1, 0), new Vector3(0, 1), new Vector3(-1, 0), new Vector3(0, -1)};
    private List<EntityController.Entity> friends = new List<EntityController.Entity>();

    private bool doneLookingForFriends = false;

    void Awake()
    {
        InitializeEntityScript();
        InitializeWheelableScript();
        toggleableScript = GetComponent<ToggleableScript>();
        boxCollider = GetComponent<BoxCollider2D>();
        entityController = GameObject.Find("EntityController").GetComponent<EntityController>();
    }

    private void InitializeWheelableScript()
    {
        wheelableScript = GetComponent<WheelableScript>();
        wheelableScript.parentEntity = entityScript.selfEntity;//set parent to self until part of a group
    }

    private void InitializeEntityScript()
    {
        entityScript = GetComponent<EntityScript>();
        entityScript.step = Step;
    }

    private void LookForFriends()
    {
        doneLookingForFriends = true;
        Vector3 position = transform.position;

        Vector3 adjacentPosition;
        foreach (Vector3 offset in adjacentPositions)
        {
            adjacentPosition = offset + position;
            GameObject obj = entityController.GetObjectAtPosition(adjacentPosition);
            if (obj != null)
            {
                EntityScript entityScript;
                if ((entityScript = obj.GetComponent<EntityScript>()) != null)
                {
                    friends.Add(entityScript.selfEntity);
                    Debug.LogFormat("added friend {0} at position {1}", obj.name, adjacentPosition);
                }
            }
        }
    }

    //Step function returns whether is moves or not during the step.
    public bool Step()
    {
        //wheel

        if (toggleableScript.state)
        {
            if (!doneLookingForFriends)
            {
                Debug.Log("wheeling");
                LookForFriends();

                
            }
        } else
        {
            doneLookingForFriends = false;
        }

        //will move whenever active
        return toggleableScript.state;
    }
}
