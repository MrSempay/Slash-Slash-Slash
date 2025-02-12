using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bee : Unit
{
    private NavMeshAgent agent;
    [SerializeField] private Player player;
    private Transform playerTransform;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = player.GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();  

    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(new Vector3(playerTransform.position.x, playerTransform.position.y, 0));
        transform.rotation = Quaternion.Euler(0, 0, 0); // Устанавливаем вращение в 0 градусов
    }
}
