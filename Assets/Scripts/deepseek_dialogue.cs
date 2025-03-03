using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class deepseek_dialogue : MonoBehaviour
{
    private string apiKey = "sk-074c57338af94af4a2057c77debd4e57";
    private string apiUrl = "https://api.deepseek.com/v1/chat/completions";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sendMsgToDS("I want to learn about math", null);
    }
    public void sendMsgToDS(string msg, UnityAction<string> callback)
    {
        StartCoroutine(PostRequest(msg, callback));
    }

    IEnumerator PostRequest(string message, UnityAction<string> callback)
    {
        //创建匿名类型请求体
        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new{role = "user", content = message}
            }
        };
        //序列化
        string jsonBody = JsonConvert.SerializeObject(requestBody);
        Debug.Log(jsonBody);
        //yield return null;

        //创建unity web request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);

        }
        else
        {
            string responseJson = request.downloadHandler.text;
            Debug.Log("response: " + responseJson);
        }
    }
}
