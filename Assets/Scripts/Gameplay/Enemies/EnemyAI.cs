﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyAI : MonoBehaviour {

    public enum State { Spawning, Idle, Searching, Chasing, Attacking, Dying}
    public State currentState;

    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private float aggroDistance = 20f;

    [SerializeField]
    private float rayDistance = 3f;
    private int raySpacing = 1;
    private float raySkin = 0.1f;

    private Vector2 targetPos;
    private bool isJumping;
    private bool fullLengthJump;

    private Enemy enemy;
    private Movement movement;
    private Player player;
    private Controller2D controller;
    private BoxCollider2D col;

    private void Awake () {
        enemy = GetComponent<Enemy> ();
        movement = GetComponent<Movement> ();
        controller = GetComponent<Controller2D>();

        col = GetComponent<BoxCollider2D> ();
        player = FindObjectOfType<Player> ();
    }

    private void FixedUpdate () {
        targetPos = player.transform.position;
        RunStateBehaviour ();
    }

    private void HandleSpawning () {
        //spawningAnimation
    }

    private void HandleIdle () {
        //idle animation
    }

    private void HandleSearching () {
        //idle animation
            float distance = Vector2.Distance (targetPos , transform.position);

            if (Mathf.Abs (distance) < aggroDistance)  {
                Trigger (State.Chasing);
            }
        //send to pathfinder for a path towards that location
        //wait until it finds something.
    }

    private void HandleChasing () {
        //running animation
        int dirX = 1;
        if (targetPos.x - transform.position.x < 0f) {
            dirX = -1;
        }
        else {
            dirX = 1;
        }
        // test if there's ground ahead
        if (controller.Collisions.below) {
            Vector2 rayOrigin = ( dirX == -1 )
                        ? new Vector2 (transform.position.x - ( col.size.x / 2 ) , transform.position.y + ( col.size.y / 2 ))
                        : new Vector2 (transform.position.x + ( col.size.x / 2 ) , transform.position.y + ( col.size.y / 2 ));
            Vector2 rayDirection = new Vector2 (2 * dirX , -1);
            float groundRayDistance = 10f;
            Debug.DrawRay (rayOrigin , rayDirection * groundRayDistance , Color.red);
            RaycastHit2D groundHit = Physics2D.Raycast (transform.position , rayDirection , groundRayDistance , layerMask);
            if (groundHit && groundHit.normal == Vector2.up) {
                //there's ground ahead. can move
                rayOrigin -= Vector2.up * ( col.size.y / 2 );
                rayDirection = Vector2.right * dirX;
                Debug.DrawRay (rayOrigin , rayDirection , Color.green);
                RaycastHit2D hit = Physics2D.Raycast (rayOrigin , rayDirection , rayDistance , layerMask);
                if (hit) {
                    //something ahead. cast jumping rays
                    if (TryJumping (hit.distance , movement.MaxSpeed.x * dirX)) {
                        fullLengthJump = true;
                    }
                    else if (TryJumping (hit.distance , 0f)) {
                        fullLengthJump = false;
                    }
                }
                movement.SetDirectionalInput (Vector2.right * dirX);
            }
            else {
                //no ground ahead;
                movement.SetDirectionalInput (Vector2.right * -dirX);
            }
        }
        else {
            if (fullLengthJump)
                movement.SetDirectionalInput (Vector2.right * dirX);
            else
                movement.SetDirectionalInput (Vector2.zero);
        }
    }

    private bool TryJumping (float wallDistance, float targetVelocityX) {
        //se wall distance < (distancia minima para conseguir um pulo sem esbarrar na parede), pule baixo e desacelerando
        if (movement.Velocity.x == 0 && targetVelocityX == 0) {
            return false;
        }

        bool doJump = false;

        Vector2 origin = transform.position;
        Vector2 startingVelocity = new Vector2 (movement.Velocity.x , movement.MaxSpeed.y);
        Vector2 currentVelocity = startingVelocity;

        float smoothVelocityX = 0f;

        for (int rayCount = 0 ; rayCount < 100 ; rayCount++) {
            float acceleration = ( rayCount == 0 ) ? movement.AccelerationTimeGrounded : movement.AccelerationTimeAirBorne;

            if (targetVelocityX != startingVelocity.x) {
                currentVelocity.x = Mathf.SmoothDamp (currentVelocity.x , targetVelocityX , ref smoothVelocityX , acceleration);
            }
            currentVelocity.y += Movement.Gravity * Time.fixedDeltaTime;

            Vector2 direction = currentVelocity.normalized;
            float distance = currentVelocity.magnitude * Time.fixedDeltaTime;
            
            RaycastHit2D hit = Physics2D.Raycast (origin , direction , distance , layerMask);
            Debug.DrawRay (origin , direction * distance , Color.gray, 2f);
            if (hit) {
                //testar se ele acertou subindo ou descendo.
                if (hit.normal == Vector2.up) {
                    doJump = true;
                    break;
                }
            }

            origin = origin + direction * distance;
        }

        if (doJump) {
            movement.HandleJump ();
        }

        return doJump;
    }

    private void HandleAttacking () {
        //attacking animation

        //this happens if the distance to the player < attack range
        //does the animation
        //if the attack hits (collider?)
        //deal damage
    }

    private void HandleDying () {
        //dying animation
        //should take the collider away in the first fra;me and deactivate any kind of outside influence

        //unsubscribes from events (on disable could do this)
        //
    }

    public void Trigger (State newState ) {
        if (newState != currentState) {
            switch (currentState) {
                case State.Spawning:
                    break;
                case State.Idle:
                    break;
                case State.Searching:
                    break;
                case State.Chasing:
                    break;
                case State.Attacking:
                    break;
                case State.Dying:
                    break;
                default:
                    break;
            }
            currentState = newState;
        }
    }

    private void RunStateBehaviour () {
        switch (currentState) {
            case State.Spawning:
                HandleSpawning ();
                break;
            case State.Idle:
                HandleIdle ();
                break;
            case State.Searching:
                HandleSearching ();
                break;
            case State.Chasing:
                HandleChasing ();
                break;
            case State.Attacking:
                HandleAttacking ();
                break;
            case State.Dying:
                HandleDying ();
                break;
            default:
                break;
        }
    }

}
