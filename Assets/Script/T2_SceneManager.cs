using System.Collections.Generic;
using UnityEngine;

public class T2_SceneManager : MonoBehaviour
{
    [Tooltip("判断该场景是否是SSVEP")]
    public bool IsSSVEP = true; 
    [Header("原始组设置")]
    [Tooltip("用于'Group'系列的原始GameObject，将被复制。")]
    public GameObject GroupOriginal; // 原始的GameObject，用于'Group'系列
    
    const int targetNumber = 11;     

    [Header("其他球体设置")]
    [Tooltip("用于'OtherBall'系列的GameObject，将被复制。")]
    public GameObject OtherBall;    // 用于'OtherBall'系列的GameObject
    [Tooltip("要创建的'OtherBall'副本数量。")]
    public int OtherBallNumber = 5; // 需要创建的'OtherBall'副本数量


    [Header("分布设置")]
    [Tooltip("用于在XY平面上分布的正方形区域的大小（宽度和高度）。")]
    public float DistributionAreaSize = 1400f; // 散布区域的大小 (14m x 14m)

    [Header("间距设置")]
    [Tooltip("任意两个复制的组中心之间的最小间隔距离。")]
    public float MinSeparationDistance = 2f; // 最小间隔属性
    [Tooltip("在放弃之前，为每个对象找到有效位置的最大尝试次数。")]
    public int MaxPlacementAttempts = 100; // 防止无限循环的尝试次数上限

    [Header("针对'Group'对象的遮挡设置")]
    public float OcclusionIncreaseDistance = 0.1f; // 用于GroupX对象上的Task3_Occlusion

    private Vector3 _initialCenterPosition; // 用于存储洗牌时的中心点
    private bool _hasGenerated = false; // 标记以确保初始中心点只设置一次

    public GameObject eye;
    //the distance from eyes to the certer
    float depthDistance = 100; //深度
    float targetVisualWidth_angle = 2f;
    ExpList expList;


    private void Awake()
    {
        expList = new ExpList();
        /*expList.initSceneSetting();
        expList.initCycleHZ(targetNumber);*/
    }

    private void Start()
    {
        GenerateGroups(); // 在唤醒时生成对象组
    }

    public static int GetTargetNumber()
    {
        return targetNumber;
    }

    public void GenerateGroups()
    {
        if (GroupOriginal == null)
        {
            Debug.LogError("GroupOriginal 未分配。无法创建 'Group' 副本。");
            // 可选：如果GroupOriginal为空，决定是否仍要生成OtherBalls
            // 目前，如果主要模板缺失，我们将返回。
            return;
        }
        if (OtherBall == null && OtherBallNumber > 0)
        {
            Debug.LogWarning("OtherBall 未分配，但 OtherBallNumber > 0。不会创建 'OtherBall' 副本。");
        }

        if (!_hasGenerated) // 仅在第一次生成时存储初始中心点
        {
            T2_Occlusion occulusion = GroupOriginal.GetComponent<T2_Occlusion>();
            if (occulusion)
            {
                Debug.Log("Reloacte the depth");
                float targetActualWidth = sceneUtility.targetActualWidth(targetVisualWidth_angle, occulusion.Ball2.transform, eye.transform);
                occulusion.Ball2.transform.localScale *= targetActualWidth;
                occulusion.Ball1.transform.localScale *= targetActualWidth;
                OtherBall.transform.localScale *= targetActualWidth;
                _initialCenterPosition = GroupOriginal.transform.position;
                _hasGenerated = true;
            }
        }

        float halfSize = DistributionAreaSize / 2f; // 散布区域半边长
        List<Vector3> placedPositions = new List<Vector3>(); // 存储已放置对象的位置
        int successfullyPlacedCountTotal = 0; // 成功放置的对象总数

        if (IsSSVEP)
        {
            // 初始化SSVEP设置；
            initSSVEP();
        }

        // --- 1. 生成 'Group' 对象 ---
        if (GroupOriginal != null && targetNumber > 0)
        {
            for (int i = 0; i < targetNumber; i++)
            {
                // 尝试放置对象，并获取实例化的对象
                if (TryPlaceObject(GroupOriginal, "Group" + i, _initialCenterPosition, halfSize, placedPositions, out GameObject newGroupInstance, i))
                {
                    successfullyPlacedCountTotal++;
                    // 特别为 Group 对象应用遮挡逻辑
                    T2_Occlusion occlusion = newGroupInstance.GetComponent<T2_Occlusion>();
                    if (occlusion != null)
                    {
                        // 这里的 'i' 是 GroupNumber 的循环索引，所以它是 0 到 GroupNumber-1
                        occlusion.TargetOcclusion = OcclusionIncreaseDistance * (i); // 原始逻辑: i+1
                        occlusion.Ball1.name = "Occlude" + i;
                        occlusion.Ball2.name = "Ball" + i;
                        Ball_SSVEP SSVEP = occlusion.Ball2.GetComponent<Ball_SSVEP>();
                        if (SSVEP != null)
                        {
                            SSVEP.Index = i + 1;
                            SSVEP.CycleHz = expList.targetCycleHz[i];
                            SSVEP.PhaseDelay = expList.targetCyclePhasedelay[i];
                        }
                        Ball ball = occlusion.Ball2.GetComponent<Ball>();
                        int index = i + 1;
                        if (ball != null)
                        {
                            ball.Index = index + 1;
                        }
                    }
                    newGroupInstance.transform.LookAt(eye.transform);
                }
                else
                {
                    Debug.LogWarning($"无法放置 Group{(i + 1)}。总放置数量可能少于请求数量。");
                }
            }
        }

        // --- 2. 生成 'OtherBall' 对象 ---
        if (OtherBall != null && OtherBallNumber > 0)
        {
            for (int i = 0; i < OtherBallNumber; i++)
            {
                // 对于 OtherBall，TryPlaceObject 的最后一个参数 (objectIndexForLogic) 对于其当前逻辑并非严格需要
                // 但为了保持一致性或未来使用，传递 'i'。
                if (TryPlaceObject(OtherBall, "OtherBall" + (i+1), _initialCenterPosition, halfSize, placedPositions, out GameObject _, i))
                {
                    successfullyPlacedCountTotal++;
                }
                else
                {
                    Debug.LogWarning($"无法放置 OtherBall{i+1}。总放置数量可能少于请求数量。");
                }
            }
        }

        

        // Debug.Log($"放置完成。总共成功放置 {successfullyPlacedCountTotal} 个对象。");

        // 如果原始模板不再需要，则停用或销毁它
        if (GroupOriginal != null && GroupOriginal.scene.IsValid()) // 检查它是否是场景对象
        {
            // GroupOriginal.SetActive(false); // 停用原始对象
            Destroy(GroupOriginal); // 如果您想销毁它，请取消此行的注释
        }

        // 如果原始模板不再需要，则停用或销毁它
        if (OtherBall != null && GroupOriginal.scene.IsValid()) // 检查它是否是场景对象
        {
            // GroupOriginal.SetActive(false); // 停用原始对象
            Destroy(GroupOriginal); // 如果您想销毁它，请取消此行的注释
        }
    }

    /// <summary>
    /// 尝试为对象找到一个有效位置并实例化它。
    /// </summary>
    /// <returns>如果成功则返回true，否则返回false。</returns>
    private bool TryPlaceObject(GameObject prefabToInstantiate, string objectName, Vector3 center, float areaHalfSize, List<Vector3> existingPositions, out GameObject instantiatedObject, int objectIndexForLogic)
    {
        instantiatedObject = null; // 初始化输出参数
        Vector3 potentialPosition = Vector3.zero; // 潜在位置
        bool positionFound = false; // 是否找到位置的标记
        int currentAttempts = 0; // 当前尝试次数

        while (!positionFound && currentAttempts < MaxPlacementAttempts)
        {
            currentAttempts++;
            // 计算随机的X和Y坐标
            float randomX = Random.Range(center.x - areaHalfSize, center.x + areaHalfSize);
            float randomY = Random.Range(center.y - areaHalfSize, center.y + areaHalfSize);
            float zPosition = center.z; // Z轴位置与中心点保持一致
            potentialPosition = new Vector3(randomX, randomY, zPosition);

            bool isValidPosition = true; // 当前位置是否有效的标记
            if (MinSeparationDistance > 0) // 仅当最小间隔大于0时才检查
            {
                foreach (Vector3 placedPos in existingPositions) // 遍历所有已放置的位置
                {
                    if (Vector3.Distance(potentialPosition, placedPos) < MinSeparationDistance) // 如果距离小于最小间隔
                    {
                        isValidPosition = false; // 则此位置无效
                        break; // 跳出循环，尝试下一个候选位置
                    }
                }
            }

            if (isValidPosition) // 如果位置有效
            {
                positionFound = true; // 标记为已找到
            }
        }

        if (positionFound) // 如果找到了有效位置
        {
            // 将实例化的对象作为此GameObject的子对象，放置在计算出的世界坐标位置
            instantiatedObject = Instantiate(prefabToInstantiate, potentialPosition, prefabToInstantiate.transform.rotation, this.transform);
            instantiatedObject.name = objectName; // 设置对象名称
            existingPositions.Add(potentialPosition); // 将新位置添加到已放置位置列表

            // Debug.Log($"在 {currentAttempts} 次尝试后，于 {instantiatedObject.transform.position} 创建了 {instantiatedObject.name}。");
            return true; // 返回成功
        }
        else
        {
            // Debug.LogWarning($"在 {MaxPlacementAttempts} 次尝试后，未能为 {objectName} 找到有效位置。");
            return false; // 返回失败
        }
    }

    /// <summary>
    /// 打乱此脚本所在GameObject的所有现有子GameObject的位置。
    /// 保持它们原始的Z轴位置。
    /// </summary>
    public void ShuffleChildPositions()
    {
        if (!_hasGenerated) // 检查是否已生成过对象组
        {
            // Debug.LogWarning("必须至少调用一次 GenerateGroups 来定义洗牌前的中心点。");
            return;
        }

        List<Transform> childrenToShuffle = new List<Transform>(); // 需要打乱位置的子对象列表
        foreach (Transform child in transform) // 'transform' 指的是此脚本所在GameObject的transform组件
        {
            childrenToShuffle.Add(child); // 添加子对象到列表
        }

        if (childrenToShuffle.Count == 0) // 如果没有子对象
        {
            Debug.Log("没有子对象可以打乱位置。");
            return;
        }

        Debug.Log($"开始打乱 {childrenToShuffle.Count} 个子对象的位置...");

        List<Vector3> newPositions = new List<Vector3>(); // 存储本次洗牌操作中已分配的新位置
        float halfSize = DistributionAreaSize / 2f; // 散布区域半边长
        int successfullyShuffledCount = 0; // 成功打乱位置的子对象数量

        foreach (Transform childToMove in childrenToShuffle) // 遍历每个需要移动的子对象
        {
            Vector3 potentialPosition = Vector3.zero; // 潜在的新位置
            bool positionFound = false; // 是否找到新位置的标记
            int currentAttempts = 0; // 当前尝试次数

            // 保留子对象原始的Z轴位置用于新位置的计算
            float originalZ = childToMove.position.z;

            while (!positionFound && currentAttempts < MaxPlacementAttempts)
            {
                currentAttempts++;
                // 在定义的区域内计算随机的X和Y坐标
                float randomX = Random.Range(_initialCenterPosition.x - halfSize, _initialCenterPosition.x + halfSize);
                float randomY = Random.Range(_initialCenterPosition.y - halfSize, _initialCenterPosition.y + halfSize);
                potentialPosition = new Vector3(randomX, randomY, originalZ); // 使用原始的Z轴位置

                bool isValidPosition = true; // 当前位置是否有效的标记
                if (MinSeparationDistance > 0) // 仅当最小间隔大于0时才检查
                {
                    foreach (Vector3 placedPos in newPositions) // 检查与本次洗牌中已重新放置的对象之间的距离
                    {
                        if (Vector3.Distance(potentialPosition, placedPos) < MinSeparationDistance)
                        {
                            isValidPosition = false; // 此位置无效
                            break; // 跳出循环
                        }
                    }
                    // 可选：您可能还想检查那些尚未移动的子对象的原始位置是否会与当前子对象的新位置冲突。
                    // 为简单起见，此版本仅在洗牌期间检查*新分配的*位置。
                }

                if (isValidPosition) // 如果位置有效
                {
                    positionFound = true; // 标记为已找到
                }
            }

            if (positionFound) // 如果找到了有效的新位置
            {
                childToMove.position = potentialPosition; // 更新子对象的位置
                newPositions.Add(potentialPosition); // 将其新位置添加到列表中，供后续检查
                childToMove.LookAt(eye.transform);
                successfullyShuffledCount++; // 成功计数增加
                // Debug.Log($"在 {currentAttempts} 次尝试后，将 {childToMove.name} 移动到 {potentialPosition}。");
            }
            else
            {
                Debug.LogWarning($"在洗牌过程中，{MaxPlacementAttempts} 次尝试后未能为 {childToMove.name} 找到新的有效位置。它将保留在当前位置。");
            }
        }
        // Debug.Log($"洗牌完成。在 {childrenToShuffle.Count} 个子对象中，成功重新定位了 {successfullyShuffledCount} 个。");
    }

    private void initSSVEP()
    {
        expList.initCycleHZ(targetNumber);
    }
}
