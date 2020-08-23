using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMapGenerator : MonoBehaviour
{
    [SerializeField]float perlinMask = 0.5f; // 0.0015
    [SerializeField]float frequency = 1.0f; // 1
    [SerializeField]float xoffset = 0f;
    [SerializeField]float zoffset = 0f;


    int chunkSize = 20;
    private void OnDrawGizmos() {

        drawChunck();

    }

    private void drawChunck()
    {
        for (int x = 0; x < chunkSize + 1; x++)
        {
            for (int y = 0; y < chunkSize + 1; y++)
            {
                for (int z = 0; z < chunkSize + 1; z++)
                {
                    Gizmos.color = new Color(1,1,1,0.1f);
                    if(MapGenerator.perlin3d((float)x / chunkSize * frequency + xoffset , (float)y / chunkSize * frequency, (float)z / chunkSize * frequency + zoffset) > perlinMask)
                        Gizmos.color = Color.red;

                    Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * 0.5f);

                }
            }
        }
    }



}