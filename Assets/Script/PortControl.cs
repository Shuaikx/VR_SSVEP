using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine.UI;

public class PortControl : MonoBehaviour
{
    #region ���崮������
    public string portName = "COM12";//������
    public int baudRate = 115200*4;//������
    public Parity parity = Parity.None;//Ч��λ
    public int dataBits = 8;//����λ
    public StopBits stopBits = StopBits.One;//ֹͣλ
    SerialPort sp = null;
    Thread dataReceiveThread;
    //���͵���Ϣ
    public List<byte> listReceive = new List<byte>();
    char[] strchar = new char[100];//���յ��ַ���Ϣת��Ϊ�ַ�������Ϣ
    string str;
    #endregion
    void Start()
    {
        baudRate = 115200 * 4;
        //baudRate = 115200 * 1;
        OpenPort();
        // �ڱ���ʵ���У�ֻ�漰���̼�����Ҫ���Ե�װ�ô�trigger ����Ҫ����trigger����˰�Port�Ľ�����Ϣ�Ĺ���ȥ��
        //dataReceiveThread = new Thread(new ThreadStart(DataReceiveFunction));
        //dataReceiveThread.Start();
    }
    void Update()
    {

    }

    #region �������ڣ����򿪴���
    public void OpenPort()
    {
        //��������
        sp = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        sp.ReadTimeout = 400;
        try
        {
            sp.Open();
            Debug.Log($"Start the {portName} port");
        }
        catch (Exception ex)
        {
            Debug.Log("Port open failed: " + ex.Message);
        }
    }
    #endregion



    #region �����˳�ʱ�رմ���
    void OnApplicationQuit()
    {
        ClosePort();
    }
    public void ClosePort()
    {
        try
        {
            sp.Close();
            dataReceiveThread.Abort();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    #endregion


    /// <summary>
    /// ��ӡ���յ���Ϣ
    /// </summary>
    void PrintData()
    {
        for (int i = 0; i < listReceive.Count; i++)
        {
            strchar[i] = (char)(listReceive[i]);
            str = new string(strchar);
        }
        Debug.Log(str);
    }

    #region ��������
    void DataReceiveFunction()
    {
        #region ���ֽ����鷢�ʹ�����Ϣ����Ϣȱʧ
        byte[] buffer = new byte[1024];
        int bytes = 0;
        while (true)
        {
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    bytes = sp.Read(buffer, 0, buffer.Length);//�����ֽ�
                    if (bytes == 0)
                    {
                        continue;
                    }
                    else
                    {
                        string strbytes = Encoding.Default.GetString(buffer);
                        Debug.Log(strbytes);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() != typeof(ThreadAbortException))
                    {
                    }
                }
            }
            Thread.Sleep(10);
        }
        #endregion
    }
    #endregion



    #region ��������
    public void WriteData(byte[] data)
    {
        if (sp.IsOpen)
        {
            sp.Write(data, 0, data.Length);
        }
    }
    public void WriteData(string data)
    {
        if (sp.IsOpen)
        {
            sp.WriteLine(data);
        }
    }
    
    #endregion
}

