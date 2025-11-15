using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("跟随设置")]
    public Vector3 offset = new Vector3(0, 5, -10);
    public float followSpeed = 3f;
    
    [Header("镜头控制")]
    public float mouseSensitivity = 3f;
    public float scrollSpeed = 5f;
    public float minDistance = 3f;
    public float maxDistance = 30f;
    
    [Header("按键")]
    public KeyCode nextNPCKey = KeyCode.N;
    public KeyCode prevNPCKey = KeyCode.B;
    public KeyCode freeLookKey = KeyCode.Mouse1; // 右键
    
    private Transform currentTarget;
    private int currentNPCIndex = 0;
    private Transform[] allNPCs;
    
    private float currentDistance = 10f;
    private float currentRotationX = 0f;
    private float currentRotationY = 30f;
    
    void Start()
    {
        currentDistance = offset.magnitude;
        Invoke("FindAllNPCs", 3f);
    }
    
    void FindAllNPCs()
    {
        GameObject npcsParent = GameObject.Find("NPCs");
        if (npcsParent != null && npcsParent.transform.childCount > 0)
        {
            System.Collections.Generic.List<Transform> npcList = new System.Collections.Generic.List<Transform>();
            
            foreach (Transform child in npcsParent.transform)
            {
                if (child != null && child.gameObject.activeSelf)
                {
                    npcList.Add(child);
                }
            }
            
            allNPCs = npcList.ToArray();
            
            if (allNPCs.Length > 0)
            {
                SwitchToNPC(0);
            }
            
            Debug.Log($"📷 找到 {allNPCs.Length} 个NPC");
        }
    }
    
    void Update()
    {
        // 切换NPC
        if (Input.GetKeyDown(nextNPCKey))
        {
            SwitchToNextNPC();
        }
        
        if (Input.GetKeyDown(prevNPCKey))
        {
            SwitchToPreviousNPC();
        }
        
        // 鼠标右键旋转视角
        if (Input.GetKey(freeLookKey))
        {
            currentRotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentRotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentRotationY = Mathf.Clamp(currentRotationY, 5f, 85f);
        }
        
        // 鼠标滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * scrollSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }
    
    void LateUpdate()
    {
        if (currentTarget != null)
        {
            // 计算旋转
            Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
            
            // 计算位置
            Vector3 direction = rotation * Vector3.back;
            Vector3 desiredPosition = currentTarget.position + Vector3.up * 2f + direction * currentDistance;
            
            // 平滑移动
            transform.position = Vector3.Lerp(
                transform.position, 
                desiredPosition, 
                Time.deltaTime * followSpeed
            );
            
            // 看向目标
            transform.LookAt(currentTarget.position + Vector3.up * 2f);
        }
    }
    
    void SwitchToNextNPC()
    {
        if (allNPCs == null || allNPCs.Length == 0) return;
        
        currentNPCIndex = (currentNPCIndex + 1) % allNPCs.Length;
        SwitchToNPC(currentNPCIndex);
    }
    
    void SwitchToPreviousNPC()
    {
        if (allNPCs == null || allNPCs.Length == 0) return;
        
        currentNPCIndex--;
        if (currentNPCIndex < 0) currentNPCIndex = allNPCs.Length - 1;
        SwitchToNPC(currentNPCIndex);
    }
    
    void SwitchToNPC(int index)
    {
        if (allNPCs == null || index < 0 || index >= allNPCs.Length) return;
        
        if (allNPCs[index] == null || !allNPCs[index].gameObject.activeSelf)
        {
            Debug.LogWarning($"⚠️ NPC {index} 无效，跳过");
            SwitchToNextNPC();
            return;
        }
        
        currentTarget = allNPCs[index];
        currentNPCIndex = index;
        
        NPCController npc = currentTarget.GetComponent<NPCController>();
        string npcName = npc != null && npc.characterData != null ? 
            $"{npc.characterData.familyName}{npc.characterData.name}" : 
            currentTarget.name;
        
        Debug.Log($"📷 {index + 1}/{allNPCs.Length}: {npcName}");
    }
}