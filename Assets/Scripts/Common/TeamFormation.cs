using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamFormation
{
    public List<Actor> frontLine = new List<Actor>();
    public List<Actor> backLine = new List<Actor>();

    public void AddToFrontLine(Actor actor)
    {
        if (frontLine.Count > 3)
            return;
        else
            frontLine.Add(actor);
    }

    public void AddToBackLine(Actor actor)
    {
        if (backLine.Count > 3)
            return;
        else
            backLine.Add(actor);
    }
}
