using UnityEngine;

[CreateAssetMenu(menuName = "Noir/Evidence Item")]
public class EvidenceItem : ScriptableObject
{
    public string displayName;
    public Sprite icon;

    [TextArea(3, 8)]
    public string description;
}
