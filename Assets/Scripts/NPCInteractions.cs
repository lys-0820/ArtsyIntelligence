using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class NPCInteractions : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    private string characterName;
    [SerializeField] private GameObject loadingIndicator;//AI输入时的loading界面
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float typingSpeed = 0.05f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingIndicator.SetActive(false);
        characterName = deepseek_universal.Instance.npcCharacter.name;
        inputField.onSubmit.AddListener((text) =>
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("输入内容为空，请重新输入");
                return;
            }
            inputField.text = "";
            loadingIndicator.SetActive(true);
            deepseek_universal.Instance.sendMsgToDS(text, HandleAIResponse);
        });
    }
    /// <summary>
    /// 处理AI的响应
    /// </summary>
    /// <param name="content">AI回复的内容</param>
    /// <param name="isSuccess">请求是否成功</param>
    /// <exception cref="NotImplementedException"></exception>
    private void HandleAIResponse(string content, bool isSuccess)
    {
        StopAllCoroutines();
        StartCoroutine(ShowAIResponse(isSuccess ? characterName + ": "+content : characterName + ": （沉思中）"));

    }

    //显示ai回复的内容
    //TODO:改成DOTween
    private IEnumerator ShowAIResponse(string text)
    {
        loadingIndicator.SetActive(false);
        StringBuilder sb = new StringBuilder();
        foreach(char c in text)
        {
            sb.Append(c);
            dialogueText.text = sb.ToString();
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
