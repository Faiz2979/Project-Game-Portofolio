using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    [SerializeField] List<GameObject> enemies;

    [Header("Detection Settings")]
    [SerializeField] float detectionRadius = 5f;
    [SerializeField] float debugSphere = 0.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        DetectEnemies();

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= detectionRadius)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                    Gizmos.DrawSphere(enemy.transform.position, debugSphere);
                }
            }
        }
    }


void DetectEnemies()
{
    RaycastHit[] hits = Physics.SphereCastAll(transform.position, detectionRadius, Vector3.up);

    foreach (RaycastHit hit in hits)
    {
        Enemy enemyComponent = hit.collider.GetComponent<Enemy>();
        GameObject enemyObj = hit.collider.gameObject;

        if (enemyComponent != null && !enemies.Contains(enemyObj))
        {
            enemies.Add(enemyObj);
            Debug.Log("Detected new enemy: " + enemyObj.name);
        }
    }
}


    
}
