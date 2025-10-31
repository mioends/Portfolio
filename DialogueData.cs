using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Game/Dialogue Data", order = 0)]
public class DialogueData : ScriptableObject
{
    public string npcName;
    public List<DialogueLine> lines = new List<DialogueLine>();
    public List<ResponseLine> responseLines = new List<ResponseLine>();
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string dialogueText;
}

[System.Serializable]
public class ResponseLine
{
    [TextArea(2, 5)]
    public string responseText;
}
