using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/FishData", fileName = "FishData", order = 1)]
public class FishData : ScriptableObject
{
    public float traceDistance = 5f;
    public float damage = 20f;
    public float speed = 2f;
}
