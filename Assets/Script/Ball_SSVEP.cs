using UnityEngine;
using System.Collections.Generic; // 引入 List 所需的命名空间

public class Ball_SSVEP : MonoBehaviour
{
    public int Index;
    public float CycleHz = 6f;       // 闪烁频率（Hz），即每秒闪烁次数
    public float PhaseDelay = 0f;    // 相位延迟（弧度）
    [SerializeField] private Color colorOrigin = Color.white;
    [SerializeField] private Color colorNotify = Color.blue;
    [SerializeField] private Color colorFlickerOn = Color.black;
    [SerializeField] private bool flickerOn = false;

    private List<Color> _flickerColorSequence = new List<Color>();
    private Material material;
    
    private float _sequenceTimer = 0f;

    private const int ScreenRefreshRateHz = 60; 

    void Start()
    {
        material = GetComponent<Renderer>().material;
        PrecalculateFlickerColors(); 
        FlickerControl.Instance.OnFlickerTurnedOn += startStimulate;
        FlickerControl.Instance.OnFlickerTurnedOff += endStimulate;
    }

    void Dispose()
    {
        FlickerControl.Instance.OnFlickerTurnedOn -= startStimulate;
        FlickerControl.Instance.OnFlickerTurnedOff -= endStimulate;
    }

    void Update()
    {
        if (flickerOn)
        {
            _sequenceTimer += Time.deltaTime;

            int sequenceLength = _flickerColorSequence.Count;

            if (sequenceLength > 0)
            {
                float framesPassed = _sequenceTimer * ScreenRefreshRateHz;
                int currentFrameIndex = (int)framesPassed % sequenceLength;
                
                material.color = _flickerColorSequence[currentFrameIndex];
            }
        }
    }

    /// <summary>
    /// 预计算一个完整闪烁周期内的所有颜色帧。
    /// </summary>
    private void PrecalculateFlickerColors()
    {
        _flickerColorSequence.Clear(); // 清空旧的序列
        int framesPerCycle = Mathf.RoundToInt(ScreenRefreshRateHz / CycleHz);

        if (framesPerCycle <= 0)
        {
            Debug.LogError("计算的帧数 <= 0，请检查 CycleHz 或 ScreenRefreshRateHz 设置。", this);
            return;
        }

        float timeStepPerFrameInCycle = (1.0f / CycleHz) / framesPerCycle;

        for (int i = 0; i < framesPerCycle; i++)
        {
            float currentTimeInCycle = i * timeStepPerFrameInCycle;
            float radians = 2 * Mathf.PI * CycleHz * currentTimeInCycle + PhaseDelay;
            float sineValue = Mathf.Sin(radians);

            float colorMix = (sineValue + 1f) * 0.5f;

            Color calculatedColor = Color.Lerp(colorOrigin, colorFlickerOn, colorMix);
            _flickerColorSequence.Add(calculatedColor);
        }
    }

    public void startNotify()
    {
        flickerOn = false; 
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
        _sequenceTimer = 0f; 
        if (!flickerOn)
            flickerOn = true;
    }

    public void endStimulate()
    {
        if (flickerOn)
            flickerOn = false;
        material.color = colorOrigin;
    }

    public void SetFlickerParameters(float newCycleHz, float newPhaseDelay)
    {
        if (CycleHz != newCycleHz || PhaseDelay != newPhaseDelay)
        {
            CycleHz = newCycleHz;
            PhaseDelay = newPhaseDelay;
            PrecalculateFlickerColors(); // 重新预计算
            _sequenceTimer = 0f; // 重置计时器
            if (flickerOn) // 如果正在闪烁，立即更新颜色
                material.color = _flickerColorSequence[0];
        }
    }
}