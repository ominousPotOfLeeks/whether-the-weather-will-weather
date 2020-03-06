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

    public TerrainController terrainController;

    public Dictionary<string, GameObject> objNames;

    private void Start()
    {
        objNames = new Dictionary<string, GameObject>
        {
            ["sheep"] = sheep
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

        public Entity(int x, int y, Tuple<int, int> chunk, GameObject obj, Func<bool> step, string objName)
        {
            this.x = x;
            this.y = y;
            this.chunk = chunk;
            this.obj = obj;
            this.step = step;
            this.objName = objName;
        }
    }

    public void AddEntity(int x, int y, string objName)
    {
        //add to array but don't load
        Tuple<int, int> chunk = terrainController.terrainArray.GetChunkCoords(x, y);
        Vector3Int position = new Vector3Int(x, y, 0);

        
        Entity entity = new Entity(x, y, chunk, null, null, objName);

        if (!chunkEntities.ContainsKey(chunk))
        {
            List<Entity> positionObjects = new List<Entity>();
            chunkEntities[chunk] = positionObjects;
        }
        chunkEntities[chunk].Add(entity);//*/
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
        Vector2 position = entity.obj.transform.position;
        entity.x = position.x;
        entity.y = position.y;

        Destroy(entity.obj);
        entity.obj = null;
    }

    public void LoadEntity(Entity entity)
    {
        Vector2 position = new Vector2(entity.x, entity.y);

        entity.obj = Instantiate(objNames[entity.objName], position, Quaternion.identity);
        entity.step = entity.obj.GetComponent<SheepScript>().Step;
    }

    // Update is called once per frame
    void Update()
    {
        if (terrainController.isGenerated)
        {
            foreach (Tuple<int, int> chunk in terrainController.terrainArray.loadedChunks)
            {
                if (chunkEntities.ContainsKey(chunk))//if there are any entities there
                {
                    foreach (Entity entity in chunkEntities[chunk])
                    {

                        //if moved during step
                        if (entity.step())
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
                    List<Entity> positionObjects = new List<Entity>();
                    chunkEntities[newChunk] = positionObjects;
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
