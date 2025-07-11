using System;
using Oculus.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class FlickerControl : MonoBehaviour
{
    [SerializeField] PortControl port;
    [SerializeField] SocketControl socket;
    [SerializeField] aimTarget_SSEVEP aim;
    [SerializeField] private float notifyTime = 1f;
    [SerializeField] private float flickTime = 4f;
    [SerializeField] private float reponseTime = 1f;
    [SerializeField] private float timer = 0f;
    [SerializeField] private GameObject CurrentBall;
    [SerializeField] private Text TextBoard;
    [SerializeField] private DataRecorder dataRecorder;
    private static Ball_SSVEP ball;
    private Outline outline;
    private bool flickerState = false;
    private bool isStimulated = false;
    private bool socketDataSent = false;
    private bool pause = false;

    public static FlickerControl Instance { get; private set; }

    // TurnFlickerOn 事件：当闪烁开启时触发，并传递当前闪烁的GameObject
    public event Action OnFlickerTurnedOn;
    // TurnFlickerOff 事件：当闪烁关闭时触发
    public event Action OnFlickerTurnedOff;

    private void Awake()
    {
        // 实现单例模式逻辑
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("FlickerControl: Found an existing instance, destroying new one.", this);
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 如果你希望这个单例在场景切换时不被销毁，添加以下行
            // DontDestroyOnLoad(gameObject); // 根据你的需求决定是否需要
        }
    }

    private void Start()
    {
        ball = CurrentBall.GetComponent<Ball_SSVEP>();
        outline = CurrentBall.GetComponent<Outline>();
        Application.targetFrameRate = 60;
        dataRecorder.CreateNewLogFile();
    }

    void Update()
    {
        if (SocketControl.GetAuthenticateState() && flickerState)
        {
            if (!socketDataSent)
            {
                byte b = System.Convert.ToByte(ball.Index);
                byte[] target = new byte[1] { b };
                socket.SendDataToServer(target);
                socketDataSent = true;
            }
            timer += Time.deltaTime;

            if (timer <= notifyTime)
            {
                outline.OutlineColor = Color.blue;
                outline.enabled = true;
            }
            else if (timer > notifyTime && timer <= notifyTime + flickTime)
            {
                if (!isStimulated)
                {
                    outline.enabled = false;
                    byte b = System.Convert.ToByte(ball.Index);
                    byte[] trigger = new byte[1] { b };
                    port.WriteData(trigger);
                    Debug.Log($"Trigger {trigger[0]} is sent");
                    isStimulated = true;
                    //gameObject.BroadcastMessage("startStimulate");
                    OnFlickerTurnedOn?.Invoke();

                }
            }
            else if (timer > notifyTime + flickTime && timer <= notifyTime + flickTime + reponseTime)
            {
                if (isStimulated)
                {
                    //gameObject.BroadcastMessage("endStimulate");
                    OnFlickerTurnedOff?.Invoke();
                    isStimulated = false;

                }
            }
            else
            {
                socketDataSent = false;
                timer = 0f;
                flickerState = false;
            }
        }
    }

    public void TurnFlickerOn(GameObject targetBall)
    {

        // ball.endStimulate();
        // trialIndex += 1;
        gameObject.BroadcastMessage("endStimulate");
        CurrentBall = targetBall;
        ball = CurrentBall.GetComponent<Ball_SSVEP>();
        outline = CurrentBall.GetComponent<Outline>();
        if (!flickerState)
            flickerState = true;
        // Debug.Log($"Flicker On is {flickerState}");
        // Debug.Log($"Ball's HZ {ball.CycleHz} HZ");
    }
    public void TurnFlickerOff()
    {
        if (flickerState)
            flickerState = false;
        gameObject.BroadcastMessage("endStimulate");
    }

    public void HandleResult(byte[] resultMessage)
    {
        if (System.Convert.ToByte(254) == resultMessage[0])
        {
            Debug.Log($"Received: Saving the model, pause!");
            SetTextBoardContext("Saving...");
            pause = true;
            return;
        }
        if (System.Convert.ToByte(253) == resultMessage[0])
        {
            Debug.Log($"Received: Saving sucess, the stimulus will start in 3 seconds");
            pause = false;
            Invoke("toNextTarget", 3.0f);
            //SetTextBoardContext("Practice Phase");
            return;
        }
        bool result = System.Convert.ToByte(ball.Index) == resultMessage[0];
        if (result)
        {
            Debug.Log($"Received: Ball {ball.Index}'s result is Correct!");
            outline.OutlineColor = Color.green;
            outline.enabled = true;
            Invoke("toNextTarget", 2.0f);
        }
        else
        {
            Debug.Log($"Received: Ball {ball.Index}'s result is Incorrect!");
            outline.OutlineColor = Color.red;
            outline.enabled = true;
            Invoke("toNextTarget", 2.0f);
        }

        dataRecorder.LogDataRow(aim.getBlockIndex(), ball.Index, result, 3.0f);
    }
    private void toNextTarget()
    {
        if (pause)
            return;
        //Debug.Log("To next target");
        outline.enabled = false;
        if (aim != null)
            aim.showNextTarget();
        else
            return;
    }

    public void SetTextBoardContext(string context)
    {
        TextBoard.text = context;
    }
}
