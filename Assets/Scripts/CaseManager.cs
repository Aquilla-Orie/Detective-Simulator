using LMNT;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public struct CaseDetails
{
    public string[] CaseInfo;//[caseNumber, caseTitle, incidenceOverview]
    public string[] VictimDetails;//[name, age, occupation, background]
    public Dictionary<string, List<string>> SuspectDetails;//[suspectName, [background, personalStory, gender]]
    public string[] PerpAndMotive;//[perpName, motive]

    public void SetCaseInfo(string[] caseDetails)
    {
        CaseInfo = caseDetails;
    }
    public string[] GetCaseInfo()
    {
        return CaseInfo;
    }
    public void SetVictimDetails(string[] victimDetails)
    {
        VictimDetails = victimDetails;
    }
    public string[] GetVictimDetails()
    {
        return VictimDetails;
    }
    public void SetSuspectDetails(Dictionary<string, List<string>> suspectDetails)
    {
        SuspectDetails = suspectDetails;
    }
    public Dictionary<string, List<string>> GetSuspectDetails()
    {
        return SuspectDetails;
    }
    public void SetPerpAndMotive(string[] perpMotive)
    {
        PerpAndMotive = perpMotive;
    }
    public string[] GetPerpAndMotive()
    {
        return PerpAndMotive;
    }
}


public class CaseManager : MonoBehaviour
{
    public static CaseManager Instance;

    public bool IsDone { get; set; }

    [SerializeField] public Camera _detectiveCamera;
    [SerializeField] public Camera _perpCamera;

    [SerializeField] private TMP_InputField _perpInput;

    //Suspect placement transforms
    [SerializeField] public Transform _suspectChairTransform;
    [SerializeField] private Transform _spawnTransform;

    [SerializeField] private Suspect _individualPrefab;

    [SerializeField] private Button _suspectSelectButtonPrefab;
    [SerializeField] private RectTransform _buttonPanelRectTransform;
    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private GameObject _caseReaderPanel;

    [SerializeField] private GameObject _suspectInterrogationPanel;
    [SerializeField] private GameObject _suspectResponsePanel;
    [SerializeField] private TMP_Text _suspectResponseText;
    [SerializeField] private TMP_Text _suspectNameText;
    [SerializeField] private GameObject _suspectHistoryPanel;
    [SerializeField] private TMP_Text _suspectHistoryText;

    [SerializeField] private GameObject _donePanel;
    [SerializeField] private GameObject _openCasefilePanel;

    private CaseDetails _caseDetails;
    private CaseFileReader _caseFileReader;

    private Suspect _selectedSuspect;

    private string[] _maleNames = new string[] { "Curtis", "Ethel", "Giuseppe", "Henry", "Joe", "Maurice", "Oliver", "Szymon" };
    private string[] _femaleNames = new string[] { "Beatriz", "Donna", "Eleanor", "Kathrine", "Marzia", "Natalie", "Olivia", "Priya", "Shanti" };

    private void Awake()
    {
        IsDone = false;
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        if (Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(Instance.transform.root);
    }

    private void Start()
    {
        _caseFileReader = GetComponent<CaseFileReader>();
        ToggleLoadingScreen(true, 0);
        DisableSuspectInterrogationPanel();
        DisableSuspectSelectPanel();
    }

    private void Update()
    {
        if (_loadingScreen == null) { return; }
        if (_loadingScreen.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                RestartGameApp();
            }
        }
    }

    public void RestartGameApp()
    {
        string _filepath = AppDomain.CurrentDomain.BaseDirectory;

        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            _filepath = Path.Combine(_filepath, Application.productName + ".exe");
        }

        System.Diagnostics.Process.Start(_filepath);

        Application.Quit();

        // In the editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnEnable()
    {
        FileProcessor.onReadComplete += SetupScene;
    }

    private void OnDisable()
    {
        FileProcessor.onReadComplete -= SetupScene;
        StopAllCoroutines();
    }

    public void SetupScene()
    {
        var suspectDetails = _caseDetails.GetSuspectDetails();

        System.Random random = new System.Random();
        var maleNames = _maleNames.OrderBy(x => random.Next()).ToList();
        var femaleNames = _femaleNames.OrderBy(x => random.Next()).ToList();

        var maleAvatarIndex = Enumerable.Range(0, 7).OrderBy(x => random.Next()).ToList();
        var femaleAvatarIndex = Enumerable.Range(0, 4).OrderBy(x => random.Next()).ToList();

        foreach (KeyValuePair<string, List<string>> suspect in suspectDetails)
        {
            //Setup the suspect
            var i = Instantiate(_individualPrefab, _spawnTransform);
            i.SetupInidividual(suspect, _caseDetails.GetCaseInfo(), _caseDetails.GetVictimDetails());
            i.SetCaseManager(this);

            //Give the suspect different voices and avatars
            if (suspect.Value[2].Equals("Male"))
            {
                SetMaleAvatar(maleAvatarIndex[0], i);
                i.SetVoice(maleNames[0]);

                maleNames.RemoveAt(0);
                maleAvatarIndex.RemoveAt(0);
            }
            if (suspect.Value[2].Equals("Female"))
            {
                SetFemaleAvatar(femaleAvatarIndex[0], i);
                i.SetVoice(femaleNames[0]);

                femaleNames.RemoveAt(0);
                femaleAvatarIndex.RemoveAt(0);
            }

            //Setup the buttons
            SetupSuspectSelectButton(suspect.Key, i);
        }
        _buttonPanelRectTransform.gameObject.SetActive(false);

        //Give case info to the case file reader
        if (_caseFileReader == null)
            _caseFileReader = GetComponent<CaseFileReader>();

        _caseFileReader.OrganizeCaseDetails(_caseDetails);
        //Keep loading screen until audio is loaded too!!
    }

    private void SetupSuspectSelectButton(string suspectName, Suspect i)
    {
        var b = Instantiate(_suspectSelectButtonPrefab, _buttonPanelRectTransform);
        b.GetComponentInChildren<TMP_Text>().text = suspectName;
        b.name = suspectName;
        b.GetComponent<ButtonBase>().SetAssignedSuspect(i);
    }

    private static void SetFemaleAvatar(int femaleAvatarIndex, Suspect i)
    {
        var av = i.transform.Find("Female").GetChild(femaleAvatarIndex).gameObject;
        av.TryGetComponent<Animator>(out Animator anim);
        i.SetAnim(anim);
        av.SetActive(true);
    }

    private static void SetMaleAvatar(int maleAvatarIndex, Suspect i)
    {
        var av = i.transform.Find("Male").GetChild(maleAvatarIndex).gameObject;
        av.TryGetComponent<Animator>(out Animator anim);
        i.SetAnim(anim);
        av.SetActive(true);
    }

    public void SetCaseDetails(CaseDetails caseDetails)
    {
        _caseDetails = caseDetails;
        Debug.Log("Case managers set case details");
    }

    public void SuspectSelected(Suspect suspect)
    {
        _selectedSuspect = suspect;
        _selectedSuspect.transform.parent = _suspectChairTransform;
        _selectedSuspect.transform.localPosition = Vector3.zero;
        _selectedSuspect.transform.localRotation = Quaternion.Euler(Vector3.zero);

        _suspectInterrogationPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = $"Interrogating {_selectedSuspect.GetName()}";

        DisableSuspectSelectPanel();
        EnableSuspectInterrogationPanel();
        EnablePerpCamera();
    }
    public void SuspectDeselected()
    {
        _selectedSuspect.transform.parent = _spawnTransform;
        _selectedSuspect.transform.localPosition = Vector3.zero;
        _selectedSuspect.StopTalking();
        _selectedSuspect = null;
        DisableSuspectInterrogationPanel();
        EnableSuspectSelectPanel();
        EnableDetectiveCamera();
    }

    private void EnableSuspectInterrogationPanel()
    {
        _suspectInterrogationPanel.SetActive(true);
    }
    private void DisableSuspectInterrogationPanel()
    {
        _suspectInterrogationPanel.SetActive(false);
    }

    private void EnableSuspectSelectPanel()
    {
        _buttonPanelRectTransform.gameObject.SetActive(true);
        _donePanel.SetActive(true);
        _openCasefilePanel.SetActive(true);
    }
    private void DisableSuspectSelectPanel()
    {
        _buttonPanelRectTransform.gameObject.SetActive(false);
        _donePanel.SetActive(false);
        _openCasefilePanel.SetActive(false);
    }

    public void InterrogateSuspect()
    {
        string interrogationText = _inputField.text;
        interrogationText = interrogationText.Trim();
        if (string.IsNullOrEmpty(interrogationText))
        {
            return;
        }
        _selectedSuspect.SendReply(_inputField.text);
        _inputField.text = "";
        _suspectInterrogationPanel.SetActive(false);
    }

    public void DisplaySuspectResponse(string response)
    {
        _suspectResponsePanel.SetActive(true);
        _suspectResponseText.text = response;
        _suspectNameText.text = _selectedSuspect.GetName();
    }

    public void OpenSuspectHistory()
    {
        _suspectHistoryPanel.SetActive(true);
        RefreshHistory();
    }

    public void RefreshHistory()
    {
        List<ChatMessage> chatMessages = _selectedSuspect.GetMessages();
        string history = "";
        string name = _selectedSuspect.GetName();

        if (chatMessages.Count <= 0) return;

        history += $"Detective:\t{chatMessages[0].Content.Split('\n')[1].Trim()}\n\n";

        for (int i = 1; i < chatMessages.Count; i++)
        {
            history += i % 2 == 0 ? "Detective:\t" : $"{name}:\t";
            history += chatMessages[i].Content;
            history += "\n\n";
        }
        _suspectHistoryText.text = history;
    }

    public void ToggleLoadingScreen(bool state, float delay = 5)
    {
        if (state)
            Invoke("ShowLoadingScreen", delay);
        else
            Invoke("HideLoadingScreen", delay);
    }
    private void ShowLoadingScreen()
    {
        _loadingScreen.SetActive(true);
    }
    private void HideLoadingScreen()
    {
        _loadingScreen.SetActive(false);
    }

    public void CloseCaseFilePanel()
    {
        _caseReaderPanel.SetActive(false);
        _caseFileReader.StopReading();
        if (!IsDone)
            EnableSuspectSelectPanel();
        else
            SceneManager.LoadScene(0);
    }

    public void OpenCaseFilePanel()
    {
        _caseReaderPanel.SetActive(true);
        DisableSuspectInterrogationPanel();
        DisableSuspectSelectPanel();
    }

    private void EnableDetectiveCamera()
    {
        _detectiveCamera.gameObject.SetActive(true);
        _perpCamera.gameObject.SetActive(false);
    }

    private void EnablePerpCamera()
    {
        _perpCamera.gameObject.SetActive(true);
        _detectiveCamera.gameObject.SetActive(false);
    }

    public void DoneInterrogating()
    {
        //Player is done interrogatig the suspects and has declared who they beleive is the criminal
        //Clear the scene and wait for the results
        IsDone = true;
        DisableSuspectInterrogationPanel();
        DisableSuspectSelectPanel();
        EnableDetectiveCamera();

        SolveCase();
    }

    private void SolveCase()
    {
        string verdict = "";

        string userSuspect = _perpInput.text.Trim();

        string perpName = _caseDetails.GetPerpAndMotive()[0];
        Debug.Log(perpName + " " + userSuspect);
        string perpMotive = _caseDetails.GetPerpAndMotive()[1];
        
        if (perpName.Contains(userSuspect))
        {
            verdict = $"That's amazing detective. You cracked the case! {perpMotive}";
        }
        else
        {
            verdict = $"Unfortunately, you didn't get that one quite right. The actual perp is {perpName}.\r\n{perpMotive}";
        }

        DeclareResults(verdict);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    internal void DeclareResults(string content)
    {
        StartCoroutine(_caseFileReader.ReadText(content));
    }
}
