using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationController : MonoBehaviour
{
    public Transform destination = null; // 目的地位置

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

            // 检查PlayerCapsule位置是否发生了改变
            if (PlayerCapsule.position != lastPosition)
            {
               
                agent.Warp(PlayerCapsule.position + PlayerCapsule.transform.forward * 2f + new Vector3(0, 0.8f, 0));
                // 设置新的目的地位置
                agent.SetDestination(destination.position);

                // 更新上一帧的位置
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
        // 使用 Warp 方法将物体传送到指定位置
        agent.Warp(position);
    }
    void Update()
    {
        // 实时更新目的地位置
        SetDestination();
    }

    void SetDestination()
    {
        if (destination != null && destination.position != lastDestinationPosition)
        {

            // 目的地位置发生了变化，重新设置目的地

            agent.Warp(PlayerCapsule.position + PlayerCapsule.transform.forward * 2f + new Vector3(0, 0.8f, 0));

            // 清除之前已绘制的路线

            Trail.Clear();

            agent.SetDestination(destination.position);

            // 更新上一帧的目的地位置
            lastDestinationPosition = destination.position;
        }
    }
}