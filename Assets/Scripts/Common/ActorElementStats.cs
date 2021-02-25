using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorElementStats
{
    private readonly Dictionary<ElementType, int> resists;
    private readonly Dictionary<ElementType, int> resistCaps;
    public readonly Dictionary<ElementType, int> negations;

    public ActorElementStats()
    {
        resists = new Dictionary<ElementType, int>();
        resistCaps = new Dictionary<ElementType, int>();
        negations = new Dictionary<ElementType, int>();
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            resists[element] = 0;
            resistCaps[element] = Helpers.DEFAULT_RESISTANCE_CAP;
            negations[element] = 0;
        }
    }

    public ActorElementStats(Dictionary<ElementType, int> value)
    {
        resists = new Dictionary<ElementType, int>();
        foreach (KeyValuePair<ElementType, int> e in value)
        {
            resists[e.Key] = e.Value;
        }
    }

    public int GetResistance(ElementType e)
    {
        return Math.Min(resists[e], resistCaps[e]);
    }

    public int GetUncapResistance(ElementType e)
    {
        return resists[e];
    }

    public void SetResistanceCap(ElementType e, int value)
    {
        resistCaps[e] = Math.Min(value, Helpers.HARD_RESISTANCE_CAP);
    }

    public int GetNegation(ElementType e)
    {
        return negations[e];
    }

    public void SetNegation(ElementType e, int value)
    {
        negations[e] = value;
    }

    public int this[ElementType i]
    {
        get { return GetResistance(i); }
        set { resists[i] = value; }
    }
}