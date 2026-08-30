using UnityEngine;
public enum ClueType {Letter, Newspaper, LightSource, Gun}
public class ClueInfo : MonoBehaviour
{
    public string ClueName;
    public ClueType type;

    [TextArea(3, 10)]
    public string clueDescription;

    public Sprite ClueImage;
}
