using UnityEngine;

public class CameraControlWithMouse : MonoBehaviour
{
    [Header("�ӽǿ��Ʋ���")]
    [Tooltip("���������")]
    public float mouseSensitivity = 100f;

    [Tooltip("��ֱ�ӽǵ���С�Ƕȣ����Ͽ����٣�")]
    public float minPitch = -85f; // ���Ͽ������Ƕȣ�ͨ���Ǹ�ֵ��

    [Tooltip("��ֱ�ӽǵ���С�Ƕȣ����¿����٣�")]
    public float maxPitch = 85f;  // ���¿������Ƕ�

    [Tooltip("ˮƽ�ӽ����ƣ��ӳ�ʼ�������Ҹ���ת���ٶȣ�90��ʾ�ܹ�180����Ұ��")]
    public float horizontalYawLimit = 90f; // ˮƽ��������ĵ����Ҹ�����ת�ĽǶ�

    private float xRotation = 0f; // ��ǰ������Ĵ�ֱ��ת�Ƕ� (Pitch)
    private float yRotation = 0f; // ��ǰ�������ˮƽ��ת�Ƕ� (Yaw)
    private float initialYaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 initialEulerAngles = transform.localEulerAngles;

        // ��ʼ��ˮƽ��ת (Yaw)
        initialYaw = initialEulerAngles.y; // ��¼��ʼ��Y�ᳯ����Ϊ���Ƶ�����
        yRotation = initialYaw;            // ��ǰ��yRotation�ӳ�ʼ����ʼ�ۻ�

        // ��ʼ����ֱ��ת (Pitch)
        xRotation = initialEulerAngles.x;
        if (xRotation > 180f) // ��ŷ����X�ķ�Χ�� (0,360) ת���� (-180, 180)
        {
            xRotation -= 360f;
        }
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch); // Ӧ�ó�ʼ��Pitch����

        // Ӧ�ó�ʼ��ת��ȷ�������ֵһ��
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- ˮƽ��ת (Yaw) ---
        yRotation += mouseX; // �ۻ��ܵ�Y����ת

        // ���㵱ǰ�ۻ���yRotation����ڳ�ʼ����(initialYaw)�ĽǶȲ�
        // Mathf.DeltaAngle �᷵�������Ƕ�֮�����̲��죨����350�Ⱥ�10��֮����20�ȣ�������-340�ȣ�
        float angleDifferenceFromInitial = Mathf.DeltaAngle(initialYaw, yRotation);

        // ������ǶȲ������� [-horizontalYawLimit, +horizontalYawLimit] ��Χ��
        float clampedAngleDifference = Mathf.Clamp(angleDifferenceFromInitial, -horizontalYawLimit, horizontalYawLimit);

        // ����Ӧ�õ�Y����ת�ǳ�ʼ������ϱ������˵ĽǶȲ�
        float finalYaw = initialYaw + clampedAngleDifference;

        // --- ��ֱ��ת (Pitch) ---
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        // Ӧ����ת
        transform.localRotation = Quaternion.Euler(xRotation, finalYaw, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}