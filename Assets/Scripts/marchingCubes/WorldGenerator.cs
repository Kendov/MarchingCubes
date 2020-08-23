using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MarchingCubes;

public class WorldGenerator : MonoBehaviour
{

    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    private void Start() {

        Generate();

    }

    void Generate(){
        for(int x = 0; x < Tables.worldSizeInChunks; x++){
            //for(int y = 0; y < Tables.worldSizeInChunks; y++){
                for(int z = 0; z < Tables.worldSizeInChunks; z++){
                    Vector3Int chunkPos = new Vector3Int(x * Tables.ChunkWidth, 0, z * Tables.ChunkWidth);
                    chunks.Add(chunkPos, new Chunk(chunkPos, this));
                }
            //}
        }
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {

        if(!isInsideWorld(pos))
            return null;
        print(pos);
        return chunks[ConvetToChunkPosition(new Vector3Int((int)pos.x, 0, (int)pos.z))];
        
    }

    bool isInsideWorld(Vector3 pos){
        if(
            pos.x < Tables.worldSizeInChunks * Tables.ChunkWidth && 
            pos.y < Tables.worldSizeInChunks * Tables.ChunkHeight &&
            pos.z < Tables.worldSizeInChunks * Tables.ChunkWidth
        )
            return true;
        else
            return false;
    }
    Vector3Int ConvetToChunkPosition(Vector3Int pos)
    {
        if (pos == Vector3.zero)
       	  return pos;

    	int remainderX = pos.x % Tables.ChunkWidth;
    	int remainderY = pos.y % Tables.ChunkWidth;
    	int remainderZ = pos.z % Tables.ChunkWidth;
    	

    	return new Vector3Int(
            remainderX == 0 ? pos.x : pos.x - remainderX,
            remainderY == 0 ? pos.y : pos.y - remainderY,
            remainderZ == 0 ? pos.z : pos.z - remainderZ
        );
    }
}


[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

}
