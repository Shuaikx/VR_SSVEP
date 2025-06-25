using UnityEngine;
using System.IO; // �����ļ�����
using System; // ���ڻ�ȡ��ǰʱ��
using System.Text; // ���� StringBuilder

public enum ExpType{ 
    Gaze_Task1,
    SSVEP_Task1,
    Gaze_Task2,
    SSVEP_Task2
}

public class DataRecorder : MonoBehaviour
{
    [Header("�û�����")]
    public int UserID = 0;
    public string UserName = "Test";
    public ExpType expType;

    [Header("��������")]
    public sceneManager sceneManager;
    public T2_SceneManager sceneManager_t2;

    [Header("�ļ�����")]
    [Tooltip("CSV�ļ���������·�� (�������Ŀ��Ŀ¼�������Data�ļ���)")]
    private  string relativeFolderPath = "ExperimentData"; // CSV�ļ����������ļ���·��
    [Tooltip("CSV�ļ���ǰ׺")]
    private string fileNamePrefix = "Data_"; // CSV�ļ���ǰ׺

    private string currentFilePath = ""; // ��ǰ����д����ļ�������·��
    private StringBuilder csvBuilder = new StringBuilder(); // ���ڸ�Ч����CSV��

    // �������� (��Щ����ΪCSV���б��⣬Ҳ�Ǽ�¼����ʱ��Ҫ�����)
    // �����Ը�����Ҫ������Щ���Ե�˳�������
    private readonly string[] columnHeaders = new string[] {
        "UserID",
        "Timestamp",
        "ExpType",
        "BlockIndex",
        "TargetIndex",
        "PhaseRadius",
        "TargetVisualAngular",
        "Result", // bool ����
        "SelectionTime"
    };

    /// <summary>
    /// ����һ���µ�CSV�ļ���д���б��⡣
    /// ����ļ��Ѵ��ڣ�������ʱ�������һ���µ�Ψһ�ļ�����
    /// </summary>
    /// <param name="userID">�û�ID���������ļ�����һ���֡�</param>
    public void CreateNewLogFile()
    {
        // ��� StringBuilder �Ա����ļ�ʹ��
        csvBuilder.Clear();

        // ȷ���ļ��д���
        string folderPath = Path.Combine(Application.dataPath, "..", relativeFolderPath); // �˻ص���Ŀ��Ŀ¼�ٽ���ָ���ļ���
        if (Application.isEditor) // �ڱ༭���У�dataPath��Assets�ļ���
        {
            folderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, relativeFolderPath);
        }
        else // �ڴ����Ӧ���У�dataPath�� <AppName>_Data �ļ���
        {
            folderPath = Path.Combine(Application.dataPath, relativeFolderPath);
            // ���ߣ����ϣ���ڳ����ִ���ļ��Աߣ�
            // folderPath = Path.Combine(Application.dataPath, "..", relativeFolderPath);
            // Directory.GetParent(Application.dataPath).FullName �ڴ������ܲ����û�ָ�����λ�ã�
            // Application.persistentDataPath ��һ�����ɿ��Ŀ�ƽ̨��д·��ѡ��
            // Ϊ�˼����������������������� Data �ļ��еķ�ʽ��
            // ���ڴ���汾�����Ƽ�ʹ�� Application.persistentDataPath
            // folderPath = Path.Combine(Application.persistentDataPath, relativeFolderPath);
        }


        if (!Directory.Exists(folderPath))
        {
            try
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"�ļ��д����ɹ�: {folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"�����ļ���ʧ��: {folderPath}\n����: {e.Message}");
                return;
            }
        }

        // �����ļ����������û�ID��ʱ�����ȷ��Ψһ��
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{fileNamePrefix}{UserID}_{UserName}_{expType.ToString()}_{timestamp}.csv";
        currentFilePath = Path.Combine(folderPath, fileName);

        // д���б���
        csvBuilder.AppendLine(string.Join(",", columnHeaders));

        // ������������д���ļ�
        try
        {
            File.WriteAllText(currentFilePath, csvBuilder.ToString());
            Debug.Log($"����־�ļ��Ѵ���: {currentFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"������д���ļ�ͷʧ��: {currentFilePath}\n����: {e.Message}");
            currentFilePath = ""; // ����ʧ�ܣ�����·��
        }
    }


    /// <summary>
    /// ��ǰCSV�ļ���¼һ�����ݡ�
    /// </summary>
    /// <param name="userID">�û�ID��</param>
    /// <param name="experimentID">����������ʵ���š�</param>
    /// <param name="blockIndex">Block��������</param>
    /// <param name="targetIndex">Target��������</param>
    /// <param name="result">һ���������͵Ľ����</param>
    public void LogDataRow(string userID, string experimentType, int blockIndex, int targetIndex, bool result, float duration)
    {
        if (string.IsNullOrEmpty(currentFilePath))
        {
            Debug.LogWarning("��־�ļ���δ�����򴴽�ʧ�ܡ����ȵ��� CreateNewLogFile��");
            return;
        }

        // ��� StringBuilder �Ա�����ʹ�� (���߲���գ�����Append��ȡ�������д�����)
        // Ϊ�˵���׷�ӣ�ÿ�ζ����µ�Builder������վɵġ�
        // �����CreateNewLogFile������׷�ӣ��������Ѿ���csvBuilder���ˣ����Ե�һ��LogDataRowӦ��׷�ӵ������
        // ��ǰʵ����CreateNewLogFile�������д�룬����LogDataRowֱ��׷�����С�

        StringBuilder rowBuilder = new StringBuilder(); // Ϊ��ǰ�д���һ���µ�Builder

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // ��ȷ�������ʱ���

        // �����б����˳���������
        rowBuilder.Append(EscapeCSVField(userID)).Append(",");
        rowBuilder.Append(EscapeCSVField(timestamp)).Append(",");
        rowBuilder.Append(EscapeCSVField(experimentType)).Append(",");
        rowBuilder.Append(blockIndex.ToString()).Append(","); // intͨ������Ҫת��
        rowBuilder.Append(targetIndex.ToString()).Append(","); // intͨ������Ҫת��
        if (sceneManager)
        {
            rowBuilder.Append(sceneManager.angularDistance).Append(",");
            rowBuilder.Append(sceneManager.targetVisualWidth_angle).Append(",");
        }
        else if (sceneManager_t2)
        {
            rowBuilder.Append(" ").Append(",");
            rowBuilder.Append(" ").Append(",");
        }
        
        rowBuilder.Append(result.ToString()).Append(","); // boolͨ������Ҫת�� (����� True/False)
        rowBuilder.Append(duration.ToString());
        // ׷�ӵ��ļ�
        try
        {
            // ʹ�� StreamWriter ��׷���У�������ÿ�ζ��� File.AppendAllText (����Ч���Ե�)
            // ��Ϊ�˼򵥣��������� AppendAllText�����������ƿ�����Ż���
            File.AppendAllText(currentFilePath, rowBuilder.ToString() + Environment.NewLine);
            // Debug.Log("�������Ѽ�¼��"); // Ƶ����¼ʱ���ܻ�ˢ�������迪��
        }
        catch (Exception e)
        {
            Debug.LogError($"��¼������ʧ��: {currentFilePath}\n����: {e.Message}");
        }
        Debug.Log("add new line to data");
    }

    public void LogDataRow(int blockIndex, int targetIndex, bool result, float duration)
    {
        this.LogDataRow(UserID.ToString(), expType.ToString(), blockIndex, targetIndex, result, duration);
    }

    /// <summary>
    /// ����CSV�ֶ��е������ַ������綺�š����š����з�����
    /// ����ֶΰ������š����Ż��з�������˫�����������������ֶ��ڵ�˫�����滻Ϊ����˫���š�
    /// </summary>
    private string EscapeCSVField(string field)
    {
        if (field == null) return "";

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
        {
            // ���ֶ��ڵ�˫�����滻Ϊ����˫����
            string escapedField = field.Replace("\"", "\"\"");
            // ��˫���Ž������ֶ�������
            return $"\"{escapedField}\"";
        }
        return field;
    }


    // ----- ʾ���÷� (���Է��������ű��У�����������ű���Start/Update�в���) -----
    /*
    public DataLogger dataLoggerInstance; // ��Inspector����ק��ֵ������GetComponent��ȡ

    void Start()
    {
    // ��ȡ�򴴽� DataLogger ʵ��
    dataLoggerInstance = GetComponent<DataLogger>(); // ����ű�����ͬһ��GameObject��
    if (dataLoggerInstance == null)
    {
    dataLoggerInstance = FindObjectOfType<DataLogger>(); // ���ҳ����е�ʵ��
    if (dataLoggerInstance == null)
    {
    Debug.LogError("DataLogger instance not found!");
    return;
    }
    }


    // 1. ����һ���µ���־�ļ�
    string currentUserID = "User_001"; // ʾ���û�ID
    dataLoggerInstance.CreateNewLogFile(currentUserID);

    // 2. ��¼һЩ����
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_A", 1, 0, true);
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_A", 1, 1, false);
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_A", 2, 0, true);

    // ���������ַ���ʾ��
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_B, Condition_Alpha", 3, 2, true);
    dataLoggerInstance.LogDataRow(currentUserID, "Exp_C \"Special\"", 4, 1, false);

    }

    void OnApplicationQuit()
    {
    // ��ѡ����Ӧ���˳�ʱ���������ȷ�����л������ݶ���д�루���ʹ���˸����ӵĻ�����ƣ�
    // ���ڵ�ǰ�� File.AppendAllText����������д�룬����ͨ������Ҫ�ر���
    Debug.Log("Ӧ�ó����˳�����־��¼������");
    }
    */
}


