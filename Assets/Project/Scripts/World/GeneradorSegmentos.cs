using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorSegmentos : MonoBehaviour
{
    public GameObject[] prefabSegmento;
    public Transform posicionInicial;

    [SerializeField]
    private int rows = 3, columns = 3;

    [SerializeField]
    private float segmentDistance = 200f;

    // Start is called before the first frame update
    void Start()
    {
        posicionInicial.position = posicionInicial.position + new Vector3(-(segmentDistance * (columns - 1))/ 2f, 0f, -(segmentDistance * (rows - 1))/ 2f);

        for (int i = 0; i < (rows * columns); i++)
        {
            Vector3 pos = posicionInicial.position + new Vector3((i % columns) * segmentDistance, 0f, (i / columns) * segmentDistance);
            if ((pos - Vector3.zero).magnitude < segmentDistance * 2f)
            {
                Instantiate(prefabSegmento[Random.Range(0, 1)],
                        pos,
                        Quaternion.Euler(Vector3.up * (90f * Random.Range(0, 3))), posicionInicial);
            }
            else
            {
                Instantiate(prefabSegmento[Random.Range(0, prefabSegmento.Length)], 
                            pos, 
                            Quaternion.Euler(Vector3.up * (90f * Random.Range(0, 3))), posicionInicial);
            }
        }
    }

}
