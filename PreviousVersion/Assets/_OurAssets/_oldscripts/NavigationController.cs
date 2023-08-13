using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationController : MonoBehaviour
{
    public Transform destination = null; // Ŀ�ĵ�λ��

    public TrailRenderer Trail;

    private NavMeshAgent agent;

    private Vector3 lastDestinationPosition;

    public Transform PlayerCapsule;

    public Vector3 warpPosition;

    private Vector3 lastPosition;

    IEnumerator CheckPositionChange()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // ���PlayerCapsuleλ���Ƿ����˸ı�
            if (PlayerCapsule.position != lastPosition)
            {
               
                agent.Warp(PlayerCapsule.position + PlayerCapsule.transform.forward * 2f + new Vector3(0, 0.8f, 0));
                // �����µ�Ŀ�ĵ�λ��
                agent.SetDestination(destination.position);

                // ������һ֡��λ��
                lastPosition = PlayerCapsule.position;
                Trail.Clear();
            }
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        SetDestination();

        lastDestinationPosition = destination.position;

        TrailRenderer Trail = GetComponent<TrailRenderer>();
        StartCoroutine(CheckPositionChange());
    }
    void WarpToPosition(Vector3 position)
    {
        // ʹ�� Warp ���������崫�͵�ָ��λ��
        agent.Warp(position);
    }
    void Update()
    {
        // ʵʱ����Ŀ�ĵ�λ��
        SetDestination();
    }

    void SetDestination()
    {
        if (destination != null && destination.position != lastDestinationPosition)
        {

            // Ŀ�ĵ�λ�÷����˱仯����������Ŀ�ĵ�

            agent.Warp(PlayerCapsule.position + PlayerCapsule.transform.forward * 2f + new Vector3(0, 0.8f, 0));

            // ���֮ǰ�ѻ��Ƶ�·��

            Trail.Clear();

            agent.SetDestination(destination.position);

            // ������һ֡��Ŀ�ĵ�λ��
            lastDestinationPosition = destination.position;
        }
    }
}