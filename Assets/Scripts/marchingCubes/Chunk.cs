using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MarchingCubes
{
    public class Chunk
    {

        public List<Vector3> vertices;
        List<int> triangles;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider collider;

        GameObject chunkObject;
        Vector3Int chunkPosition;
        WorldGenerator world;

        float [,,] terrainMap;
        float isoSurface = 0.0005f;


        public Chunk(Vector3Int position, WorldGenerator world) {
            this.world = world;
            chunkObject = new GameObject();
            chunkPosition = position;
            chunkObject.transform.position = chunkPosition;
            chunkObject.transform.tag = "Terrain";
            chunkObject.transform.parent = world.transform;

            vertices = new List<Vector3>();
            triangles = new List<int>();
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            collider = chunkObject.AddComponent<MeshCollider>();
            meshRenderer.material = Resources.Load<Material>("Materials/grassTerrain");
            

            
            
            chunkObject.name = $"chunk({chunkPosition.x},{chunkPosition.y},{chunkPosition.z})";
            terrainMap = new float[Tables.ChunkWidth, Tables.ChunkHeight, Tables.ChunkWidth];
            
            
            PopulateChunkValues();
            //GetVertices();
            GenerateWorld();
            generateMesh();
        }

        void GenerateWorld(){
            float [,,] tempChunkData = new float[Tables.ChunkWidth + 1, Tables.ChunkHeight + 1 , Tables.ChunkWidth + 1];

            for(int x = 0; x <= Tables.ChunkWidth; x++)
            for(int y = 0; y <= Tables.ChunkHeight; y++)
            for(int z = 0; z <= Tables.ChunkWidth; z++)
            {
                tempChunkData[x, y, z] = MapGenerator.perlin3d(
                    (float)x / Tables.ChunkWidth + (chunkPosition.x / Tables.ChunkWidth),
                    (float)y / Tables.ChunkWidth + (chunkPosition.y),
                    (float)z / Tables.ChunkWidth + (chunkPosition.z / Tables.ChunkWidth)
                );
            }
            for(int x = 0; x < Tables.ChunkWidth; x++)
            for(int y = 0; y < Tables.ChunkHeight; y++)
            for(int z = 0; z < Tables.ChunkWidth; z++)
            {
                float[] cube = new float[8];
                for(int i = 0; i < 8; i++)
                {
                    Vector3Int corner = new Vector3Int(x, y, z) + Tables.cubeTable[i];
                    cube[i] = tempChunkData[corner.x, corner.y, corner.z];
                    
                }
                marchingCalc(new Vector3(x, y, z), cube);
            }
            
        }


        void PopulateChunkValues(){
            for(int x = 0; x < Tables.ChunkWidth; x++)
            {
                for(int y = 0; y < Tables.ChunkHeight; y++)
                {
                    for(int z = 0; z < Tables.ChunkWidth; z++)
                    {
                        terrainMap[x, y, z] = MapGenerator.perlin3d(
                            (float)x / Tables.ChunkWidth + (chunkPosition.x / Tables.ChunkWidth),
                            (float)y / Tables.ChunkWidth + (chunkPosition.y),
                            (float)z / Tables.ChunkWidth + (chunkPosition.z / Tables.ChunkWidth)
                        );
                    }
                }
            }
        }
        void GetVertices(){
            for(int x = 0; x < Tables.ChunkWidth; x++)
            {
                for(int y = 0; y < Tables.ChunkHeight; y++)
                {
                    for(int z = 0; z < Tables.ChunkWidth; z++)
                    {
                        float[] cube = new float[8];
                        bool ignore = false; //if no outside value ignore this block
                        for(int i = 0; i < 8; i++)
                        {
                            Vector3Int corner = new Vector3Int(x, y, z) + Tables.cubeTable[i];
                            if (corner.x >= Tables.ChunkWidth  ||
                                corner.y >= Tables.ChunkHeight ||
                                corner.z >= Tables.ChunkWidth  )
                            {
                                // TODO: Make a better solution for this calculation
                                //calculate the next chunk position
                                //if value is out of chunk, subtract by 1
                                var nextChunkPos = new Vector3Int(
                                    chunkPosition.x + corner.x >= Tables.ChunkWidth? chunkPosition.x + corner.x : chunkPosition.x,
                                    chunkPosition.y + corner.y >= Tables.ChunkHeight? chunkPosition.y + corner.y : chunkPosition.y,
                                    chunkPosition.z + corner.z >= Tables.ChunkWidth? chunkPosition.z + corner.z : chunkPosition.z);
                                
                                var nextChunk = world.GetChunkFromVector3(nextChunkPos);
                                if (nextChunk == null){
                                    ignore = true;
                                    break;
                                }

                                cube[i] = nextChunk.terrainMap[
                                    corner.x >= Tables.ChunkWidth? 0 : corner.x,
                                    corner.y >= Tables.ChunkHeight? 0 : corner.y,
                                    corner.z >= Tables.ChunkWidth? 0 : corner.z
                                ];

                            }
                            else
                            {
                                cube[i] = terrainMap[corner.x, corner.y, corner.z];
                            }
                            
                        }

                        if(!ignore)
                            marchingCalc(new Vector3(x, y, z), cube);
                    }
                }
            }
        }

        int GetCubeConfiguration(float[] cube)
        {
            // loop the 8 vertices of cube
            int cubeIndex = 0;
            for(int i = 0; i < 8; i++)
            {
                if(cube[i] > isoSurface)
                    cubeIndex |= 1 << i;
            }
            
            return cubeIndex;
        }
        void marchingCalc(Vector3 position, float[] cube)
        {
            
            Vector3[] vertlist = new Vector3[12];

            int cubeIndex = GetCubeConfiguration(cube);
            

            if (cubeIndex <= 0 || cubeIndex >= 255)
                return;
            

                
            for(int i = 0; Tables.triTable[cubeIndex, i] != -1; i++)
            {
                int indiceVertice = Tables.triTable[cubeIndex, i];

                //No interpolation
                //Vector3 vertice1 = position + Tables.cubeTable[Tables.edgeTable[indiceVertice, 0]];
                //Vector3 vertice2 = position + Tables.cubeTable[Tables.edgeTable[indiceVertice, 1]];
                
                //Vector3 vertPosition = (vertice1 + vertice2) / 2f;
                
                //with interpolation
                Vector3 vertPosition = VertexInterp(
                    isoSurface ,
                    position + Tables.cubeTable[Tables.edgeTable[indiceVertice, 0]],
                    position + Tables.cubeTable[Tables.edgeTable[indiceVertice, 1]],
                    cube[Tables.edgeTable[indiceVertice, 0]],
                    cube[Tables.edgeTable[indiceVertice, 1]] );
                
                int repeatedVertice = vertices.IndexOf(vertPosition);
                if(repeatedVertice < 0){
                    vertices.Add(vertPosition);
                    triangles.Add(vertices.Count -1);

                }
                else{
                    triangles.Add(repeatedVertice);
                }
                
            }
        }

        
        Vector3 VertexInterp(float isolevel,Vector3 p1, Vector3 p2, float valp1, float valp2)
        {
            float mu;
            Vector3 p;
            
            if (Mathf.Abs(isolevel - valp1) < 0.00001)
                return p1;
            if (Mathf.Abs(isolevel-valp2) < 0.00001)
                return p2;
            if (Mathf.Abs(valp1-valp2) < 0.00001)
                return p1;
            mu = (isolevel - valp1) / (valp2 - valp1);
            p.x = p1.x + mu * (p2.x - p1.x);
            p.y = p1.y + mu * (p2.y - p1.y);
            p.z = p1.z + mu * (p2.z - p1.z);

            return p;

        }//vertexInterp


        void ClearData(){
            this.vertices.Clear();
            this.triangles.Clear();
        }
        
        void generateMesh(){
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            //mesh.Optimize();
            NormalSolver.RecalculateNormals(mesh, 45);
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            collider.sharedMesh = mesh;
            
        }

        public void UpdateChunk(){
            ClearData();
            GetVertices();
            generateMesh();
            world.PrintMessage($"Updated!{chunkPosition}");
        }

        public void AddVoxel(Vector3 pos){
            Vector3Int vecInt = new Vector3Int(
                Mathf.FloorToInt(pos.x),
                Mathf.FloorToInt(pos.y),
                Mathf.FloorToInt(pos.z)
            );
            vecInt -= chunkPosition;
            //world.PrintMessage(vecInt.ToString());
            terrainMap[vecInt.x, vecInt.y, vecInt.z] = 0f;
            
            ClearData();
            GetVertices();
            generateMesh();

        }
        public void AddVoxel(Vector3[] positions){
            foreach (Vector3 pos in positions)
            {
                Vector3Int vecInt = new Vector3Int(
                    Mathf.CeilToInt(pos.x),
                    Mathf.CeilToInt(pos.y),
                    Mathf.CeilToInt(pos.z)
                );
                vecInt -= chunkPosition;
                
                if(isInsideChunk(vecInt))
                    terrainMap[vecInt.x, vecInt.y, vecInt.z] = 0f;
            }
            ClearData();
            GetVertices();
            generateMesh();

        }
        public void RemoveVoxel(Vector3 pos){
            Vector3Int vecInt = new Vector3Int(
                Mathf.FloorToInt(pos.x),
                Mathf.FloorToInt(pos.y),
                Mathf.FloorToInt(pos.z)
            );
            vecInt -= chunkPosition;

            terrainMap[vecInt.x, vecInt.y, vecInt.z] = 1f;
            
            ClearData();
            GetVertices();
            generateMesh();
        }
        

        bool isInsideChunk(Vector3 pos){
            if(
                pos.x < Tables.ChunkWidth + 1 && 
                pos.y < Tables.ChunkHeight + 1 &&
                pos.z < Tables.ChunkWidth + 1
            )
                return true;
            else
                return false;
        }
    }//class Chunk

    
}
