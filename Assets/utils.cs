using UnityEngine;
using System.Collections.Generic;

public class utils
{
    public static void arrangeObjectsOnWidth(System.Collections.Generic.List<GameObject> players, float offsetFromCenter)
    {
        Vector3 bgSize = GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().bounds.size;
        float totalSpace = bgSize.x;
        float distributedSpace = totalSpace / (players.Count * 2);

        foreach (GameObject g in players)
            g.transform.position = new Vector3((distributedSpace * (players.IndexOf(g) * 2 + 1)) - (totalSpace / 2), offsetFromCenter, 0);
    }
    public static List<Vector3> objectSpacings(int length, float spacing)
    {
        int halfL = length / 2;
        List<float> list = new List<float>();
        int i = 0;
        while (list.Count < length)
        {
            if (list.Count == 0)
            {
                if (length % 2 == 0)
                {
                    list.Add(-(float)spacing / 2);
                    list.Add((float)spacing / 2);
                }
                else
                    list.Add(0f);
            }
            else
            {
                list.Add(list[list.Count - 1] + spacing);
                list.Insert(0, list[0] - spacing);
            }
            i++;
        }
        List<Vector3> positions = new List<Vector3>();
        list.ForEach(x => positions.Add(new Vector3(x, 0, 0)));
        positions.Reverse();
        return positions;
    }

    internal static List<int> getSetOfRandomNumbers(int cardsLength, int amount)
    {
        HashSet<int> set = new HashSet<int>();
        while (set.Count < amount)
        {
            set.Add(Random.Range(0, cardsLength));
        }
        return new List<int>(set);
    }
}