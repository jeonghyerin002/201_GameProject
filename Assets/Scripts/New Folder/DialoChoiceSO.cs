using UnityEngine;

[CreateAssetMenu(fileName = "DialoChoiceSO", menuName = "Dialog System/DialogChoiceSO")]
public class DialoChoiceSO : ScriptableObject
{
    public string text;
    public int nextId;
}
