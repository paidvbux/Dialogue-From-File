using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadDialogueFromFileScript : MonoBehaviour
{
    [Serializable]
    public class TextRow
    {
        string name => value;

        public int indents;
        public string value;
        public bool isPlayerDialogue;
        
        public List<TextRow> followUpDialogue;


        public TextRow(int _indents, string _value, bool _isPlayerDialogue)
        {
            indents = _indents;
            value = _value;
            isPlayerDialogue = _isPlayerDialogue;
            followUpDialogue = new List<TextRow>();
        }
    }

    public TextAsset file;

    public List<TextRow> rows;
    public TextMeshProUGUI testText;

    string current;

    bool inputReceived;
    bool finishedLerpingText;

    //////////////////////////////////////////////////

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

            bool isValid = rows.Count - 1 != -1;
            bool isFollowUpDialogue = indents - 1 == (isValid ? rows[rows.Count - 1].indents : 0);

            bool isPlayerDialogue = value.StartsWith('!');

            TextRow textRow = new TextRow(indents, isPlayerDialogue ? value.Substring(1) : value, isPlayerDialogue);

            if (isValid && isFollowUpDialogue)
                rows[rows.Count - 1].followUpDialogue.Add(textRow);
            else
                rows.Add(textRow);
        }
    }

    IEnumerator RunFile()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            StartCoroutine(LerpText(current, rows[i].value, 0.1f, ChangeStringValue));
            if (rows[i].followUpDialogue.Count != 0)
                CreateResponses();
            bool isFinished = rows[i].followUpDialogue.Count == 0 ? finishedLerpingText : inputReceived && finishedLerpingText;
            yield return new WaitWhile(() => isFinished);
            current = "";
            finishedLerpingText = false;
        }
    }

    void CreateResponses()
    {

    }

    public IEnumerator LerpText(string a, string b, float t, Action<string> stringCallback)
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

            stringCallback(calculatedString);
            yield return new WaitForSeconds(t);
        }
        finishedLerpingText = true;
    }

    void ChangeStringValue(string value)
    {
        current = value;
        testText.text = current;
    }
    #endregion
}
