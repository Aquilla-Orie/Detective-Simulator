using LMNT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Responsible for voicing the case file and reading out the details to the player
public class CaseFileReader : MonoBehaviour
{
    [SerializeField] private TextAnimator _textAnimator;

    private LMNTSpeech _speech;
    private string caseFile = "";

    private void Awake()
    {
        _speech = GetComponent<LMNTSpeech>();
    }

    //Break down the case detailas into readable and understandable paragraphs
    public void OrganizeCaseDetails(CaseDetails caseDetails)
    {
        //First the case info
        //[caseNumber, caseTitle, incidenceOverview]
        var caseInfo = caseDetails.GetCaseInfo();
        string caseNumber = caseInfo[0];
        string caseName = caseInfo[1];
        string incidenceOverview = caseInfo[2];

        //Victim details
        //[name, age, occupation, background]
        var victimDetails = caseDetails.GetVictimDetails();
        string victimName = victimDetails[0];
        string victimAge = victimDetails[1];
        string victimOccupation = victimDetails[2];
        string victimBackground = victimDetails[3];

        //Suspect details
        //[name, [background, personalStory, gender]]
        var suspectDetails = caseDetails.GetSuspectDetails();
        string suspectString = $"There are {suspectDetails.Count} suspects in custody. The suspects are:\r\n\r\n";
        foreach (KeyValuePair<string, List<string>> suspect in suspectDetails)
        {
            suspectString += $"{suspect.Key}, {suspect.Value[2]}.\r\n{suspect.Value[0]}.\r\n{suspect.Value[1]}\r\n\r\n";
        }

        caseFile = $"Case File, {caseNumber} {caseName}.\r\n{incidenceOverview}\r\nThe victim, {victimName}, {victimAge}, {victimOccupation}, {victimBackground}\r\n\r\n{suspectString}";

        StartCoroutine(ReadText(caseFile));
    }
    public IEnumerator ReadText(string text)
    {
        //Break casefile into paragraphs and read it one at a time
        _speech.dialogue = text;
        //StartCoroutine(_speech.Talk());
        //yield return new WaitWhile(() => !_speech.GetComponent<AudioSource>().isPlaying);
        CaseManager.Instance.OpenCaseFilePanel();
        _textAnimator.AnimateText(text);
        CaseManager.Instance.ToggleLoadingScreen(false, 0);
        yield return null;
    }


    public void StopReading()
    {
        SkipReading();
        _speech.Stop();
    }

    public void SkipReading()
    {
        _textAnimator.SkipTextAnimation();
    }
}
