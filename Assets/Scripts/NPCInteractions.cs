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
    [SerializeField] private GameObject loadingIndicator;//AI����ʱ��loading����
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
                Debug.LogWarning("��������Ϊ�գ�����������");
                return;
            }
            inputField.text = "";
            loadingIndicator.SetActive(true);
            deepseek_universal.Instance.sendMsgToDS(text, HandleAIResponse);
        });
    }
    /// <summary>
    /// ����AI����Ӧ
    /// </summary>
    /// <param name="content">AI�ظ�������</param>
    /// <param name="isSuccess">�����Ƿ�ɹ�</param>
    /// <exception cref="NotImplementedException"></exception>
    private void HandleAIResponse(string content, bool isSuccess)
    {
        StopAllCoroutines();
        StartCoroutine(ShowAIResponse(isSuccess ? characterName + ": "+content : characterName + ": ����˼�У�"));

    }

    //��ʾai�ظ�������
    //TODO:�ĳ�DOTween
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
