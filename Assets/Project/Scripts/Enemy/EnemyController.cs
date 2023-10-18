using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Vector2 destinationArea;
    public float waitToPatrol;
    public float distancePatrol2Chase;
    public float distanceChase2Attack;
    public float waitToChase;
    public float waitToAttack;
    private Animator enemyController;
    [SerializeField]
    private NavMeshAgent agent;
    private GameObject[] playerTransforms;
    private Transform playerTransform;
    private EnemyState currentState;
    private Vector3 patrolDestination = Vector3.zero;
    private float sqDistance2Player;
    void Start()
    {
        enemyController = GetComponent<Animator>();
        //agent = GetComponent<NavMeshAgent>();
        //playerTransform = GameObject.Find("Player").transform;
        playerTransforms = GameObject.FindGameObjectsWithTag("Player");
        if (playerTransforms.Length > 0) { 
            foreach (GameObject player in playerTransforms)
            {
                if (player.activeInHierarchy)
                {
                    Debug.Log("Enemy follow " + player.name);
                    playerTransform = player.transform;
                    break;
                }
            }
        }
        else
            playerTransform = GameObject.FindWithTag("Player").transform;
        currentState = EnemyState.NONE;
        ChangeState(EnemyState.PATROL);
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;
        switch (newState)
        {
            case EnemyState.PATROL:
                if (currentState == EnemyState.CHASE)
                {
                    StopCoroutine("UpdatePlayerDestination");
                }
                currentState = newState;
                StartCoroutine("GeneratePatrolDestination");
                break;
            case EnemyState.CHASE:
                if (currentState == EnemyState.PATROL)
                    StopCoroutine("GeneratePatrolDestination");
                else if (currentState == EnemyState.ATTACK)
                    StopCoroutine("UpdateAttack");
                currentState = newState;
                StartCoroutine("UpdatePlayerDestination");
                break;
            case EnemyState.ATTACK:
                if (currentState == EnemyState.CHASE)
                {
                    StopCoroutine("UpdatePlayerDestination");
                    currentState = newState;
                    StartCoroutine("UpdateAttack");
                }
                break;
            case EnemyState.DEATH:
                if (currentState == EnemyState.PATROL)
                    StopCoroutine("GeneratePatrolDestination");
                StopAllCoroutines();
                enemyController.SetTrigger("Death");
                currentState = newState;
                break;
        }
    }

    public IEnumerator GeneratePatrolDestination()
    {
        while (true)
        {
            patrolDestination = transform.position
                                + new Vector3(Random.Range(-destinationArea.x, destinationArea.y),
                                              0,
                                              Random.Range(-destinationArea.y, destinationArea.y));
            agent.SetDestination(patrolDestination);
            yield return new WaitForSecondsRealtime(waitToPatrol);
        }
    }
    public IEnumerator UpdatePlayerDestination()
    {
        while (true)
        {
            agent.SetDestination(playerTransform.position);
            yield return new WaitForSecondsRealtime(waitToChase);
        }
    }
    public IEnumerator UpdateAttack()
    {
        while (true)
        {
            enemyController.SetTrigger("Attack");
            yield return new WaitForSecondsRealtime(waitToAttack);
        }
    }

    public void ChangeStateToDeath()
    {
        ChangeState(EnemyState.DEATH);
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.PATROL:
                sqDistance2Player = (playerTransform.position - transform.position).sqrMagnitude;
                if (sqDistance2Player <= distancePatrol2Chase * distancePatrol2Chase)
                    ChangeState(EnemyState.CHASE);
                break;
            case EnemyState.CHASE:
                sqDistance2Player = (playerTransform.position - transform.position).sqrMagnitude;
                if (sqDistance2Player <= distanceChase2Attack * distanceChase2Attack)
                    ChangeState(EnemyState.ATTACK);
                else if (sqDistance2Player > distancePatrol2Chase * distancePatrol2Chase)
                    ChangeState(EnemyState.PATROL);
                break;
            case EnemyState.ATTACK:
                sqDistance2Player = (playerTransform.position - transform.position).sqrMagnitude;
                if (sqDistance2Player > distanceChase2Attack * distanceChase2Attack)
                    ChangeState(EnemyState.CHASE);
                break;
        }
        enemyController.SetFloat("speed", agent.velocity.sqrMagnitude);
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}

public enum EnemyState
{
    NONE,
    PATROL,
    ATTACK,
    CHASE,
    DEATH
}