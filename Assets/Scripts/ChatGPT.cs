using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenAI;
using TMPro;
using UnityEngine;
using LMNT;
using System.Threading.Tasks;
using System;

public class ChatGPT : MonoBehaviour
{
    private OpenAIApi _openAI = new OpenAIApi();
    private List<ChatMessage> _messages = new List<ChatMessage>();

    private string _prompt = "I want you to come up with a crime scenario. Build up the main characters, their names, background story, suspects, personal story from the perspective of each individual, and then present this to me as a case file.\r\n\r\nThis is how it will go: In the case file, you provide me with the main story, the victim, the suspects, a brief on each individual, and also the perp and the motive.\n\nPresent the case files to me in this fashion:\r\nCase File #caseNumber: #caseTitle\r\nIncidence Overview: #overviewParagraph\r\nVictim:\r\nName:#victimName\r\nAge:#victimAge\r\nOccupation:#victimOccupation\r\nBackground:#victimBackground\r\nSuspects:#numberOfSuspects\r\nName:#suspectName\r\nBackground:#suspectCrimeRelation\r\nPersonal Story:#suspectPersonalStory\r\nGender:#suspectGender\r\n\r\nPerpetrator:#name\r\nMotive:#detailedMotiveofPerp";


    private void Start()
    {
        SendReply();
        //ReadFile();
    }

    private void ReadFile()
    {
        FileProcessor.ReadFile();
    }

    private async void SendReply()
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = _prompt,
        };

        //AppendMessage(newMessage);

        if (_messages.Count == 0) newMessage.Content = _prompt;

        _messages.Add(newMessage);

        CreateChatCompletionResponse completionResponse;
        try
        {
            completionResponse = await _openAI.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-4-turbo",
                Messages = _messages
            });
        }
        catch
        {
            //There was an error getting a response from ChatGPT, read from previous file!
            ReadFile();
            throw;
        }


        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();

            _messages.Add(message);

            FileProcessor.WriteToFile(_messages[_messages.Count - 1].Content);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt!");
        }
    }
}
