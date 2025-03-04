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
        public string name = "������";
        [TextArea(3, 10)]
        public string personalityPrompt = "����һ�������ˣ����ó������˳�����Ĭ�������ñ��˲¡�";
    }
    [SerializeField] public NPCCharacter npcCharacter;
    //�ص�ί�У������첽����API��Ӧ
    public delegate void DialogueCallback(string content, bool isSuccess);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //sendMsgToDS("���", null);
    }
    public void sendMsgToDS(string msg, DialogueCallback callback)
    {
        StartCoroutine(PostRequest(msg, callback));
    }

    /// <summary>
    /// ����Ի������Э��
    /// </summary>
    /// <param name="message">��ҵ���������</param>
    /// <param name="callback">�ص����������ڴ���API��Ӧ</param>
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

        //���л�
        string jsonBody = JsonConvert.SerializeObject(requestBody);
        //Debug.Log(jsonBody);
        //yield return null;
        UnityWebRequest request = CreateWebRequest(jsonBody);
        yield return request.SendWebRequest();

        //�������
        if (IsRequestError(request))
        {
            if(request.responseCode == 429) //��������
            {
                Debug.LogWarning("�������ƣ��ӳ�������...");
                yield return new WaitForSeconds(5f);
                StartCoroutine(PostRequest(message, callback));
                yield break;
            }
            //Debug.LogError("Error: " + request.error);
            //Debug.LogError("Response: " + request.downloadHandler.text);
            else
            {
                Debug.LogError($"API Error: {request.responseCode}\n{request.downloadHandler.text}");
                callback?.Invoke($"API����ʧ��:{request.downloadHandler.text}", false);
                yield break;

            }

        }


        //������Ӧ
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
            //����
            callback?.Invoke("����֪��˵ʲô�ˣ�", false);
        }
        request.Dispose(); //�ͷ�����

    }
    /// <summary>
    /// ����unityWebRequest����
    /// </summary>
    /// <param name="jsonBody">�������json�ַ���</param>
    /// <returns>���úõ�UnityWebRequest����</returns>
    private UnityWebRequest CreateWebRequest(string jsonBody)
    {

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        //����unity web request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer " + apiKey);
        request.SetRequestHeader("Accept", "application/json");
        return request;


    }
    /// <summary>
    /// �ж��������Ƿ��б���
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
    /// ����api��Ӧ
    /// </summary>
    /// <param name="jsonResponse">���ص�json�ַ���</param>
    /// <returns>�������deepseek response����</returns>
    private DeepSeekResponse ParseResponse(string jsonResponse)
    {
        try
        {
            DeepSeekResponse response = JsonUtility.FromJson<DeepSeekResponse>(jsonResponse);
            if (response == null || response.choices == null || response.choices.Length == 0)
            {
                Debug.LogError("��Ӧ��ʽ�����δ������Ч����");
                return null;
            }
            return response;
        }
        catch(System.Exception e)
        {
            Debug.LogError($"json����ʧ��:{e.Message}\n��Ӧ���ݣ�{jsonResponse}");
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
        public Message message; //���ɵ���Ϣ

    }
    [System.Serializable]
    private class DeepSeekResponse
    {
        public Choice[] choices; //���ɵ�ѡ���б�
    }
}
