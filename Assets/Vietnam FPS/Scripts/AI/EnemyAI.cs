
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float sightRange = 25f;
    public float attackRange = 10f;
    public float fireCooldown = 0.8f;
    public float bulletDamage = 10f;

    Transform player;
    NavMeshAgent agent;
    int patrolIndex;
    float nextShot;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        player = playerGO ? playerGO.transform : null;
    }

    void Start()
    {
        GoNextPatrol();
    }

    void Update()
    {
        if (!player) return;
        float d = Vector3.Distance(transform.position, player.position);
        bool canSee = d <= sightRange && HasLineOfSight();

        if (canSee)
        {
            agent.SetDestination(player.position);
            if (d <= attackRange && Time.time >= nextShot)
            {
                nextShot = Time.time + fireCooldown;
                var h = player.GetComponent<Health>();
                if (h) h.Damage(bulletDamage);
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                GoNextPatrol();
        }
    }

    void GoNextPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[patrolIndex].position;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 target = (player.position + Vector3.up * 1.6f) - origin;
        if (Physics.Raycast(origin, target.normalized, out RaycastHit hit, sightRange))
            return hit.collider.CompareTag("Player");
        return false;
    }
}
