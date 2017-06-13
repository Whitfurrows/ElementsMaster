﻿using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// component to be attached to the fireball instance
/// </summary>
[RequireComponent(typeof (Controller2D))]
public class TheFireball : Projectile {

    [SerializeField]
    private Buffed buff;

    private Knockback knockback;
    private int dirX;
    private float speed;

    private Damage damage;

    private bool isBuffed;

    [SerializeField]
    private ParticleSystem particles;
    [SerializeField]
    private ParticleSystem particlesExplosion;

    private Controller2D controller;
    private BoxCollider2D collider;

    private ParticleSystem.MinMaxGradient defaultGradient;

    /// <summary>
    /// initialize the component, ensuring that the reuse of it (from the pool) won't be affected by the previous lifetime
    /// </summary>
    /// <param name="dirX">horizontal direction</param>
    /// <param name="spawnPosition"></param>
    /// <param name="speed"></param>
    /// <param name="size"></param>
    /// <param name="maxDuration"></param>

    private void Awake () {
        defaultGradient = particles.colorOverLifetime.color;
    }

    public void Initialize ( int dirX , Vector2 spawnPosition , ref float speed , ref Vector2 size , ref Damage damage , ref Knockback knockback , ref float maxDuration ) {

        ResetBuff ();
        
        gameObject.SetActive (true);

        this.dirX = dirX;

        transform.position = spawnPosition;

        this.speed = speed * dirX;

        transform.localScale = new Vector3 (size.x , size.y , 1);

        controller = GetComponent<Controller2D> ();
        collider = controller.collider;

        this.damage = damage;
        this.knockback = knockback;

        ParticleSystem.ColorOverLifetimeModule colorModule = particles.colorOverLifetime;
        colorModule.color = defaultGradient;

        StartCoroutine (Die (maxDuration)); //TODO: projectiles object pool for memory fragmentation
    }

    private void Update () {
        controller.Move (Vector2.right * ( speed * Time.deltaTime ));
    }

    private void OnController2DTrigger ( Collider2D col ) {
        if (collider.enabled) {
            if (col.GetComponent<TheFireWall> () != null) {
                TheFireWall theFireWall = col.GetComponent<TheFireWall> ();
                Buff ();
            } else if (col.CompareTag (MyTags.enemy.ToString ())) {
                damage.DealDamage (col);
                knockback.Push (col, dirX);
                Explode ();
            } else if (col.CompareTag (MyTags.block.ToString ())) {
                ColliderDistance2D colDist = col.Distance (collider);
                Vector3 dist = ( colDist.pointB - colDist.pointA );
                transform.position = transform.position - dist;
                Explode ();
            }
        }
    }

    private void ResetBuff () {
        isBuffed = false;
        particles.transform.localScale = Vector3.one;
        
    }

    private void Buff () {
        if (!isBuffed) {
            isBuffed = true;

            particles.transform.localScale = Vector3.one * buff.size;

            ParticleSystem.ColorOverLifetimeModule colorModule = particles.colorOverLifetime;
            ParticleSystem.MinMaxGradient gradient = new ParticleSystem.MinMaxGradient (buff.gradientMin , buff.gradientMax);
            colorModule.color = gradient;
        }
    }

    private void Explode () {
        collider.enabled = false; 
        particles.Stop ();
        
        particlesExplosion.gameObject.SetActive (true);
        StartCoroutine (Die (particlesExplosion.main.startLifetime.constantMax));
    }

    /// <summary>
    /// works like the unityengine.destroy method, but deactivating it for the pool
    /// </summary>
    /// <param name="time">time after which the object will die</param>
    /// <returns></returns>
    private IEnumerator Die (float time) {
        while (time > 0) {
            time -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate ();
        }
        collider.enabled = true;
        particlesExplosion.gameObject.SetActive (false);
        gameObject.SetActive (false);
    }

    [Serializable]
    private struct Buffed {
        public float size;
        public Color gradientMin;
        public Color gradientMax;
    }
}
