using Meta.XR.ImmersiveDebugger.UserInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ExpList {
    private List<float> angularDistance_O = new List<float>() { 20f, 30f, 40f };
    private List<float> targetWidth_O = new List<float>() { 2f, 1.5f, 1f };
    public List<Tuple<float, float>> expSettings;
    public float[] targetCycleHz;
    public float[] targetCyclePhasedelay;
    // ������� ˳����� ÿ�ζ�Ҫ���� �����ظ�

    public ExpList()
    {
        expSettings = new List<Tuple<float, float>>();
        targetCycleHz = new float[0];
        targetCyclePhasedelay = new float[0];
}

    public void initSceneSetting()
    {
        Tuple<float, float>[,] combinationMatrix = CreateCombinationMatrix(angularDistance_O, targetWidth_O);

        List<Tuple<float, float>> shuffledList = ShuffleMatrixElements(combinationMatrix);
        Debug.Log("���Һ��ʵ������: " + shuffledList.Count);
        expSettings = shuffledList;
    }

    public void initCycleHZ(int targetNum)
    {
        float[] freqs = new float[40];
        float[] delays = new float[40];
        int index = 0;
        for (float f = 8f; f <= 15.8f; f = f + 0.4f)
        {
            freqs[index] = f;
            delays[index] = (f * 0.35f) % 2f;
            index = index + 1;
        }
        targetCycleHz = freqs[0..targetNum];
        targetCyclePhasedelay = delays[0..targetNum];
    }

    private Tuple<T, U>[,] CreateCombinationMatrix<T, U>(List<T> listA, List<U> listB)
    {
        if (listA == null || listB == null || listA.Count == 0 || listB.Count == 0)
        {
            Debug.LogError("������б���Ϊ�ջ�ա�");
            return null;
        }

        int rows = listA.Count;
        int cols = listB.Count;
        Tuple<T, U>[,] matrix = new Tuple<T, U>[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = Tuple.Create(listA[i], listB[j]);
            }
        }

        return matrix;
    }
    

    /// <summary>
    /// ��һ����Ԫ����������Ԫ��������ң�������һ���µ��б��С�
    /// ʹ���� Fisher-Yates ϴ���㷨��ȷ������ԡ�
    /// </summary>
    /// <param name="matrix">Ҫ���ҵĶ�Ԫ�����</param>
    /// <returns>һ���������о���Ԫ����˳����������б�</returns>
    private List<Tuple<T, U>> ShuffleMatrixElements<T, U>(Tuple<T, U>[,] matrix)
    {
        if (matrix == null)
        {
            Debug.LogError("����ľ�����Ϊ�ա�");
            return new List<Tuple<T, U>>(); // ����һ�����б��Ա������
        }

        List<Tuple<T, U>> flatList = new List<Tuple<T, U>>();
        foreach (var element in matrix)
        {
            flatList.Add(element);
        }

        int n = flatList.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            Tuple<T, U> temp = flatList[i];
            flatList[i] = flatList[j];
            flatList[j] = temp;
        }
        return flatList;
    }
}
