using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    private static UIController instance;

    public static UIController Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("UIController is not initialized!");
            }
            return instance;
        }
    }

    public GameObject UI;
    public TextMeshProUGUI LogText;
    public TextMeshProUGUI SceneText;

    private float cooldownTime = 1f; // 冷却时间1秒
    private float lastToggleTime;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 监听 Debug.Log 事件
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        // 取消监听 Debug.Log 事件
        Application.logMessageReceived -= HandleLog;
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            // 检查是否超过冷却时间
            if (Time.time - lastToggleTime >= cooldownTime)
            {
                // 切换UI的显示状态
                UI.SetActive(!UI.activeSelf);
                // 更新上次操作时间
                lastToggleTime = Time.time;
                // 添加UI提示
                UIController.Instance.AddLogText($"UI visibility: {(UI.activeSelf ? "Shown" : "Hidden")}");
            }
        }

        if  (OVRInput.GetDown(OVRInput.Button.Four))
        {
            // 检查是否超过冷却时间
            if (Time.time - lastToggleTime >= cooldownTime)
            {
                // clear log text
                LogText.text = string.Empty;
                lastToggleTime = Time.time;
            }
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string formattedLog = null;

        // 根据日志类型添加前缀，仅处理 Error
        if (type == LogType.Error)
        {
            formattedLog = "[ERROR] " + logString;
        }

        if (formattedLog != null)
        {
            AddLogText(formattedLog);
        }
    }

    public void AddLogText(string newText)
    {
        if (LogText != null)
        {
            LogText.text += newText + "\n";
        }
        else
        {
            Debug.LogError("LogText is not assigned in the inspector!");
        }
    }

    public void UpdateSceneText(string newText)
    {
        if (SceneText != null)
        {
            SceneText.text = newText;
        }
        else
        {
            Debug.LogError("SceneText is not assigned in the inspector!");
        }
    }
}
