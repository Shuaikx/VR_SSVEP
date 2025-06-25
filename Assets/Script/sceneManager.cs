﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Threading;
using System.Linq;
using UnityEditor;

/*
 * center point and eye posint should be calibrated in the future
 * custom target number, target visual width, angular distance for the exp
 * Angular Distance measured by angle
 * Depth Distance measured by z
 * First ball is right above the center
 * OneSide_angle affected by angularDistance
 * First ball position affected by centerPoint position, depthDistance, oneSide_angle
 * Fitts circle ball sequence affected by centerPoint position, angularDistance, depthDistance (first ball position) And target number.
 */

public class sceneManager : MonoBehaviour {

    public GameObject firstBall;
    public GameObject eye;
    public GameObject center;
    public GameObject Balls;
    public bool IsSSVEP = false;

    //set the following three factors
    static int targetNumber =11;
    
    public static int expCount = 0;
    //public static int expCount2 = 0;
    //the distance from eyes to the certer
    float depthDistance = 100; //深度

    Vector3 centerPoint; // 圆盘中心的位置，每一次更新场景的时候都要重置到 （0,0,depthDistance）这个位置
    Vector3 firstballPosition;

    //miminum included angle between two sides (side is from one target to center point)
    [Header("目前场景的设置")]
    public float targetVisualWidth_angle = 3;
    public float angularDistance = 20;

    float includedAngle_angle;
    float threeIncludedAngle_angle;
    float oneSide_angle;
    ExpList expList;
    private bool isPratice = true;

    private void Awake()
    {
        expList = new ExpList();
        expList.initSceneSetting();
        expList.initCycleHZ(targetNumber);
        angularDistance = expList.expSettings[expCount].Item1;
        targetVisualWidth_angle = expList.expSettings[expCount].Item2;

        includedAngle_angle = (float)360 / targetNumber;
        threeIncludedAngle_angle = 5 * includedAngle_angle;
        centerPoint = new Vector3(0, 0, depthDistance);
        if (isPratice)
        {
            angularDistance = 30f;
            targetVisualWidth_angle = 2f;
        }
        oneSide_angle = sceneUtility.outputOneSide_angle(angularDistance, threeIncludedAngle_angle);

        produceFittsCircle(targetNumber, firstBall, firstballPosition);
    }

    void Start () {
        // List<int> numberList = Enumerable.Range(1, targetNumber).ToList();
    }

    public void setIsPratice(bool state)
    {
        isPratice = state;
    }

    public void updateOriginSence()
    {
        angularDistance = expList.expSettings[expCount].Item1;
        targetVisualWidth_angle = expList.expSettings[expCount].Item2;
        if (isPratice)
        {
            angularDistance = 30f;
            targetVisualWidth_angle = 2f;
        }
        centerPoint = new Vector3(0, 0, depthDistance) + eye.transform.position;
        center.transform.position = centerPoint;
        oneSide_angle = sceneUtility.outputOneSide_angle(angularDistance, threeIncludedAngle_angle);

        changeBallPosition(targetNumber, firstBall);
    }

    public void updateScene()
    {
        expCount++;
        if(expCount >= 9)
        {
            expCount = 0;
        }
        //Debug.Log("expCount2:" + expCount2);
        Debug.Log("Scene Index: " + (expCount+1));
        angularDistance = expList.expSettings[expCount].Item1;
        targetVisualWidth_angle = expList.expSettings[expCount].Item2;
        Debug.Log("Target Width: " + targetVisualWidth_angle + ", Angular Distance: " + angularDistance);
        oneSide_angle = sceneUtility.outputOneSide_angle(angularDistance, threeIncludedAngle_angle);
        changeBallPosition(targetNumber, firstBall);
    }

    private void produceFittsCircle(int targetNumber, GameObject originBall, Vector3 startBallPosition)
    {
        firstballPosition = sceneUtility.outputFirstballPosition(centerPoint, depthDistance, oneSide_angle);
        originBall.transform.position = firstballPosition;
        float targetActualWidth = sceneUtility.targetActualWidth(targetVisualWidth_angle, originBall.transform, eye.transform);
        originBall.transform.localScale *= targetActualWidth;
        originBall.name = "Ball0";
        Ball_SSVEP SSVEP_O = originBall.GetComponent<Ball_SSVEP>();
        Ball ball_O = originBall.GetComponent<Ball>();
        if (ball_O != null)
        {
            ball_O.Index = 1;
        }
        if (SSVEP_O != null)
        {
            SSVEP_O.Index = 1;
            SSVEP_O.CycleHz = expList.targetCycleHz[0];
            SSVEP_O.PhaseDelay = expList.targetCyclePhasedelay[0];
        }

        Vector3 preBallPosition = firstballPosition;
        for (int i = 0; i < targetNumber - 1; i++) {
            Vector3 convertOncePosition = sceneUtility.convertPositionOnce(preBallPosition, threeIncludedAngle_angle);
            GameObject clone = GameObject.Instantiate(originBall, convertOncePosition, Quaternion.identity);
            clone.transform.parent = Balls.transform;
            Ball_SSVEP SSVEP = clone.GetComponent<Ball_SSVEP>();
            Ball ball = clone.GetComponent<Ball>();
            int index = i + 1;
            if (SSVEP != null) {
                SSVEP.Index = index+1;
                SSVEP.CycleHz = expList.targetCycleHz[index];
                SSVEP.PhaseDelay = expList.targetCyclePhasedelay[index];
            }
            if (ball != null)
            {
                ball.Index = index + 1;
            }
            clone.name = "Ball" + (index).ToString();
            clone.tag = "ball";
            preBallPosition = convertOncePosition;
        }
    }

    private void changeBallPosition(int targetNumber, GameObject originBall)
    {
        // 记录上一个球的位置
        firstballPosition = sceneUtility.outputFirstballPosition(centerPoint, depthDistance, oneSide_angle);
        // 将origin ball 移动到该位置
        originBall.transform.position = firstballPosition;
        //将目标小球变成预设的width
        float targetActualWidth = sceneUtility.targetActualWidth(targetVisualWidth_angle, originBall.transform, eye.transform);
        originBall.transform.localScale = new Vector3(1,1,1);
        originBall.transform.localScale *= targetActualWidth; 

        Vector3 preBallPosition = firstballPosition;
        for (int i = 0; i < targetNumber - 1; i++)
        {
            Vector3 convertOncePosition = sceneUtility.convertPositionOnce(preBallPosition, threeIncludedAngle_angle);
            GameObject gb = GameObject.Find("Ball" + (i + 1).ToString());
            gb.transform.position = convertOncePosition;
            gb.transform.localScale = new Vector3(1, 1, 1);
            gb.transform.localScale *= targetActualWidth;

            preBallPosition = convertOncePosition;
        }
    }

    public static int returnTargetNumber()
    {
        return targetNumber;
    }

    public float returnVisualSize()
    {
        return targetVisualWidth_angle;
    }

    public float returnAngularDistance()
    {
        return angularDistance;
    }

    public static void QuitApplication()
    {
        Debug.Log("尝试退出程序...");

        // 判断是否在 Unity Editor 中运行
#if UNITY_EDITOR
        // 如果在 Editor 中，停止 Play 模式
        EditorApplication.isPlaying = false;
#else
        // 如果在打包的程序中，调用 Application.Quit()
        Application.Quit();
#endif

        // 注意：Application.Quit() 在打包程序中不会立即退出，
        // 通常会在当前帧结束后或更晚发生。
    }
}