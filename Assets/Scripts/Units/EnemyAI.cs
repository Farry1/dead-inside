using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    EnemyUnit enemyUnit;
    // Start is called before the first frame update
    void Start()
    {
        enemyUnit = GetComponent<EnemyUnit>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
