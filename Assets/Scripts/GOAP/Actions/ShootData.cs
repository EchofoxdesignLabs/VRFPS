using CrashKonijn.Goap.Classes.References;
using UnityEngine;
using VRDefender.GOAP.Actions;

public class ShootData : CommonData
{
    public static readonly int SHOOT = Animator.StringToHash("Shoot");
    [GetComponent] public Animator animator { get; set; }
}
