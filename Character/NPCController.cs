using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    public CharacterRuntimeData characterData;
    
    private NavMeshAgent agent;
    private TextMesh nameLabel;
    private Animator animator; // 添加
    
    // 动画参数名
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int SpeedXHash = Animator.StringToHash("SpeedX");
    private static readonly int SpeedZHash = Animator.StringToHash("SpeedZ");
    
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
            nameLabel.text = $"{data.name}\n{data.age}岁";
        }
        
        // 根据年龄调整速度
        float speedMultiplier = data.age < 16 ? 0.7f : 
                               data.age > 60 ? 0.5f : 1f;
        agent.speed = 2.5f * speedMultiplier;
        
        // 重要：确保Agent不会陷入地面
        agent.baseOffset = data.age < 16 ? 0.3f : 0f;
        
        // 设置位置
        transform.position = data.position;
        
        Debug.Log($"✅ NPC初始化: {data.name}");
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
    
    private void UpdateAnimation()
    {
        if (animator == null || agent == null) return;
        
        // 获取NavMesh Agent的速度
        Vector3 velocity = agent.velocity;
        float speed = velocity.magnitude;
        
        // 转换到局部空间（相对角色朝向）
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        
        // 更新动画参数
        animator.SetFloat(SpeedHash, speed);
        animator.SetFloat(SpeedXHash, localVelocity.x);
        animator.SetFloat(SpeedZHash, localVelocity.z);
    }
    
    private void UpdatePhysicalState()
    {
        characterData.physical.hunger -= Time.deltaTime * 0.05f;
        
        if (agent.velocity.magnitude > 0.1f)
        {
            characterData.physical.energy -= Time.deltaTime * 0.1f;
        }
        
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
    
    // 在NPCController类的最后添加
    public void FootL()
    {
        // 脚步声事件 - 左脚
        // 将来可以在这里播放脚步音效
    }

    public void FootR()
    {
        // 脚步声事件 - 右脚
    }



}