using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadDialogueFromFileScript : MonoBehaviour
{
    #region Classes
    [Serializable]
    public class TextRow
    {
        public int indents;
        public string value;
        public bool isPlayerDialogue, waitWithClick;

        public List<TextRow> followUpDialogue;


        public TextRow(int _indents, string _value, bool _isPlayerDialogue, bool _waitWithClick)
        {
            indents = _indents;
            value = _value;
            isPlayerDialogue = _isPlayerDialogue;
            followUpDialogue = new List<TextRow>();
            waitWithClick = _waitWithClick;
        }
    }
    #endregion

    #region General Variables/Settings
    [Header("General Settings")]
    public string speakerName;
    public TextMeshProUGUI nameText, dialogueText;
    #endregion

    #region Dialogue Settings
    [Header("Dialogue Settings")]
    public TextAsset file;
    public List<TextRow> rows;

    public float waitTime = 0.5f, readSpeed = 0.05f;
    #endregion

    #region Hidden Variables
    string current;
    
    bool inputReceived;
    bool finishedLerpingText;
    #endregion

    ////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Unity Runtime Functions
    void Awake()
    {
        current = "";
        inputReceived = false;
        finishedLerpingText = false;

        ReadFile();
        StartCoroutine(RunFile());
    }
    #endregion

    #region Custom Functions
    void ReadFile()
    {
        string[] rowValues = file.text.Split("\n");

        for (int i = 0; i < rowValues.Length; i++)
        {
            string value = rowValues[i].Replace("\t", "");
            int indents = 0;
            int index = 0;

            while (true)
            {
                index = rowValues[i].IndexOf("\t", index);
                if (index == -1)
                    break;
                indents++;
                index++;
            }

            bool isPlayerDialogue = value.StartsWith('~');
            bool waitWithClick = (isPlayerDialogue ? value.Substring(1) : value).StartsWith('|');
            string finalValue = value.Substring((waitWithClick ? 1 : 0) + (isPlayerDialogue ? 1 : 0));

            TextRow textRow = new TextRow(indents, finalValue, isPlayerDialogue, waitWithClick);

            rows.Add(textRow);
        }
    }

    IEnumerator RunFile()
    {
        foreach (TextRow row in rows)
        {
            bool hasFollowup = row.followUpDialogue.Count != 0;
            nameText.text = row.isPlayerDialogue ? "You" : speakerName;
            StartCoroutine(LerpText(current, row.value, readSpeed));
            
            if (row.waitWithClick)
            {
                yield return new WaitWhile(() => {
                    bool waiting = (hasFollowup ? (inputReceived && finishedLerpingText && Input.GetKeyDown
                        (KeyCode.Mouse0)) : finishedLerpingText && Input.GetKeyDown(KeyCode.Mouse0));
                    return !waiting;
                });
            }
            else
            {
                yield return new WaitWhile(() => !(hasFollowup ? (inputReceived && finishedLerpingText) : finishedLerpingText));
                yield return new WaitForSeconds(waitTime);
            }

            current = "";
            finishedLerpingText = false;
            inputReceived = false;
        }
    }

    public IEnumerator LerpText(string a, string b, float t)
    {
        while (a != b)
        {
            string calculatedString;

            int length = a.Length + (a.Length < b.Length ? 1 : 0);
            if (b[length - 1] == '<')
            {
                int index = b.IndexOf('>', length);
                length = index + ((index == b.Length - 1) ? 1 : 2);
            }

            calculatedString = b.Substring(0, length);

            a = calculatedString;

            current = calculatedString;
            dialogueText.text = calculatedString;
            yield return new WaitForSeconds(t);
        }
        finishedLerpingText = true;
        inputReceived = true;
    }
    #endregion
}
