using System.Collections.Generic;

public class ActorTagData
{
    public HashSet<TagType> baseActorTags = new HashSet<TagType>();
    public HashSet<TagType> statusEffectTags = new HashSet<TagType>();
    private HashSet<TagType> cachedTagSet;
    public bool isDirty = false;
}