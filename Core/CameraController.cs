using UnityEngine;

/// <summary>
/// 自由摄像机控制：WASD 平移、右键旋转、左键拖拽、滚轮缩放
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("初始定位")]
    public Vector3 initialPosition = new Vector3(332f, 210f, 225f);
    public Vector2 initialRotation = new Vector2(45f, 0f);
    
    [Header("移动")]
    public float moveSpeed = 10f;
    public float fastMoveMultiplier = 2f;
    
    [Header("旋转/拖拽")]
    public float rotationSensitivity = 3f;
    public float dragSpeed = 0.5f;
    
    [Header("缩放")]
    public float scrollSpeed = 50f;
    [Tooltip("距地面的最近高度")]
    public float minZoomHeight = 2.5f;
    [Tooltip("距地面的最远高度")]
    public float maxZoomHeight = 50f;
    [Tooltip("键盘缩放速度")]
    public float keyboardZoomSpeed = 20f;
    public KeyCode zoomInKey = KeyCode.Equals;
    public KeyCode zoomOutKey = KeyCode.Minus;
    public LayerMask groundLayers = ~0;
    
    private Vector3 forwardPlane;
    private Vector3 rightPlane;
    private float rotationX;
    private float rotationY;
    
    private float lastGroundHeight;
    
    private void Start()
    {
        transform.position = initialPosition;
        rotationX = initialRotation.y;
        rotationY = initialRotation.x;
        lastGroundHeight = initialPosition.y - minZoomHeight;
        ApplyRotation();
        ClampHeight();
    }
    
    private void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleDrag();
        HandleZoom();
    }
    
    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            rotationX += Input.GetAxis("Mouse X") * rotationSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * rotationSensitivity;
            rotationY = Mathf.Clamp(rotationY, -89f, 89f);
            ApplyRotation();
        }
    }
    
    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        forwardPlane = transform.forward;
        forwardPlane.y = 0;
        forwardPlane.Normalize();
        rightPlane = transform.right;
        rightPlane.y = 0;
        rightPlane.Normalize();
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
        {
            float speed = moveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                speed *= fastMoveMultiplier;
            }
            
            Vector3 move = rightPlane * horizontal + forwardPlane * vertical;
            transform.position += move * speed;
            ClampHeight();
        }
    }
    
    private void HandleDrag()
    {
        if (Input.GetMouseButton(0))
        {
            float dragX = -Input.GetAxis("Mouse X") * dragSpeed;
            float dragY = -Input.GetAxis("Mouse Y") * dragSpeed;
            Vector3 drag = rightPlane * dragX + forwardPlane * dragY;
            transform.position += drag;
            ClampHeight();
        }
    }
    
    private void HandleZoom()
    {
        float delta = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        
        if (Input.GetKey(zoomInKey))
        {
            delta += keyboardZoomSpeed;
        }
        if (Input.GetKey(zoomOutKey))
        {
            delta -= keyboardZoomSpeed;
        }
        
        if (Mathf.Abs(delta) <= 0.001f)
        {
            return;
        }
        
        Vector3 target = transform.position + transform.forward * delta * Time.deltaTime;
        AdjustHeight(ref target);
        transform.position = target;
    }
    
    private void ClampHeight()
    {
        var pos = transform.position;
        AdjustHeight(ref pos);
        transform.position = pos;
    }
    
    private void AdjustHeight(ref Vector3 position)
    {
        float ground = SampleGroundHeight(position);
        float height = Mathf.Clamp(position.y - ground, minZoomHeight, maxZoomHeight);
        position.y = ground + height;
    }
    
    private float SampleGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 200f, Vector3.down, out hit, 500f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            lastGroundHeight = hit.point.y;
        }
        return lastGroundHeight;
    }
}
