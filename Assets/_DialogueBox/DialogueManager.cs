using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class DialogueMessage
{
    public string characterName;
    public Sprite portrait;
    [TextArea(3, 10)] public string sentence;
    
    [Header("Fitur Pilihan")]
    public bool isChoice; // Centang jika ini adalah menu pilihan
    public string choice1Text; // Label pilihan 1 (misal: Ayam)
    public string choice2Text; // Label pilihan 2 (misal: Tikus)
}

public class DialogueManager : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public GameObject startButton;
    public GameObject nextButton;
    
    [Header("Panel Pilihan")]
    public GameObject choicePanel;
    public Button btnChoice1;
    public Button btnChoice2;

    [Header("Pengaturan Dialog")]
    public float typingSpeed = 0.04f;
    public DialogueMessage[] messages;

    private int currentMessageIndex;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    void Start()
    {
        dialogueBox.SetActive(false);
        choicePanel.SetActive(false);
        if (startButton != null) startButton.SetActive(true);
    }

    public void StartDialogue()
    {
        dialogueBox.SetActive(true);
        if (startButton != null) startButton.SetActive(false);
        currentMessageIndex = 0;
        ShowMessage();
    }

    void ShowMessage()
    {
        DialogueMessage currentMsg = messages[currentMessageIndex];
        nameText.text = currentMsg.characterName;
        portraitImage.sprite = currentMsg.portrait;
        portraitImage.gameObject.SetActive(currentMsg.portrait != null);

        // Cek apakah ini bagian pilihan
        if (currentMsg.isChoice)
        {
            nextButton.SetActive(false);
            choicePanel.SetActive(true);
            btnChoice1.GetComponentInChildren<TextMeshProUGUI>().text = currentMsg.choice1Text;
            btnChoice2.GetComponentInChildren<TextMeshProUGUI>().text = currentMsg.choice2Text;
        }
        else
        {
            nextButton.SetActive(true);
            choicePanel.SetActive(false);
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentMsg.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    // Dipanggil saat tombol pilihan 1 ditekan
    public void SelectChoice1()
    {
        Debug.Log("Pilihan: Ayam (Senang)");
        // Bisa tambahkan logika: jika pilih ayam, beri item atau ubah state game
        SkipToResult(true); 
    }

    // Dipanggil saat tombol pilihan 2 ditekan
    public void SelectChoice2()
    {
        Debug.Log("Pilihan: Tikus (Marah)");
        SkipToResult(false);
    }

    void SkipToResult(bool isHappy)
    {
        choicePanel.SetActive(false);
        // Logika sederhana: langsung lompat ke dialog hasil (biasanya index berikutnya)
        currentMessageIndex++; 
        ShowMessage();
    }

    public void DisplayNextLine()
    {
        if (isTyping) { StopCoroutine(typingCoroutine); dialogueText.text = messages[currentMessageIndex].sentence; isTyping = false; return; }
        
        currentMessageIndex++;
        if (currentMessageIndex < messages.Length) ShowMessage();
        else EndDialogue();
    }

    public void EndDialogue()
    {
        dialogueBox.SetActive(false);
        if (startButton != null) startButton.SetActive(true);
    }
}
