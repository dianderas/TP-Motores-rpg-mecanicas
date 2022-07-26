using System;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{
  public class AIController : MonoBehaviour
  {
    [SerializeField] float chaseDistance = 5f;
    [SerializeField] float suspicionTime = 3f;
    [SerializeField] PatrolPath patrolPath;
    [SerializeField] float waypointTolerance = 1f;
    [SerializeField] float waypointDwellTime = 3f;

    Fighter fighter;
    Health health;
    GameObject player;
    Mover mover;

    Vector3 guardPosition;
    float timeSinceLastSawPlayer = Mathf.Infinity;
    float timeSinceArrivedWaypoint = Mathf.Infinity;
    int currentWaypointIndex = 0;

    private void Start()
    {
      fighter = GetComponent<Fighter>();
      health = GetComponent<Health>();
      mover = GetComponent<Mover>();

      player = GameObject.FindWithTag("Player");

      guardPosition = transform.position;
    }

    private void Update()
    {
      if (health.IsDead()) return;

      if (InAttackRangeOfPlayer() && fighter.CanAttack(player))
      {
        AttackBehaviour();
      }
      else if (timeSinceLastSawPlayer < suspicionTime)
      {
        SuspicionBehaviour();
      }
      else
      {
        PatrolBehaviour();
      }

      UpdateTimers();
    }

    private void UpdateTimers()
    {
      timeSinceLastSawPlayer += Time.deltaTime;
      timeSinceArrivedWaypoint += Time.deltaTime;
    }

    private void PatrolBehaviour()
    {
      Vector3 nextPosition = guardPosition;
      if (patrolPath != null)
      {
        if (AtWaypoint())
        {
          timeSinceArrivedWaypoint = 0;
          CycleWaypoint();
        }
        nextPosition = GetCurrentWaypoint();
      }
      if (timeSinceArrivedWaypoint > waypointDwellTime)
      {
        mover.StartMoveAction(nextPosition);
      }
    }

    private bool AtWaypoint()
    {
      float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
      return distanceToWaypoint < waypointTolerance;
    }

    private void CycleWaypoint()
    {
      currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
    }

    private Vector3 GetCurrentWaypoint()
    {
      return patrolPath.GetWaypoint(currentWaypointIndex);
    }

    private void SuspicionBehaviour()
    {
      GetComponent<ActionScheduler>().CancelCurrentAction();
    }

    private void AttackBehaviour()
    {
      timeSinceLastSawPlayer = 0;
      fighter.Attack(player);
    }

    private bool InAttackRangeOfPlayer()
    {
      float distanceToplayer = Vector3.Distance(player.transform.position, transform.position);
      return distanceToplayer < chaseDistance;
    }

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }
  }
}