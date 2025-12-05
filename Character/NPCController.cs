using UnityEngine;
using UnityEngine.AI;
using GameSystems;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    public CharacterRuntimeData characterData;
    
    [Header("行为系统")]
    public DailySchedule dailySchedule;
    public NPCBehaviorState currentBehavior = NPCBehaviorState.Idle;
    public NPCBehaviorState suggestedBehavior = NPCBehaviorState.Idle;
    public Animator animator;

    private NavMeshAgent agent;
    private TextMesh nameLabel;    
    
    // 动画参数名
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int SpeedXHash = Animator.StringToHash("SpeedX");
    private static readonly int SpeedZHash = Animator.StringToHash("SpeedZ");
    
    // 行为状态动画参数（根据你的动画控制器调整）
    private static readonly int IsSleepingHash = Animator.StringToHash("IsSleeping");
    private static readonly int IsWorkingHash = Animator.StringToHash("IsWorking");
    private static readonly int IsRestingHash = Animator.StringToHash("IsResting");
    private static readonly int IsEatingHash = Animator.StringToHash("IsEating");    
    private static readonly int RestVariantHash = Animator.StringToHash("RestVariant");

    public void Initialize(CharacterRuntimeData data)
    {
        characterData = data;
        agent = GetComponent<NavMeshAgent>();
        
        // 获取子对象的Animator
        animator = GetComponentInChildren<Animator>();
        
        // 设置名字标签
        nameLabel = GetComponentInChildren<TextMesh>();
        if (nameLabel != null)
        {
            nameLabel.text = string.Empty;
        }
        
        // 根据年龄调整速度
        float speedMultiplier = data.age < 16 ? 0.7f : 
                               data.age > 60 ? 0.5f : 1f;
        agent.speed = 2.5f * speedMultiplier;
        
        // 重要：确保Agent不会陷入地面
        agent.baseOffset = data.age < 16 ? 0.3f : 0f;
        
        // 设置位置
        transform.position = data.position;
        
        // 创建默认日程表
        if (dailySchedule == null)
        {
            dailySchedule = DailySchedule.CreateSimpleTestSchedule();
        }
        
        // 注册到调度管理器
        if (NPCScheduleManager.Instance != null)
        {
            NPCScheduleManager.Instance.RegisterNPC(this);
        }
        
        Debug.Log($"✅ NPC初始化: {data.name}");
    }
    
    void OnDestroy()
    {
        // 注销
        if (NPCScheduleManager.Instance != null)
        {
            NPCScheduleManager.Instance.UnregisterNPC(this);
        }
    }
    
    void Update()
    {
        // 让名字标签始终面向摄像机
        if (nameLabel != null && Camera.main != null)
        {
            nameLabel.transform.LookAt(Camera.main.transform);
            nameLabel.transform.Rotate(0, 180, 0);
        }

        
        // 更新动画
        UpdateAnimation();
        
        // 更新状态
        if (characterData != null)
        {
            UpdatePhysicalState();
        }
    }
    
    /// <summary>
    /// 时辰变化回调 - 由NPCScheduleManager调用
    /// </summary>
    public void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        if (dailySchedule == null)
        {
            return;
        }
        
        // 查询新时辰的行为
        TimeSlotBehavior timeSlot = dailySchedule.GetBehaviorForTime(newTimeOfDay);
        
        // 切换行为
        suggestedBehavior = timeSlot.behavior;
        
        //Debug.Log($"👤 {characterData?.name ?? "NPC"}: {newTimeOfDay} → {timeSlot.behavior}");
    }
    
    /// <summary>
    /// 切换行为状态
    /// </summary>
    private void SwitchBehavior(NPCBehaviorState newBehavior)
    {
        if (currentBehavior == newBehavior)
        {
            return;
        }
        
        // 退出旧行为
        ExitBehavior(currentBehavior);
        
        // 进入新行为
        currentBehavior = newBehavior;
        EnterBehavior(currentBehavior);
    }
    
    /// <summary>
    /// 进入行为状态
    /// </summary>
    private void EnterBehavior(NPCBehaviorState behavior)
    {
        if (animator == null)
        {
            return;
        }
        
        // 获取RandomWalk组件
        var randomWalk = GetComponent<NPCRandomWalk>();
        
        // 根据行为决定RandomWalk和移动控制
        if (behavior == NPCBehaviorState.Idle)
        {
            // Idle状态：启用RandomWalk，恢复移动能力
            if (randomWalk != null)
            {
                randomWalk.enabled = true;
            }
            
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;  // 恢复移动能力
            }
        }
        else
        {
            // 其他状态：禁用RandomWalk，停止移动
            if (randomWalk != null)
            {
                randomWalk.enabled = false;
            }
            
            // 停止NavMesh移动
            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
            }
        }
        
        // 重置所有行为动画参数
        ResetAllBehaviorAnimations();
        
        // 设置对应的动画参数
        switch (behavior)
        {
            case NPCBehaviorState.Sleeping:
                if (animator.parameters.Length > 0)
                {
                    animator.SetBool(IsSleepingHash, true);
                }
                break;
                
            case NPCBehaviorState.Working:
                if (animator.parameters.Length > 0)
                {
                    animator.SetBool(IsWorkingHash, true);
                }
                break;
                
            case NPCBehaviorState.Resting:
                if (animator.parameters.Length > 0)
                {
                    animator.SetBool(IsRestingHash, true);
        
                    // 随机选择Rest变体（0-2）
                    if (HasParameter(RestVariantHash))
                    {
                        int variant = Random.Range(0, 3);
                        animator.SetInteger(RestVariantHash, variant);
                    }
                }
                break;
                
            case NPCBehaviorState.Eating:
                if (animator.parameters.Length > 0)
                {
                    animator.SetBool(IsEatingHash, true);
                }
                break;
                
            case NPCBehaviorState.Idle:
            case NPCBehaviorState.Walking:
            default:
                // 保持Idle动画
                break;
        }
    }
    
    /// <summary>
    /// 退出行为状态
    /// </summary>
    private void ExitBehavior(NPCBehaviorState behavior)
    {
        // 如果要退出非Idle状态，恢复NavMeshAgent
        if (behavior != NPCBehaviorState.Idle && behavior != NPCBehaviorState.Walking)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }
    }
    
    /// <summary>
    /// 重置所有行为动画参数
    /// </summary>
    private void ResetAllBehaviorAnimations()
    {
        if (animator == null || animator.parameters.Length == 0)
        {
            return;
        }
        
        // 检查参数是否存在再设置
        if (HasParameter(IsSleepingHash))
            animator.SetBool(IsSleepingHash, false);
        if (HasParameter(IsWorkingHash))
            animator.SetBool(IsWorkingHash, false);
        if (HasParameter(IsRestingHash))
            animator.SetBool(IsRestingHash, false);
        if (HasParameter(IsEatingHash))
            animator.SetBool(IsEatingHash, false);
    }
    
    /// <summary>
    /// 检查动画参数是否存在
    /// </summary>
    private bool HasParameter(int paramHash)
    {
        if (animator == null)
        {
            return false;
        }
        
        foreach (var param in animator.parameters)
        {
            if (param.nameHash == paramHash)
            {
                return true;
            }
        }
        return false;
    }
    
    private void UpdateAnimation()
    {
        if (animator == null || agent == null) 
            return;
        
        // 获取NavMesh Agent的速度
        Vector3 velocity = agent.velocity;
        float speed = velocity.magnitude;
        
        // 转换到局部空间（相对角色朝向）
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        
        // 更新移动动画参数
        if (HasParameter(SpeedHash))
            animator.SetFloat(SpeedHash, speed);
        if (HasParameter(SpeedXHash))
            animator.SetFloat(SpeedXHash, localVelocity.x);
        if (HasParameter(SpeedZHash))
            animator.SetFloat(SpeedZHash, localVelocity.z);
    }
    
    private void UpdatePhysicalState()
    {
        // 获取当前行为的配置
        BehaviorStateConfig config = BehaviorStateDatabase.GetConfig(currentBehavior);
        
        // 应用状态变化
        characterData.physical.hunger += config.hungerCost * Time.deltaTime;
        characterData.physical.energy -= config.energyCost * Time.deltaTime;
        
        // 移动额外消耗
        if (agent.velocity.magnitude > 0.1f)
        {
            characterData.physical.energy -= Time.deltaTime * 0.05f;
        }
        
        // 限制范围
        characterData.physical.hunger = Mathf.Clamp(characterData.physical.hunger, 0, 100);
        characterData.physical.energy = Mathf.Clamp(characterData.physical.energy, 0, 100);
    }
    
    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }
    
    // 脚步声事件
    public void FootL() { }
    public void FootR() { }
}