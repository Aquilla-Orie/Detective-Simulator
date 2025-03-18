using LMNT;
using OpenAI;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Suspect", menuName = "CreateSuspect", order = 1)]
public class Suspect : MonoBehaviour
{
    [SerializeField] private string _name;
    [SerializeField] private string _background;
    [SerializeField] private string _personalStory;
    [SerializeField] private string _gender;

    [SerializeField] private Animator _animator;

    [SerializeField] private GameObject _loadingSpinner;

    [SerializeField] private CaseManager _caseManager;

    private LMNTSpeech _speech;

    private OpenAIApi _openAI = new OpenAIApi();
    private List<ChatMessage> _messages = new List<ChatMessage>();

    private string _prompt;

    public void SetupInidividual(KeyValuePair<string, List<string>> suspectDetails, string[] caseInfo, string[] victimDetails)
    {
        _name = suspectDetails.Key;
        _background = suspectDetails.Value[0];
        _personalStory = suspectDetails.Value[1];
        _gender = suspectDetails.Value[2];

        _prompt = $"Assume the character of {_name}, a {_gender}. You are in a suspect in a crime. The crime overview is {caseInfo[2]}. The victim is {victimDetails[0]}, of age {victimDetails[1]}, occupation {victimDetails[2]}. This is their story: {victimDetails[3]}. This is your background {_background}, and your personal story is {_personalStory}. You are being questioned by me now. Reply to all my questions and try to convince me that you are innocent. Do not break character. Do not even tell me you are an AI model.";

        _animator = GetComponent<Animator>();
        _loadingSpinner.SetActive(false);
        //SendReply();
    }

    public void SetCaseManager(CaseManager manager)
    {
        _caseManager = manager;
    }

    public void SetVoice(string voice)
    {
        _speech = GetComponent<LMNTSpeech>();
        _speech.voice = voice;
    }

    public void SetAnim(Animator anim)
    {
        _animator = anim;
    }
    public void StopTalking()
    {
        _speech.Stop();
    }

    public void SetAnimTrigger()
    {
        _animator.SetTrigger("switch");
    }

    public string GetName()
    {
        return _name;
    }

    public List<ChatMessage> GetMessages()
    {
        return _messages;
    }


    public async void SendReply(string input = "")
    {
        _loadingSpinner?.SetActive(true);
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = input,
        };

        //AppendMessage(newMessage);

        if (_messages.Count == 0) newMessage.Content = _prompt + "\n" + input;

        _messages.Add(newMessage);

        var completionResponse = await _openAI.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4-turbo",
            Messages = _messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();

            _messages.Add(message);

            _speech.dialogue = message.Content;
            _caseManager.DisplaySuspectResponse(message.Content);
            //StartCoroutine(_speech.TalkWithAnim(_animator));
            _loadingSpinner.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt!");
        }
    }

}
