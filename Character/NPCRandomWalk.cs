using UnityEngine;
using UnityEngine.AI;

public class NPCRandomWalk : MonoBehaviour
{
    [Header("移动设置")]
    public float walkRadius = 20f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    
    private NavMeshAgent agent;
    private float waitTimer;
    private bool isWaiting;
    private bool initialized = false;

    // 卡住检测
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float STUCK_THRESHOLD = 0.1f; // 移动小于0.1米视为卡住
    private const float STUCK_CHECK_TIME = 3f;   // 3秒检测一次
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // 延迟初始化，等待Agent就位
        Invoke("Initialize", 1f);
    }
    
    void Initialize()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            initialized = true;
            SetRandomDestination();
            Debug.Log($"✅ {gameObject.name} 开始随机行走");
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name} NavMesh Agent未激活，重试");
            Invoke("Initialize", 1f);
        }
    }
    
    void Update()
    {
        if (!initialized || agent == null || !agent.isOnNavMesh) return;

         // 卡住检测
        DetectStuck();        
        
        // 检查是否到达目的地
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                if (!isWaiting)
                {
                    isWaiting = true;
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);
                }
                
                waitTimer -= Time.deltaTime;
                
                if (waitTimer <= 0)
                {
                    SetRandomDestination();
                    isWaiting = false;
                }
            }
        }
    }
    
    /// <summary>
    /// 检测是否卡住
    /// </summary>
    private void DetectStuck()
    {
        // 如果Agent应该在移动但实际没怎么动
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            
            if (distanceMoved < STUCK_THRESHOLD)
            {
                stuckTimer += Time.deltaTime;
                
                if (stuckTimer >= STUCK_CHECK_TIME)
                {
                    Debug.LogWarning($"⚠️ {gameObject.name} 卡住了，重新寻路");
                    ResetStuck();
                }
            }
            else
            {
                stuckTimer = 0f; // 正常移动，重置计时
            }
        }
        
        lastPosition = transform.position;
    }
    
    /// <summary>
    /// 重置卡住状态
    /// </summary>
    private void ResetStuck()
    {
        stuckTimer = 0f;
        agent.ResetPath(); // 清除当前路径
        
        // 立即尝试新目标
        Invoke("SetRandomDestination", 0.5f);
    }
    
    void SetRandomDestination()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        
        // 尝试多次找到有效目标点
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
            {
                // 检查目标点是否可达
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    //Debug.Log($"🎯 {gameObject.name} 新目标: {hit.position}");
                    return;
                }
            }
        }
        
        Debug.LogWarning($"⚠️ {gameObject.name} 10次尝试都未找到有效目标");
    }    
}