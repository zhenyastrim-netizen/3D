using System;
using UnityEngine;

[Serializable]
public class LootEntry
{
    public ItemData item;

    [Min(0.01f)]
    public float weight = 1f;

    [Min(1)]
    public int minimumAmount = 1;

    [Min(1)]
    public int maximumAmount = 1;
}