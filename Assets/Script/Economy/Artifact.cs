using UnityEngine;

//
[CreateAssetMenu(menuName = "ScriptableObjects/Item/Artifact")]
public class Artifact : BaseItem
{
    [Header("=== Artifact type ==========")]
    public ArtifactType type = ArtifactType.Forest;

    public override int MaxStackable => 1;
}
