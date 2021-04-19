using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIGroupFormation
{
    HorizontalLine,
    VerticalLine,
    Block,
    Spike
}
public class AIFormationFactory : MonoBehaviour
{
    public static AIFormation GetFormation(AIGroupFormation formation, int membersCount)
    {
        if (membersCount == 0)
            return new AIFormation(Vector2Int.zero, Vector2Int.zero);

        switch (formation)
        {
            //case AIGroupFormation.VerticalLine:
            //    AIFormation v_f = new AIFormation(new Vector2Int(1, membersCount), new Vector2Int(Mathf.CeilToInt(membersCount / 2), 0));
                
            //    for (int i = 0; i < v_f.Grid.GetLength(1); i++)
            //    {
            //        v_f.Grid[0, i] = i;
            //    }
            //    return v_f;
            //case AIGroupFormation.HorizontalLine:
            //    AIFormation h_f = new AIFormation(new Vector2Int(membersCount, 1), new Vector2Int(0, Mathf.CeilToInt(membersCount / 2)));
                
            //    for (int i = 0; i < h_f.Grid.GetLength(0); i++)
            //    {
            //        h_f.Grid[i, 0] = i;
            //    }
            //    return h_f;
            //case AIGroupFormation.Block:
            //    AIFormation b_f = new AIFormation(new Vector2Int(Mathf.CeilToInt(membersCount / 2), membersCount / Mathf.CeilToInt(membersCount / 2))
            //        , new Vector2Int(Mathf.CeilToInt(membersCount / 2), 0));
                
            //    for (int i = 0; i < b_f.Grid.GetLength(0); i++)
            //    {
            //        for (int j = 0; j < b_f.Grid.GetLength(1); j++)
            //        {
            //            b_f.Grid[i, j] = i + j;
            //        }

            //    }
            //    return b_f;
            //case AIGroupFormation.Spike:
            //    AIFormation s_f = new AIFormation(new Vector2Int(membersCount, Mathf.FloorToInt(membersCount / 2) + 1)
            //        , new Vector2Int(Mathf.CeilToInt(membersCount / 2), 0));
                
            //    var middleX = Mathf.CeilToInt(s_f.Grid.GetLength(0) / 2);
            //    var posIndex = 0;
            //    s_f.Grid[middleX, 0] = posIndex++;
            //    for (int j = 1; j < s_f.Grid.GetLength(1); j++)
            //    {
            //        s_f.Grid[middleX + j, j] = posIndex++;
            //        s_f.Grid[middleX - j, j] = posIndex++;

            //    }
            //    return s_f;
            default:
                return new AIFormation(Vector2Int.zero, Vector2Int.zero);
        }
    }

}
