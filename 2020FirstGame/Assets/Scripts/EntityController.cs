using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{

    public Dictionary<Tuple<int, int>, List<Entity>> chunkEntities =
            new Dictionary<Tuple<int, int>, List<Entity>>();

    HashSet<Tuple<Entity, Tuple<int, int>>> entitiesWhichChangedChunk = 
        new HashSet<Tuple<Entity, Tuple<int, int>>>();

    //stores pointers to the entities in chunkEntities
    //public List<Tuple<Tuple<int, int>, int>> activeEntities = new List<Tuple<Tuple<int, int>, int>>();

    public GameObject sheep;
    public GameObject miner;

    public TerrainController terrainController;

    public Dictionary<string, GameObject> objNames;

    private void Start()
    {
        objNames = new Dictionary<string, GameObject>
        {
            ["sheep"] = sheep,
            ["miner"] = miner
        };
    }


    public class Entity
    {
        //holds data about an entity for when it is loaded after being unloaded
        public float x;
        public float y;
        public Tuple<int, int> chunk;
        public int chunkEntityListIndex;
        public GameObject obj;
        public string objName;
        public Func<bool> step;

        public Entity(float x, float y, Tuple<int, int> chunk, GameObject obj, Func<bool> step, string objName)
        {
            this.x = x;
            this.y = y;
            this.chunk = chunk;
            this.obj = obj;
            this.step = step;
            this.objName = objName;
        }
    }

    public GameObject GetEntityAtPosition(Vector3 position)
    {
        GameObject obj = null;
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null)
        {
            obj = hit.collider.gameObject;
        }
        else
        {
            /*//NOT IMPLEMENTED because I think having colliders and doing raycasts is better
            Tuple<int, int> chunk = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
            if (chunkEntities.ContainsKey(chunk))
            {

            }//*/
        }
        return obj;
    }

    public void AddBunchOfEntities(int centreX, int centreY, string objName, float radius, float density, float bias=2f)
    {
        //adds a bunch of entities
        int numEntities = Mathf.RoundToInt(density * (radius * radius));
        float x;
        float y;

        for (int i=0; i<numEntities; i++)
        {
            x = centreX + (Mathf.Pow(UnityEngine.Random.Range(0f, 1f), bias) * 2 - 1) * radius;
            y = centreY + (Mathf.Pow(UnityEngine.Random.Range(0f, 1f), bias) * 2 - 1) * radius;
            AddEntity(x, y, objName);
        }
    }

    public void AddEntity(float x, float y, string objName)
    {
        //add to array but don't load
        Tuple<int, int> chunk = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

        
        Entity entity = new Entity(x, y, chunk, null, null, objName);

        if (!chunkEntities.ContainsKey(chunk))
        {
            chunkEntities[chunk] = new List<Entity>();
        }
        chunkEntities[chunk].Add(entity);//*/

        if (terrainController.terrainArray.nextLoadedChunks.Contains(entity.chunk) || terrainController.terrainArray.loadedChunks.Contains(entity.chunk))
        {
            //adding entity to chunk that is already loaded, so entity will stay unloaded unless we load it now
            LoadEntity(entity);
        }
    }

    public void RemoveEntity(GameObject obj)
    {
        //search through relevant chunk for the object

        //(NOT IMPLEMENTED)
    }

    public void UnloadChunkEntities(Tuple<int, int> chunk)
    {
        if (chunkEntities.ContainsKey(chunk))
        {
            foreach (Entity entity in chunkEntities[chunk])
            {
                UnloadEntity(entity);
            }
        }
    }

    public void LoadChunkEntities(Tuple<int, int> chunk)
    {
        if (chunkEntities.ContainsKey(chunk))
        {
            foreach (Entity entity in chunkEntities[chunk])
            {
                LoadEntity(entity);
            }
        }
    }
    public void UnloadEntity(Entity entity)
    {
        if (entity.obj == null)
        {
            Debug.Log("not loaded");
        }
        else
        {
            Vector2 position = entity.obj.transform.position;
            entity.x = position.x;
            entity.y = position.y;

            entity.obj.GetComponent<EntityScript>().selfEntity = null;
            Destroy(entity.obj);
            entity.obj = null;
        }
    }

    public void LoadEntity(Entity entity)
    {
        Vector2 position = new Vector2(entity.x, entity.y);

        if (entity.obj != null)
        {
            Debug.Log("duplicate");
        } else
        {
            entity.obj = Instantiate(objNames[entity.objName], position, Quaternion.identity);
        }
        
        entity.step = entity.obj.GetComponent<EntityScript>().step;
        entity.obj.GetComponent<EntityScript>().selfEntity = entity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (terrainController.isGenerated)
        {
            foreach (Tuple<int, int> chunk in terrainController.terrainArray.chunkLookUp.Keys)
            {
                if (!terrainController.terrainArray.loadedChunks.Contains(chunk) && chunkEntities.ContainsKey(chunk))
                {
                    foreach (Entity entity in chunkEntities[chunk])
                    {
                        if (entity.obj != null)
                        {
                            Debug.LogFormat("chunk: {0}, x:{1}, y:{2}", chunk, entity.x, entity.y);
                        }
                    }
                }
            }

            foreach (Tuple<int, int> chunk in terrainController.terrainArray.loadedChunks)
            {
                if (chunkEntities.ContainsKey(chunk))//if there are any entities there
                {
                    foreach (Entity entity in chunkEntities[chunk])
                    {
                        if (entity.obj == null)
                        {
                            //not loaded somehow
                            //Debug.LogFormat("chunk: {0}, x:{1}, y:{2}", chunk, entity.x, entity.y);
                        }
                        else if (entity.step()) //if moved during step
                        {
                            //check if chunk changed
                            Vector2 position = entity.obj.transform.position;
                            Tuple<int, int> newChunk = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
                            if (entity.chunk != newChunk)
                            {
                                //will need to be careful making this particular part threaded
                                entitiesWhichChangedChunk.Add(new Tuple<Entity, Tuple<int, int>> (entity, newChunk));
                            }
                        }
                    }
                }
            }
            foreach (Tuple<Entity, Tuple<int, int>> data in entitiesWhichChangedChunk)
            {
                Entity entity = data.Item1;
                Tuple<int, int> newChunk = data.Item2;

                if (!terrainController.terrainArray.loadedChunks.Contains(newChunk)) {
                    //if new chunk is not loaded, we also need to unload the entity
                    UnloadEntity(entity);
                }
                chunkEntities[entity.chunk].Remove(entity);
                entity.chunk = newChunk;
                if (!chunkEntities.ContainsKey(newChunk))
                {
                    chunkEntities[newChunk] = new List<Entity>();
                }
                chunkEntities[newChunk].Add(entity);//*/
            }
            entitiesWhichChangedChunk.Clear();
        }
        
        /*foreach (Tuple<Tuple<int, int>, int> entityPointer in activeEntities)
        {
            chunkEntities[entityPointer.Item1][entityPointer.Item2].step();
        }//*/
    }
}
