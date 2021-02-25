using System.Collections.Generic;
using System.Linq;

public class StatBonus
{
    public float FlatModifier { get; private set; }
    public int AdditiveModifier { get; private set; }
    public List<float> MultiplyModifiers { get; private set; }
    public float CurrentMultiplier { get; private set; }
    public bool isStatOutdated;
    public bool HasFixedModifier { get => FixedModifier.Count > 0; }
    public List<float> FixedModifier { get; private set; }

    public StatBonus()
    {
        FlatModifier = 0;
        AdditiveModifier = 0;
        MultiplyModifiers = new List<float>();
        CurrentMultiplier = 1.00f;
        FixedModifier = new List<float>();
        isStatOutdated = true;
    }

    public StatBonus(StatBonus old)
    {
        FlatModifier = old.FlatModifier;
        AdditiveModifier = old.AdditiveModifier;
        MultiplyModifiers = old.MultiplyModifiers.ToList();
        CurrentMultiplier = old.CurrentMultiplier;
        FixedModifier = old.FixedModifier.ToList();
        isStatOutdated = old.isStatOutdated;
    }

    public void ResetBonus()
    {
        FlatModifier = 0;
        AdditiveModifier = 0;
        MultiplyModifiers.Clear();
        CurrentMultiplier = 1.00f;
        FixedModifier.Clear();
        isStatOutdated = true;
    }

    public bool IsZero()
    {
        if (HasFixedModifier == true)
            return false;
        if (AdditiveModifier != 0 || MultiplyModifiers.Count != 0 || FlatModifier != 0)
            return false;
        return true;
    }

    public void AddBonuses(StatBonus otherBonus, bool overwriteFixed = true)
    {
        if (otherBonus == null)
            return;

        FlatModifier += otherBonus.FlatModifier;
        AdditiveModifier += otherBonus.AdditiveModifier;
        MultiplyModifiers.AddRange(otherBonus.MultiplyModifiers);
        FixedModifier.AddRange(otherBonus.FixedModifier);
        FixedModifier.OrderBy(x => x);
        UpdateCurrentMultiply();
    }

    public void AddBonus(ModifyType type, float value)
    {
        switch (type)
        {
            case ModifyType.FlatAddition:
                AddToFlat(value);
                return;

            case ModifyType.Additive:
                AddToAdditive((int)value);
                return;

            case ModifyType.Multiply:
                AddToMultiply(value);
                return;

            case ModifyType.FixedToValue:
                AddFixedBonus(value);
                return;
        }
    }

    public void RemoveBonus(ModifyType type, float value)
    {
        switch (type)
        {
            case ModifyType.FlatAddition:
                AddToFlat(-value);
                return;

            case ModifyType.Additive:
                AddToAdditive((int)-value);
                return;

            case ModifyType.Multiply:
                RemoveFromMultiply(value);
                return;

            case ModifyType.FixedToValue:
                RemoveFixedBonus(value);
                return;
        }
    }

    public void SetFlat(int value)
    {
        FlatModifier = value;
        isStatOutdated = true;
    }

    public void SetAdditive(int value)
    {
        AdditiveModifier = value;
        isStatOutdated = true;
    }

    private void AddFixedBonus(float value)
    {
        FixedModifier.Add(value);
    }

    private void RemoveFixedBonus(float value)
    {
        FixedModifier.Remove(value);
    }

    private void AddToFlat(float value)
    {
        FlatModifier += value;
        isStatOutdated = true;
    }

    private void AddToAdditive(int value)
    {
        AdditiveModifier += value;
        isStatOutdated = true;
    }

    private void AddToMultiply(float value)
    {
        MultiplyModifiers.Add(value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    private void RemoveFromMultiply(float value)
    {
        MultiplyModifiers.Remove(value);
        UpdateCurrentMultiply();
        isStatOutdated = true;
    }

    public void UpdateCurrentMultiply()
    {
        float mult = 1.0f;
        foreach (float i in MultiplyModifiers)
            mult *= 1f + i / 100f;
        CurrentMultiplier = mult;
    }

    public int CalculateStat(int stat)
    {
        return (int)CalculateStat((float)stat);
    }

    public float CalculateStat(float stat)
    {
        isStatOutdated = false;
        if (HasFixedModifier)
        {
            return FixedModifier[0];
        }
        return (stat + FlatModifier) * (1f + AdditiveModifier / 100f) * CurrentMultiplier;
    }
}

public class StatBonusCollection
{
    private Dictionary<TagType, StatBonus> statBonuses;

    public StatBonusCollection()
    {
        statBonuses = new Dictionary<TagType, StatBonus>();
    }

    public bool IsEmpty()
    {
        if (statBonuses.Count == 0)
            return true;
        else
            return false;
    }

    public IEnumerable<TagType> GetTagTypeIntersect(IEnumerable<TagType> types)
    {
        return statBonuses.Keys.Intersect(types);
    }

    public void AddBonus(TagType restrictionType, ModifyType modifyType, float value)
    {
        if (!statBonuses.ContainsKey(restrictionType))
            statBonuses[restrictionType] = new StatBonus();
        statBonuses[restrictionType].AddBonus(modifyType, value);
    }

    public bool RemoveBonus(TagType restrictionType, ModifyType modifyType, float value)
    {
        if (statBonuses.ContainsKey(restrictionType))
        {
            statBonuses[restrictionType].RemoveBonus(modifyType, value);
            if (statBonuses[restrictionType].IsZero())
                statBonuses.Remove(restrictionType);
            return true;
        }
        else
            return false;
    }

    public StatBonus GetStatBonus(TagType restriction)
    {
        if (statBonuses.ContainsKey(restriction))
            return statBonuses[restriction];
        else
            return null;
    }

    public StatBonus GetTotalStatBonus(IEnumerable<TagType> tagTypes)
    {
        StatBonus returnBonus = new StatBonus();

        if (tagTypes == null)
            return returnBonus;

        var intersectingTypes = GetTagTypeIntersect(tagTypes);
        if (intersectingTypes.Count() == 0)
            return returnBonus;

        foreach (TagType type in intersectingTypes)
        {
            returnBonus.AddBonuses(statBonuses[type]);
        }
        returnBonus.UpdateCurrentMultiply();

        return returnBonus;
    }

    public Dictionary<TagType, StatBonus> GetAllStatBonus()
    {
        return statBonuses;
    }
}