using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDialogueFromFileScript : MonoBehaviour
{
    [System.Serializable]
    public class TextRow
    {
        public int indents;
        public string value;
        public bool isPlayerDialogue;
    
        public TextRow(int _indents, string _value, bool _isPlayerDialogue)
        {
            indents = _indents;
            value = _value;
            isPlayerDialogue = _isPlayerDialogue;
        }
    }

    public TextAsset file;

    public List<TextRow> rows;

    void Awake()
    {
        string[] rowValues = file.text.Split("\n"); 

        foreach (string rowValue in rowValues)
        {
            string value = rowValue.Replace("\t", "");
            int indents = 0;
            int index = 0;
            while (index <= rowValue.Length - 1)
            {
                index = rowValue.IndexOf('\t', index);
                if (index == -1)
                    return;
                indents++;
            }

            rows.Add(new TextRow(indents, value, value.StartsWith("!")));
            print(rowValue);
        }
    }
}
