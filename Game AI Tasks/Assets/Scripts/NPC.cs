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
    int nextPatrolPoint;
    NPCStates currentState = NPCStates.Patrol;
    NavMeshAgent navMeshAgent;
    MeshRenderer meshRenderer;
    float nextShootTime = 0;

    public Transform[] waypoints;
    int waypointsIndex;
    private int farthestPatrolPointIndex;
    Vector3 target;
    private GameObject player;
    private float retreatDistance = 10.0f;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        //navMeshAgent.SetDestination(PatrolPoints[nextPatrolPoint]);
        meshRenderer = GetComponent<MeshRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");
        UpdateDestination();
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
            currentState = NPCStates.Retreat;
        }
    }

    private void Patrol()
    {
        // Increment patrol point counter and wrap around if necessary
        // Set the next patrol point as the new destination

        // Your code
        // Switch to patrol material and start moving
        meshRenderer.material = PatrolMaterial;
        navMeshAgent.isStopped = false;

        // Check if NPC has reached the current patrol point
        if (Vector3.Distance(transform.position, target) < 1)
        {
            IterateWayPointIndex();
            UpdateDestination();
        }

        // Switch to chase state if player is within chase range
        if (Vector3.Distance(Player.position, transform.position) <= ChaseRange)
        {
            currentState = NPCStates.Chase;
        }
    }

    void UpdateDestination()
    {
        target = waypoints[waypointsIndex].position;
        navMeshAgent.SetDestination(target);
    }

    void IterateWayPointIndex()
    {
        waypointsIndex++;
        if (waypointsIndex == waypoints.Length)
        {
            waypointsIndex = 0;
        }
    }

    private void Retreat()
    {
        // Your code
        // Switch to retreat material
        meshRenderer.material = RetreatMaterial;

        if (Vector3.Distance(transform.position, player.transform.position) > retreatDistance) // if player is far enough away
        {
            navMeshAgent.SetDestination(waypoints[waypointsIndex].position); // move to current patrol point
        }
        else // if player is too close
        {
            // calculate the farthest patrol point from the player
            float farthestDistance = 0.0f;
            for (int i = 0; i < waypoints.Length; i++)
            {
                float distance = Vector3.Distance(waypoints[i].position, player.transform.position);
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestPatrolPointIndex = i;
                }
            }

            navMeshAgent.SetDestination(waypoints[farthestPatrolPointIndex].position); // move to farthest patrol point
            currentState = NPCStates.Patrol;
        }

        if (Vector3.Distance(Player.position, transform.position) <= ChaseRange)
        {
            currentState = NPCStates.Chase;
            Debug.Log("about to chase again");
        }
    }
}