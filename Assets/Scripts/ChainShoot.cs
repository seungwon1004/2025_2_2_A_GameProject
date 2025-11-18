using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainShoot : MonoBehaviour
{
    [SerializeField] float refreshRate = 0.1f;
    [SerializeField][Range(1, 10)] int maximunEnemiesInChain = 3;
    [SerializeField] float delayBetweenEachChain = 0.5f;
    [SerializeField] Transform playerFirePoint;
    [SerializeField] EnemyDetector playerEnemyDetector;
    [SerializeField] GameObject linRendererPrefab;

    bool shooting;
    bool shot;
    float counter = 1;
    GameObject currentClosestEnemy;

    List<GameObject> spawnedLineRenderers = new List<GameObject>();
    List<GameObject> enemiesInChain = new List<GameObject>();
    List<GameObject> activeEffects = new List<GameObject>();
    
    IEnumerator ChainReaction(GameObject closestEnemy)
    {
        yield return new WaitForSeconds(delayBetweenEachChain);

        if(counter == maximunEnemiesInChain)
        {
            yield return null;
        }
        else
        {
            if (shooting)
            {
                counter++;
                enemiesInChain.Add(closestEnemy);
                if(!enemiesInChain.Contains(closestEnemy.GetComponent<EnemyDetector>().GetClosestEnemy()))
                {
                    NewLineRenderer(closestEnemy.transform, closestEnemy.GetComponent<EnemyDetector>().GetClosestEnemy().transform);
                    StartCoroutine(ChainReaction(closestEnemy.GetComponent<EnemyDetector>().GetClosestEnemy()));
                }
            }
        }
    }

    IEnumerator UpdateLineRenderer (GameObject lineR, Transform startPos, Transform endPos, bool getClosetEnemyToPlayer = false)
    {
        lineR.GetComponent<LineRendererController>().SetPosition(startPos, endPos);
        yield return new WaitForSeconds(refreshRate);

        if (getClosetEnemyToPlayer)
        {
            StartCoroutine(UpdateLineRenderer(lineR, startPos, playerEnemyDetector.GetClosestEnemy().transform, true));
            if(currentClosestEnemy != playerEnemyDetector.GetClosestEnemy())
            {
                StopShooting();
                StartShooting();
            }
        }
        else
        {
            StartCoroutine(UpdateLineRenderer(lineR, startPos, endPos));
        }
    }

    void NewLineRenderer(Transform startPos, Transform endPos, bool getClosestEnemyToPlayer = false)
    {
        GameObject lineR = Instantiate(linRendererPrefab);
        spawnedLineRenderers.Add(lineR);
        StartCoroutine(UpdateLineRenderer(lineR, startPos, endPos, getClosestEnemyToPlayer));
    }

    void StartShooting()
    {
        shooting = true;
        
        if(playerEnemyDetector != null && playerFirePoint != null && linRendererPrefab != null)
        {
            if(!shot)
            {
                shot = true;
                currentClosestEnemy = playerEnemyDetector.GetClosestEnemy();
                NewLineRenderer(playerFirePoint, playerEnemyDetector.GetClosestEnemy().transform, true);

                if(maximunEnemiesInChain > 1)
                {
                    StartCoroutine(ChainReaction(playerEnemyDetector.GetClosestEnemy()));
                }
            }
        }
    }

    void StopShooting()
    {
        shooting = false;
        shot = false;

        for (int i = 0; i < spawnedLineRenderers.Count; i++)
        {
            Destroy(spawnedLineRenderers[i]);
        }

        spawnedLineRenderers.Clear();
        enemiesInChain.Clear();

        for(int i = 0; i< activeEffects.Count; i++)
        {
            Destroy(activeEffects[i]);
        }

        activeEffects.Clear();
    }

    private void Update()
    {
        if(Input.GetButton("Fire1"))
        {
            if(playerEnemyDetector.GetEnemiesInRange().Count > 0)
            {
                if(!shooting)
                {
                    StartShooting();
                }
            }
            else
            {
                StopShooting();
            }
        }

        if(Input.GetButtonUp("Fire1"))
        {
            StopShooting();
        }
    }
}
