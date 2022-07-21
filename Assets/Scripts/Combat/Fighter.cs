using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Combat
{
  public class Fighter : MonoBehaviour, IAction
  {
    [SerializeField] float weaponRange = 1.2f;
    [SerializeField] float timeBetweenAttacks = 1f;
    [SerializeField] float weaponDamage = 5f;

    Health target;
    float timeSinceLastAttack = Mathf.Infinity;

    private void Update()
    {
      timeSinceLastAttack += Time.deltaTime;

      if (target == null) return;
      if (target.IsDead()) return;

      if (!GetIsInRange())
      {
        GetComponent<Mover>().MoveTo(target.transform.position);
      }
      else
      {
        GetComponent<Mover>().Cancel();
        AttackBehaviour();
      }
    }

    private void AttackBehaviour()
    {
      transform.LookAt(target.transform);
      if (timeSinceLastAttack > timeBetweenAttacks)
      {
        TriggerAttack();
        timeSinceLastAttack = 0;
      }
    }

    private void TriggerAttack()
    {
      GetComponent<Animator>().ResetTrigger("stopAttack");
      // This will trigger the Hit() event.
      GetComponent<Animator>().SetTrigger("attack");
    }

    // Animation Event
    public void Hit()
    {
      if (target == null) return;
      target.TakeDamage(weaponDamage);
    }

    public bool CanAttack(GameObject combatTarget)
    {
      if (combatTarget == null) return false;
      Health targetToTest = combatTarget.GetComponent<Health>();
      return targetToTest != null && !targetToTest.IsDead();
    }

    private bool GetIsInRange()
    {
      return Vector3.Distance(transform.position, target.transform.position) < weaponRange;
    }

    public void Attack(GameObject combatTarget)
    {
      GetComponent<ActionScheduler>().StartAction(this);
      target = combatTarget.GetComponent<Health>();
    }

    public void Cancel()
    {
      TriggerStopAttack();
      target = null;
    }

    private void TriggerStopAttack()
    {
      GetComponent<Animator>().ResetTrigger("attack");
      GetComponent<Animator>().SetTrigger("stopAttack");
    }
  }
}