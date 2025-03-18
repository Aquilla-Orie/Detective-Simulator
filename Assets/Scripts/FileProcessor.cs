using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileProcessor : MonoBehaviour
{
    public delegate void OnReadComplete();
    public delegate void OnWriteComplete();

    public static event OnReadComplete onReadComplete;
    public static event OnWriteComplete onWriteComplete;
    static string _filepath;


    private void OnEnable()
    {
        FileProcessor.onWriteComplete += ReadFile;
        _filepath = AppDomain.CurrentDomain.BaseDirectory;
    }

    private void OnDisable()
    {
        FileProcessor.onReadComplete -= ReadFile;
    }

    public static void WriteToFile(string data)
    {
        //Store the Crime Scenario in a file
        FileStream fsWrite = new FileStream($"{_filepath}\\CaseText.txt",
                     FileMode.Create, FileAccess.Write, FileShare.None);
        //FileStream fsWrite = new FileStream(@"D:\Documents\CaseText.txt",
        //             FileMode.Create, FileAccess.Write, FileShare.None);
        string text = data.Replace("*", "");
        byte[] textArray = Encoding.UTF8.GetBytes(text);
        fsWrite.Write(textArray, 0, textArray.Length);
        fsWrite.Close();

        //Done Writing file copy
        //Read file copy and setup game
        onWriteComplete?.Invoke();
    }
    public static void ReadFile()
    {
        Debug.Log("File processor reading file");
        string[] caseInfo = new string[3];
        string[] victimDetails = new string[4];

        int numOfSuspects;

        Dictionary<string, List<string>> suspectDetails = new Dictionary<string, List<string>>();

        string[] perpAndMotive = new string[2];

        string fileName = $"{_filepath}\\CaseText.txt";
        //string fileName = @"D:\Documents\CaseText.txt";

        using FileStream fs = File.OpenRead(fileName);
        using var sr = new StreamReader(fs);

        string line;

        List<string> lines = new List<string>();

        while ((line = sr.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            lines.Add(line);
        }

        for (int i = 0; i < lines.Count; i++)
        {
            var brokenLine = lines[i].Split(' ');
            if (brokenLine[0].Equals("Case"))
            {
                //Get the case details
                caseInfo[0] = brokenLine[2];
                caseInfo[1] = lines[i].Split(':')[1].Trim();
            }
            if (brokenLine[0].Equals("Incidence"))
            {
                //Incidence overview may be broken into two paragraphs
                var l = lines[i];

                if (String.IsNullOrWhiteSpace(l.Split(':')[1]))//The actual paragraph is on a new line
                    caseInfo[2] = lines[i+1];
                else
                    caseInfo[2] = lines[i];
            }
            if (brokenLine[0].Contains("V"))
            {

                victimDetails[0] = lines[i + 1].Split(':')[1].Trim();
                victimDetails[1] = lines[i + 2].Split(':')[1].Trim();
                victimDetails[2] = lines[i + 3].Split(':')[1].Trim();
                victimDetails[3] = lines[i + 4].Split(':')[1].Trim();

                i += 4;
            }
            if (brokenLine[0].Equals("Suspects:"))
            {
                int.TryParse(lines[i].Split(':')[1].Trim(), out numOfSuspects);
                if (numOfSuspects <= 0)
                {
                    int.TryParse(lines[i-1].Split(':')[1].Trim(), out numOfSuspects);
                }
                for (int a = 1; a <= numOfSuspects; a++)
                {
                    string firstLine = lines[i + 1];
                    //ChatGPT did some type of numbering above the suspect name. We don't need it
                    if (!firstLine.Contains("Name"))
                        i++;

                    string suspectName = lines[i + 1].Split(':')[1].Trim();
                    string suspectBackground = lines[i + 2].Split(':')[1].Trim();
                    string suspectPersonalStory = lines[i + 3].Split(':')[1].Trim();
                    string suspectGender = lines[i + 4].Split(':')[1].Trim();

                    i += 4;
                    suspectDetails.Add(suspectName, new List<string> { suspectBackground, suspectPersonalStory, suspectGender });

                }
            }
            if (brokenLine[0].Equals("Perpetrator:"))
            {
                perpAndMotive[0] = lines[i].Split(':')[1].Trim();
                perpAndMotive[1] = lines[i+1].Split(':')[1].Trim();
            }
        }

        CaseDetails caseDetails = new CaseDetails();

        caseDetails.SetCaseInfo(caseInfo);
        caseDetails.SetVictimDetails(victimDetails);
        caseDetails.SetSuspectDetails(suspectDetails);
        caseDetails.SetPerpAndMotive(perpAndMotive);

        CaseManager.Instance.SetCaseDetails(caseDetails);


        onReadComplete?.Invoke();

        Debug.Log("File processor finished reading file");

        //Set Everything in the Case Manager

        //Setup Scene
        //CaseManager.Instance.SetupScene();
    }
}
