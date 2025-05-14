using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.IO;

public class SpeechToText : MonoBehaviour
{
    // 单例实例
    public static SpeechToText Instance { get; private set; }

    [SerializeField] private APIConfig apiConfig;

    // 回调事件，用于返回识别结果
    public delegate void OnTranscriptionComplete(string transcription);
    public event OnTranscriptionComplete TranscriptionComplete;

    private void Awake()
    {
        // 确保单例唯一性
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (apiConfig == null || string.IsNullOrEmpty(apiConfig.APIKey))
        {
            Debug.LogError("APIConfig is not assigned or APIKey is missing!");
            enabled = false;
            return;
        }
    }

    public IEnumerator SendAudioToWhisperAPI(AudioClip clip)
    {
        // 将音频转换为 WAV 格式
        byte[] audioData = WavUtility.FromAudioClip(clip);

        // 构建请求
        string url = "https://api.openai.com/v1/audio/transcriptions";
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-1"); // 使用 OpenAI Whisper 模型

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", "Bearer " + apiConfig.APIKey);

        Debug.Log("Sending audio to Whisper API...");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Whisper API Error: " + request.error);
            Debug.LogError($"Response: {request.downloadHandler.text}");
            TranscriptionComplete?.Invoke(null); // 返回空结果
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Whisper API Response: " + jsonResponse);

            // 解析返回的文字
            string transcription = ExtractTranscriptionFromResponse(jsonResponse);
            UIController.Instance.AddLogText("Transcription: " + transcription);
            TranscriptionComplete?.Invoke(transcription); // 触发回调事件
        }
    }

    private string ExtractTranscriptionFromResponse(string jsonResponse)
    {
        // 解析 JSON 响应，提取文字
        var response = JsonUtility.FromJson<WhisperResponse>(jsonResponse);
        return response.text;
    }
}

// 用于解析 Whisper API 的响应
[System.Serializable]
public class WhisperResponse
{
    public string text;
}

public static class WavUtility
{
    // 将 AudioClip 转换为 WAV 格式的字节数组
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            WriteWavHeader(stream, clip);
            WriteWavData(stream, clip);
            return stream.ToArray();
        }
    }

    // 写入 WAV 文件头
    private static void WriteWavHeader(Stream stream, AudioClip clip)
    {
        int sampleCount = clip.samples * clip.channels;
        int fileSize = 36 + sampleCount * 2;

        using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
        {
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF")); // Chunk ID
            writer.Write(fileSize); // Chunk Size
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE")); // Format
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt ")); // Subchunk1 ID
            writer.Write(16); // Subchunk1 Size (PCM)
            writer.Write((short)1); // Audio Format (PCM = 1)
            writer.Write((short)clip.channels); // Number of Channels
            writer.Write(clip.frequency); // Sample Rate
            writer.Write(clip.frequency * clip.channels * 2); // Byte Rate
            writer.Write((short)(clip.channels * 2)); // Block Align
            writer.Write((short)16); // Bits Per Sample
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data")); // Subchunk2 ID
            writer.Write(sampleCount * 2); // Subchunk2 Size
        }
    }

    // 写入 WAV 数据
    private static void WriteWavData(Stream stream, AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
        {
            foreach (float sample in samples)
            {
                short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(intSample);
            }
        }
    }
}
