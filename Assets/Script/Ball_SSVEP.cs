using UnityEngine;
using Oculus.Interaction;

public class Ball_SSVEP : MonoBehaviour
{
    public int Index;
    public float CycleHz = 6f;       // 闪烁频率（Hz），即每秒闪烁次数
    public float PhaseDelay = 0f;    // 相位延迟（弧度）
    [SerializeField] private Color colorOrigin = Color.white;
    [SerializeField] private Color colorNotify = Color.blue;
    [SerializeField] private Color colorFlickerOn = Color.black;
    [SerializeField] private bool flickerOn = false;

    private Material material;
    private float timer;

    void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (flickerOn)
        {
            Flicker();
        }
    }

    private void Flicker()
    {
        // 核心修正：正确计算正弦波频率与相位
        float radians = 2 * Mathf.PI * CycleHz * timer + PhaseDelay;
        float sineValue = Mathf.Sin(radians);

        // 将正弦波从[-1,1]映射到[0,1]区间
        float colorMix = (sineValue + 1f) * 0.5f;

        // 颜色混合
        material.color = Color.Lerp(colorOrigin, colorFlickerOn, colorMix);

        // 更新时间（无需取模）
        timer += Time.deltaTime;
    }

    // 保留其他方法（startNotify/endNotify等）...
    public void startNotify()
    {
        timer = 0f;
        if (material.color != colorNotify)
            material.color = colorNotify;
    }

    public void endNotify()
    {
        if (material.color == colorNotify)
            material.color = colorOrigin;
    }

    public void startStimulate()
    {
        if (!flickerOn)
            flickerOn = true;
    }

    public void endStimulate()
    {
        if (flickerOn)
            flickerOn = false;
        material.color = colorOrigin;
    }
}
