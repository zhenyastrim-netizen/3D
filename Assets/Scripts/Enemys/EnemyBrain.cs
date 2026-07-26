using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("State")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float loseTargetRange = 25f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.8f;

    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private EnemyHealth enemyHealth;

    public EnemyState CurrentState => currentState;
    public Transform Target => target;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead)
            return;

        if (enemyHealth != null && enemyHealth.IsDead)
        {
            SetDead();
            return;
        }

        if (target == null)
        {
            FindPlayer();
            currentState = EnemyState.Idle;
            return;
        }

        UpdateState();
    }

    private void UpdateState()
    {
        Vector3 enemyPosition = transform.position;
Vector3 targetPosition = target.position;

enemyPosition.y = 0f;
targetPosition.y = 0f;

float distanceToTarget = Vector3.Distance(
    enemyPosition,
    targetPosition
);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToTarget <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                if (distanceToTarget > loseTargetRange)
                {
                    ChangeState(EnemyState.Idle);
                }
                else if (distanceToTarget <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                break;

            case EnemyState.Attack:
                if (distanceToTarget > attackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        Debug.Log(
            $"{gameObject.name}: состояние → {currentState}"
        );
    }

    public void SetDead()
    {
        ChangeState(EnemyState.Dead);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(
            transform.position,
            loseTargetRange
        );
    }
#endif
}