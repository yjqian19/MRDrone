using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using Meta.XR.MRUtilityKit;

public class OpenAIRequest : MonoBehaviour
{
    [SerializeField] private APIConfig apiConfig;
    private string model = "gpt-4-0125-preview";
    private bool isProcessingRequest = false;
    private float processingStartTime; // 处理请求的开始时间

    // Recording
    private int recordingThreshold = 1; // 录音时长（秒）
    private AudioClip recordedClip;
    private bool isRecording = false;

    // Scene Info
    private Transform redCubeTransform = null; // Initialize to null
    private Transform droneTransform = null; // Initialize to null
    private Transform userTransform = null; // Initialize to null
    private string sceneInfoText = string.Empty; // Initialize to empty string

    private void Start()
    {
        // 订阅 SpeechToText 的 TranscriptionComplete 事件
        SpeechToText.Instance.TranscriptionComplete += OnTranscriptionComplete;

        if (apiConfig == null || string.IsNullOrEmpty(apiConfig.APIKey))
        {
            Debug.LogError("APIConfig is not assigned or APIKey is missing!");
            enabled = false;
            return;
        }
    }

    private void OnTranscriptionComplete(string transcription)
    {
        if (!string.IsNullOrEmpty(transcription))
        {
            StartCoroutine(SendFunctionCallToOpenAI(transcription));
        }
        else
        {
            Debug.LogWarning("Transcription is empty or null.");
        }
    }

    void Update()
    {
        if (!isProcessingRequest)
        {

            // 录音；检测触发按钮是否按下
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                if (!isRecording)
                {
                    processingStartTime = Time.time; // 记录开始时间
                    UIController.Instance.AddLogText("Recording started...");
                    recordedClip = Microphone.Start(null, true, 30, 44100); // 无限录音
                    isRecording = true;
                }
            }
            else
            {
                if (isRecording)
                {
                    // 停止录音
                    int recordingPosition = Microphone.GetPosition(null);
                    if (recordingPosition > 0)
                    {
                        float[] samples = new float[recordingPosition * recordedClip.channels];
                        recordedClip.GetData(samples, 0);

                        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", recordingPosition, recordedClip.channels, recordedClip.frequency, false);
                        trimmedClip.SetData(samples, 0);
                        recordedClip = trimmedClip;
                    }

                    Microphone.End(null);
                    UIController.Instance.AddLogText("Recording stopped.");
                    isRecording = false;

                    // 检查录音时长是否大于等于设定的时长
                    if (recordedClip.length >= recordingThreshold)
                    {
                        isProcessingRequest = true; // Set the flag to indicate a request is being processed

                        // Update the scene information
                        UpdateScene();

                        UIController.Instance.AddLogText("Sending to Whisper...");
                        StartCoroutine(SpeechToText.Instance.SendAudioToWhisperAPI(recordedClip));
                    }
                    else
                    {
                        UIController.Instance.AddLogText("Recording is too short. Please try again.");
                        recordedClip = null; // 清空录音数据
                    }
                }
            }

            // 快捷键
            // if (OVRInput.GetDown(OVRInput.Button.One))
            // {
            //     instruction = "Rotate the drone to face the user.";
            //     UpdateScene();
            //     StartCoroutine(SendFunctionCallToOpenAI(instruction));
            // }
            // else if (OVRInput.GetDown(OVRInput.Button.Two))
            // {
            //     instruction = "Place the drone to the front of the user.";
            //     UpdateScene();
            //     StartCoroutine(SendFunctionCallToOpenAI(instruction));
            // }
        }

    }

    private void UpdateScene()
    {
        sceneInfoText = string.Empty; // Reset the scene info text

        // 获取场景中的对象
        redCubeTransform = Functions.Instance.GetCubeTransform();
        droneTransform = Functions.Instance.GetDroneTransform();
        userTransform = Camera.main.transform; // Assuming the camera is the user's transform

        MRUKRoom sceneInfo = MRUK.Instance.GetCurrentRoom();
        if (sceneInfo != null)
        {
            List <MRUKAnchor> anchors = sceneInfo.Anchors;
            if (anchors != null && anchors.Count > 0)
            {
                foreach (MRUKAnchor anchor in anchors)
                {
                    if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR ||
                    anchor.Label == MRUKAnchor.SceneLabels.CEILING ||
                    anchor.Label == MRUKAnchor.SceneLabels.WALL_FACE ||
                    anchor.Label == MRUKAnchor.SceneLabels.GLOBAL_MESH)
                    {
                        continue;
                    }
                    else
                    {
                        sceneInfoText += $"Object Name: {anchor.Label}, Position: {anchor.GetAnchorCenter()}\n";
                    }
                }
            }
            else
            {
                UIController.Instance.AddLogText("SceneInfo: No anchors found in the current scene.");
            }
        }
        else
        {
            UIController.Instance.AddLogText("SceneInfo: No scene information available.");
        }


        sceneInfoText += $"Object Name: Red Cube , Postion: {redCubeTransform.position}\n" +
            $"Drone Position: {droneTransform.position}, Forward: {droneTransform.forward}\n" +
            $"User Position: {userTransform.position}, Forward: {userTransform.forward}";

        UIController.Instance.UpdateSceneText(sceneInfoText);
    }

    IEnumerator SendFunctionCallToOpenAI(string instruction)
    {
        isProcessingRequest = true;

        string systemMessage = "You are a spatial assistant. Only use function calls to act.";
        string userMessage =
            $"{instruction}\n" +
            $"Scene Information: {sceneInfoText}\n";

        // Prepare the prompt / request object (Newtonsoft.Json)
        var requestObj = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = userMessage }
            },
            tools = new[] {
                new {
                    type = "function",
                    function = new {
                        name = "MoveDrone",
                        description = "Moves the drone to a target position in world space",
                        parameters = new {
                            type = "object",
                            properties = new Dictionary<string, object> {
                                { "x", new { type = "number", description = "X coordinate in world space" } },
                                { "y", new { type = "number", description = "Y coordinate in world space" } },
                                { "z", new { type = "number", description = "Z coordinate in world space" } }
                            },
                            required = new[] { "x", "y", "z" }
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "MoveDroneToFront",
                        description = "Moves the drone to the front of the user",
                        parameters = new {
                            type = "object",
                            properties = new Dictionary<string, object>(), // 无参数
                            required = new string[] {}
                        }
                    }
                },
                new {
                    type = "function",
                    function = new {
                        name = "RotateDroneToFace",
                        description = "Rotates the drone to face the user",
                        parameters = new {
                            type = "object",
                            properties = new Dictionary<string, object>(), // 无参数
                            required = new string[] {}
                        }
                    }
                }
            },
            tool_choice = "auto", // "auto" will let the model decide when to call the function

        };

        string body = JsonConvert.SerializeObject(requestObj, Formatting.Indented);
        Debug.Log("Request Body: " + body);

        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        byte[] postData = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiConfig.APIKey);

        UIController.Instance.AddLogText("Sending request to GPT...");

        yield return request.SendWebRequest();

        // 请求结束后重置状态
        isProcessingRequest = false;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("OpenAI Error: " + request.error);
            Debug.LogError($"Response: {request.downloadHandler.text}");

        }
        else
        {
            string json = request.downloadHandler.text;
            HandleFunctionCall(json);
        }
    }

    void HandleFunctionCall(string json)
    {
        var response = JsonConvert.DeserializeObject<ChatResponse>(json);
        var toolCalls = response.choices[0].message.tool_calls;

        foreach (var call in toolCalls)
        {
            string name = call.function.name;
            string argumentsJson = call.function.arguments;

            // 调用对应函数
            ExecuteUnityFunction(name, argumentsJson);
            UIController.Instance.AddLogText($"Function: {name}, Arguments: {argumentsJson}");
        }

        float totalTime = Time.time - processingStartTime; // 计算处理请求的总时间
        UIController.Instance.AddLogText($"Total processing time: {totalTime:F2} seconds");

    }

    void ExecuteUnityFunction(string name, string argumentsJson)
    {
        if (name == "MoveDrone")
        {
            // 解析参数
            var args = JsonConvert.DeserializeObject<MoveDroneArgs>(argumentsJson);
            // 调用实际的无人机移动方法
            Functions.Instance.MoveDrone(new Vector3(args.x, args.y, args.z));
            UIController.Instance.AddLogText($"MoveDrone to ({args.x}, {args.y}, {args.z})");
        }
        else if (name == "MoveDroneToFront")
        {
            Functions.Instance.MoveDroneToFront(userTransform);
            UIController.Instance.AddLogText("MoveDroneToFront called");
        }
        else if (name == "RotateDroneToFace")
        {
            Functions.Instance.RotateDroneToFace(userTransform);
            UIController.Instance.AddLogText("RotateDroneToFace called");
        }
        else
        {
            Debug.LogWarning("Unknown function call: " + name);
        }
    }


}

// 用于解析 MoveDrone 函数参数的类
[System.Serializable]
public class MoveDroneArgs
{
    public float x;
    public float y;
    public float z;
}

// Request
// 新增用于JSON序列化的类
[System.Serializable]
public class ChatRequest
{
    public string model;
    public RequestMessage[] messages;
    public Function[] functions;
    public FunctionCallRequest function_call;
}

[System.Serializable]
public class RequestMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class Function
{
    public string name;
    public string description;
    public FunctionParameters parameters;
}

[System.Serializable]
public class FunctionParameters
{
    public string type;
    public Dictionary<string, PropertyType> properties; // 使用字典
    public string[] required;
}

[System.Serializable]
public class PropertyType
{
    public string type;
    public string description; // 可选字段，提供额外说明
}

[System.Serializable]
public class EmptyProperties
{
    // 空属性对象
}

[System.Serializable]
public class FunctionCallRequest
{
    public string name;
}


// Response
// Add the following classes to handle the JSON response from OpenAI
[System.Serializable]
public class ChatResponse
{
    public Choice[] choices;
}
public class Choice
{
    public Message message;
}

public class Message
{
    public ToolCall[] tool_calls;
}

public class ToolCall
{
    public ToolFunction function;
}

public class ToolFunction
{
    public string name;
    public string arguments;
}
