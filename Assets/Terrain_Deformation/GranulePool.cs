using System.Collections.Generic;
using UnityEngine;

public class GranulePool : MonoBehaviour
{
    public GameObject granulePrefab;
    public int initialPoolSize = 400;

    private List<GameObject> pool = new List<GameObject>();
    private static int count = 0;

    void Start()
    {
      
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject granule = Instantiate(granulePrefab);
            granule.SetActive(false);
            pool.Add(granule);
        }
    }

    // Метод для получения гранулы из пула
    public GameObject GetGranule()
    {
        foreach (GameObject granule in pool)
        {
            if (granule!=null && !granule.activeSelf)
            {
                count++;
                granule.SetActive(true);
                return granule;
            }
        }

        // Если в пуле нет свободных объектов, создаем новую гранулу
        GameObject newGranule = Instantiate(granulePrefab);
        pool.Add(newGranule);
        return newGranule;
        
    }

   
    public void ReturnGranule(GameObject granule)
    {
        count--;
        granule.SetActive(false);
    }
}
