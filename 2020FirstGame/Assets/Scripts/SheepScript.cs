using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepScript : MonoBehaviour
{
    private Rigidbody2D myRigidbody2D;

    int counter = 0;
    public int speed;
    int velocity;
    int seed;

    Vector2 direction;

    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        direction = new Vector2(UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));
    }

    private void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        seed = UnityEngine.Random.Range(600, 1050);
    }

    public bool Step()
    {
        //do whatever we do for one step

        //Next Goal: find destination, then move towards it a little bit

        //Current Function: wiggle back and forth I guess
        bool moved = false;

        counter++;
        if (counter > seed)
        {
            direction = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
            counter = 0;
            //Debug.Log(direction);
        } 
        else if (counter > seed/2)
        {
            velocity = speed;
            moved = true;
        } 
        else
        {
            velocity = 0;
        }//*/

        return moved;
    }

    private void FixedUpdate()
    {
        myRigidbody2D.AddForce(velocity * direction);
    }

}
