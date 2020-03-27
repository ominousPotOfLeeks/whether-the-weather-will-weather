using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepScript : MonoBehaviour
{
    private Rigidbody2D myRigidbody2D;
    private EntityScript myEntityScript;

    int counter = 0;
    public int speed;
    int velocity;
    int seed;
    float seedMultiplier = 1;

    Vector2 direction;

    public EntityController.Entity selfEntity;

    private void Awake()
    {
        InitializeEntityScript();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        seed = UnityEngine.Random.Range(600, 1050);
        ChangeDirection();
    }
    private void InitializeEntityScript()
    {
        myEntityScript = GetComponent<EntityScript>();
        myEntityScript.step = Step;
    }

    private void ChangeDirection()
    {
        direction = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
        counter = 0;
    }

    private void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
        counter = 0;
    }

    public EntityController.EntityStepData Step()
    {
        //do whatever we do for one step

        //Next Goal: find destination, then move towards it a little bit

        //Current Function: wiggle back and forth I guess
        bool moved = false;

        counter++;
        if (counter > seed)
        {
            ChangeDirection();
            seedMultiplier = 1;
            //Debug.Log(direction);
        } 
        else if (counter > seed * seedMultiplier / 3)
        {
            velocity = 0;
        } 
        else
        {
            velocity = speed;
            moved = true;
        }//*/

        return new EntityController.EntityStepData(moved, false);
    }

    private void FixedUpdate()
    {
        myRigidbody2D.AddForce(velocity * direction);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (counter > seed * seedMultiplier / 3 && collision.relativeVelocity.magnitude != 0)
        {
            SetDirection((1 / collision.relativeVelocity.magnitude) * collision.relativeVelocity);
            seedMultiplier = 0.1f;
        }
        
    }
}
