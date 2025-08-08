using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float debugSphere = 0.5f;
    [SerializeField] private float detectInterval = 0.1f;

    [Space(10)]
    [Header("Detected Enemies")]
    [SerializeField] List<GameObject> enemies = new List<GameObject>();
    public GameObject TargettedEnemy;

    private WaitForSeconds wait;

    private void Start()
    {
        wait = new WaitForSeconds(detectInterval);
        StartCoroutine(DetectEnemiesCoroutine());
    }

    private IEnumerator DetectEnemiesCoroutine()
    {
        while (true)
        {
            DetectEnemies();
            yield return wait;
        }
    }

    private void DetectEnemies()
    {
        enemies.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemies.Add(hit.gameObject);
                Debug.Log("Detected new enemy: " + hit.name);
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        foreach (var enemy in enemies)
        {
            if (enemy == null)
            {
                Gizmos.color = Color.red;
                continue;
            }
            ;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= detectionRadius)
            {
                Vector3 enemyPos = new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, enemyPos);
                Gizmos.DrawWireSphere(enemyPos, debugSphere);
            }
        }
    }

    public void SetTargettedEnemy(GameObject enemy)
    {
        TargettedEnemy = enemy;
    }
}
