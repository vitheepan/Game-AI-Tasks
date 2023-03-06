using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    public enum NPCStates
    {
        Patrol,
        Chase,
        Attack,
        Retreat
    }

    [SerializeField]
    Vector3[] PatrolPoints;
    [SerializeField]
    Transform Player;
    [SerializeField]
    Bullet Bullet;
    [SerializeField]
    Material PatrolMaterial;
    [SerializeField]
    Material ChaseMaterial;
    [SerializeField]
    Material RetreatMaterial;
    [SerializeField]
    Material AttackMaterial;
    [SerializeField]
    float ChaseRange = 7f;
    [SerializeField]
    float AttackRange = 4f;

    float FireRate = 2f;
    int nextPatrolPoint = 0;
    NPCStates currentState = NPCStates.Patrol;
    NavMeshAgent navMeshAgent;
    MeshRenderer meshRenderer;
    float nextShootTime = 0;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.SetDestination(PatrolPoints[nextPatrolPoint]);
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        SwitchState();
    }

    private void SwitchState()
    {
        switch (currentState)
        {
            case NPCStates.Patrol:
                Patrol();
                break;
            case NPCStates.Chase:
                Chase();
                break;
            case NPCStates.Attack:
                Attack();
                break;
            case NPCStates.Retreat:
                Retreat();
                break;
            default:
                Patrol();
                break;
        }
    }

    private void Attack()
    {
        //Your code
        // Check if the player is within attack range
        if (Vector3.Distance(Player.position, transform.position) <= AttackRange)
        {
            // Stop moving and switch to attack material
            navMeshAgent.isStopped = true;
            meshRenderer.material = AttackMaterial;

            // Rotate towards the player
            transform.LookAt(Player.position);

            // Fire bullet if enough time has passed since the last shot
            if (Time.time >= nextShootTime)
            {
                Instantiate(Bullet, transform.position + (transform.forward * 1.5f), transform.rotation);
                nextShootTime = Time.time + 1f / FireRate;
            }
        }
        else
        {
            // Player is out of attack range, so switch back to chase state
            currentState = NPCStates.Chase;
        }
    }

    private void Chase()
    {
        // Your code
        // Switch to chase material
        meshRenderer.material = ChaseMaterial;

        // Set destination to player's position
        navMeshAgent.SetDestination(Player.position);

        // Switch to attack state if within attack range
        if (Vector3.Distance(Player.position, transform.position) <= AttackRange)
        {
            currentState = NPCStates.Attack;
        }
        // Switch back to patrol state if out of chase range
        else if (Vector3.Distance(Player.position, transform.position) > ChaseRange)
        {
            currentState = NPCStates.Patrol;
        }
    }

    private void Patrol()
    {
        // Your code
        // Switch to patrol material and start moving
        meshRenderer.material = PatrolMaterial;
        navMeshAgent.isStopped = false;

        // Check if NPC has reached the current patrol point
        if (Vector3.Distance(transform.position, PatrolPoints[nextPatrolPoint]) < navMeshAgent.stoppingDistance)
        {
            // Increment patrol point counter and wrap around if necessary
            nextPatrolPoint = (nextPatrolPoint + 1) % PatrolPoints.Length;

            // Set the next patrol point as the new destination
            navMeshAgent.SetDestination(PatrolPoints[nextPatrolPoint]);
        }

        // Switch to chase state if player is within chase range
        if (Vector3.Distance(Player.position, transform.position) <= ChaseRange)
        {
            currentState = NPCStates.Chase;
        }
    }

    private void Retreat()
    {
        // Your code
        // Switch to retreat material
        meshRenderer.material = RetreatMaterial;

        // Calculate the patrol point farthest away from the player
        int farthestPatrolPoint = 0;
        float maxDistance = 0;
        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(PatrolPoints[i], Player.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestPatrolPoint = i;
            }
        }

        // Move towards the farthest patrol point
        navMeshAgent.SetDestination(PatrolPoints[farthestPatrolPoint]);

        // Switch back to patrol state if the NPC reaches the destination patrol point
        if (Vector3.Distance(transform.position, PatrolPoints[farthestPatrolPoint]) < navMeshAgent.stoppingDistance)
        {
            currentState = NPCStates.Patrol;
            nextPatrolPoint = farthestPatrolPoint;
        }
        // Switch to a new retreat state if the player moves close to the destination patrol point
        else if (Vector3.Distance(Player.position, PatrolPoints[farthestPatrolPoint]) < ChaseRange)
        {
            currentState = NPCStates.Retreat;
        }
    }

}
