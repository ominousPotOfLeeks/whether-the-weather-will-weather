using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepScript : MonoBehaviour
{
    private Rigidbody2D myRigidbody2D;

    int counter = 0;
    int direction = 1;
    public int speed;
    int velocity;

    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        velocity = direction * speed;
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Step()
    {
        //do whatever we do for one step

        //Next Goal: find destination, then move towards it a little bit

        //Current Function: wiggle back and forth I guess

        counter++;
        if (counter > 200)
        {
            direction *= -1;
            velocity = direction * speed;
            counter = 0;
            Debug.Log("switch direction");
        } 
        else if (counter > 100)
        {
            velocity = direction * speed;
        } 
        else
        {
            velocity = 0;
        }

    }

    private void FixedUpdate()
    {
        myRigidbody2D.AddForce(new Vector2(velocity, velocity));
    }

}
