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
    #region 定义串口属性
    public string portName = "COM12";//串口名
    public int baudRate = 115200*4;//波特率
    public Parity parity = Parity.None;//效验位
    public int dataBits = 8;//数据位
    public StopBits stopBits = StopBits.One;//停止位
    SerialPort sp = null;
    Thread dataReceiveThread;
    //发送的消息
    public List<byte> listReceive = new List<byte>();
    char[] strchar = new char[100];//接收的字符信息转换为字符数组信息
    string str;
    #endregion
    void Start()
    {
        baudRate = 115200 * 4;
        //baudRate = 115200 * 1;
        OpenPort();
        // 在本次实验中，只涉及到刺激程序要给脑电装置打trigger 不需要接受trigger，因此把Port的接受信息的功能去掉
        //dataReceiveThread = new Thread(new ThreadStart(DataReceiveFunction));
        //dataReceiveThread.Start();
    }
    void Update()
    {

    }

    #region 创建串口，并打开串口
    public void OpenPort()
    {
        //创建串口
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



    #region 程序退出时关闭串口
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
    /// 打印接收的信息
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

    #region 接收数据
    void DataReceiveFunction()
    {
        #region 按字节数组发送处理信息，信息缺失
        byte[] buffer = new byte[1024];
        int bytes = 0;
        while (true)
        {
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    bytes = sp.Read(buffer, 0, buffer.Length);//接收字节
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



    #region 发送数据
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

