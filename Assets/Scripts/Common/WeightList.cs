using System.Collections.Generic;
using UnityEngine;

public class WeightList<T>
{
    private List<WeightListItem<T>> list;
    public int Sum { get; private set; }
    public int Count { get => list.Count; }

    public WeightList()
    {
        list = new List<WeightListItem<T>>();
    }

    public T ReturnWeightedRandom()
    {
        if (Sum == 0)
            return default;

        int weight = UnityEngine.Random.Range(1, Sum + 1);
        foreach (WeightListItem<T> x in list)
        {
            weight -= x.weight;
            if (weight <= 0)
                return x.item;
        }
        Debug.Log("Did not return proper item. Error in sum or list?");
        return list[UnityEngine.Random.Range(0, list.Count)].item;
    }

    public void Add(T item, int value)
    {
        list.Add(new WeightListItem<T>(item, value));
        Sum += value;
    }

    public void Clear()
    {
        list.Clear();
        Sum = 0;
    }

    public IEnumerator<WeightListItem<T>> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public class WeightListItem<T2>
    {
        public T2 item;
        public int weight;

        public WeightListItem(T2 i, int w)
        {
            item = i;
            weight = w;
        }
    }
}