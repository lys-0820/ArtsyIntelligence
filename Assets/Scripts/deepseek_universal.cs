using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class deepseek_universal : MonoBehaviour
{

    public static deepseek_universal Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [Header("API Settings")]
    [SerializeField] private string apiKey = "sk-074c57338af94af4a2057c77debd4e57";
    [SerializeField] private string modelName = "deepseek-chat";
    [SerializeField] private string apiUrl = "https://api.deepseek.com/v1/chat/completions";

    [Header("Dialogue settings")]
    [UnityEngine.Range(0, 2)] public float temperature = 1.3f;
    [UnityEngine.Range(1, 1000)] public int maxTokens = 100;

    //character
    [System.Serializable]
    public class NPCCharacter
    {
        public string name = "谜语人";
        [TextArea(3, 10)]
        public string personalityPrompt = "你是一个谜语人，你擅长给别人出冷幽默的谜语让别人猜。";
    }
    [SerializeField] public NPCCharacter npcCharacter;
    //回调委托，用于异步处理API响应
    public delegate void DialogueCallback(string content, bool isSuccess);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //sendMsgToDS("你好", null);
    }
    public void sendMsgToDS(string msg, DialogueCallback callback)
    {
        StartCoroutine(PostRequest(msg, callback));
    }

    /// <summary>
    /// 处理对话请求的协程
    /// </summary>
    /// <param name="message">玩家的输入内容</param>
    /// <param name="callback">回调函数，用于处理API响应</param>
    /// <returns></returns>
    IEnumerator PostRequest(string message, DialogueCallback callback)
    {
        List<Message> messages = new List<Message>
        {
            new Message{role = "system", content = npcCharacter.personalityPrompt}, //system settings
            new Message{role = "user", content = message} //user input
        };

        ChatRequest requestBody = new ChatRequest
        {
            model = modelName,
            messages = messages,
            temperature = temperature,
            max_tokens = maxTokens

        };

        //序列化
        string jsonBody = JsonConvert.SerializeObject(requestBody);
        //Debug.Log(jsonBody);
        //yield return null;
        UnityWebRequest request = CreateWebRequest(jsonBody);
        yield return request.SendWebRequest();

        //处理错误
        if (IsRequestError(request))
        {
            if(request.responseCode == 429) //速率限制
            {
                Debug.LogWarning("速率限制，延迟重试中...");
                yield return new WaitForSeconds(5f);
                StartCoroutine(PostRequest(message, callback));
                yield break;
            }
            //Debug.LogError("Error: " + request.error);
            //Debug.LogError("Response: " + request.downloadHandler.text);
            else
            {
                Debug.LogError($"API Error: {request.responseCode}\n{request.downloadHandler.text}");
                callback?.Invoke($"API请求失败:{request.downloadHandler.text}", false);
                yield break;

            }

        }


        //处理响应
       // Debug.Log(request.downloadHandler.text);
        DeepSeekResponse response = ParseResponse(request.downloadHandler.text);
        if(response!=null && response.choices.Length > 0)
        {
            Debug.Log("Reply " + request.downloadHandler.text);
            string npcReply = response.choices[0].message.content;
            Debug.Log(npcReply);
            callback?.Invoke(npcReply, true);
        }
        else
        {
            //报错
            callback?.Invoke("（不知道说什么了）", false);
        }
        request.Dispose(); //释放请求

    }
    /// <summary>
    /// 创建unityWebRequest对象
    /// </summary>
    /// <param name="jsonBody">请求体的json字符串</param>
    /// <returns>配置好的UnityWebRequest对象</returns>
    private UnityWebRequest CreateWebRequest(string jsonBody)
    {

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        //创建unity web request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer " + apiKey);
        request.SetRequestHeader("Accept", "application/json");
        return request;


    }
    /// <summary>
    /// 判断请求结果是否有报错
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private bool IsRequestError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.ProtocolError ||
               request.result == UnityWebRequest.Result.DataProcessingError;
    }
    /// <summary>
    /// 解析api响应
    /// </summary>
    /// <param name="jsonResponse">返回的json字符串</param>
    /// <returns>解析后的deepseek response对象</returns>
    private DeepSeekResponse ParseResponse(string jsonResponse)
    {
        try
        {
            DeepSeekResponse response = JsonUtility.FromJson<DeepSeekResponse>(jsonResponse);
            if (response == null || response.choices == null || response.choices.Length == 0)
            {
                Debug.LogError("响应格式错误或未包含有效数据");
                return null;
            }
            return response;
        }
        catch(System.Exception e)
        {
            Debug.LogError($"json解析失败:{e.Message}\n响应内容：{jsonResponse}");
            return null; 
        }
    }
    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public List<Message> messages;
        public float temperature;
        public int max_tokens;

    }
    [System.Serializable]
    public class Message
    {
        public string role; //system/user/assistant
        public string content;
        //public string reasoning_content;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message; //生成的消息

    }
    [System.Serializable]
    private class DeepSeekResponse
    {
        public Choice[] choices; //生成的选择列表
    }
}
