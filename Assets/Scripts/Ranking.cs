using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Ranking : NetworkBehaviour
{
    public float refreshTime;
    public List<Transform> rankingList;

    private void Start()
    {
        if (isClient)
        {
            StartCoroutine(UpdateRanking());
        }
    }

    private IEnumerator UpdateRanking()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshTime);
            PlayerInfo[] playersArray = FindObjectsOfType<PlayerInfo>();
            List<PlayerInfo> playersList = new List<PlayerInfo>(playersArray);
            playersList.RemoveAll((p) => !p.gameObject.GetComponent<PlayerHealth>().isAlive);
            playersList.Sort((p1, p2) => p2.killCount.CompareTo(p1.killCount));
            for (int i = 0; i < 10; i++)
            {
                if (i < playersList.Count)
                {
                    rankingList[i].Find("Rank").GetComponent<Text>().text = (i + 1).ToString();
                    rankingList[i].Find("Name").GetComponent<Text>().text = playersList[i].playerName;
                    rankingList[i].Find("Kills").GetComponent<Text>().text = playersList[i].killCount.ToString();
                }
                else
                {
                    rankingList[i].Find("Rank").GetComponent<Text>().text = string.Empty;
                    rankingList[i].Find("Name").GetComponent<Text>().text = string.Empty;
                    rankingList[i].Find("Kills").GetComponent<Text>().text = string.Empty;
                }
            }
        }
    }

}
