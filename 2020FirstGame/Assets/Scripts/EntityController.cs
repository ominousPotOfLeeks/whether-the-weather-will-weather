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

    HashSet<Entity> entitiesToBeRemoved = new HashSet<Entity>();

    //stores pointers to the entities in chunkEntities
    //public List<Tuple<Tuple<int, int>, int>> activeEntities = new List<Tuple<Tuple<int, int>, int>>();

    public GameObject sheep;
    public GameObject miner;
    public GameObject wheel;
    public GameObject car;

    public int objectPoolInitialSizeSheep;
    public int objectPoolInitialSizeMiner;
    public int objectPoolInitialSizeWheel;
    public int objectPoolInitialSizeCar;

    public TerrainController terrainController;
    public ObjectPooler objectPooler;

    public Dictionary<string, GameObject> objNames;
    public Dictionary<string, List<GameObject>> objectPools = new Dictionary<string, List<GameObject>>();
    public Dictionary<string, int> objectPoolInitialSizes;

    private void Start()
    {
        objNames = new Dictionary<string, GameObject>
        {
            ["sheep"] = sheep,
            ["miner"] = miner,
            ["wheel"] = wheel,
            ["car"] = car
        };
        objectPoolInitialSizes = new Dictionary<string, int>
        {
            ["sheep"] = objectPoolInitialSizeSheep,
            ["miner"] = objectPoolInitialSizeMiner,
            ["wheel"] = objectPoolInitialSizeWheel,
            ["car"] = objectPoolInitialSizeCar
        };

        foreach (string objName in objNames.Keys)
        {
            objectPools[objName] = (objectPooler.AddObjectPool(objNames[objName], objectPoolInitialSizes[objName]));
        }
    }

    public class EntityStepData
    {
        public bool hasMoved;
        public bool tobeRemoved;

        public EntityStepData(bool hasMoved, bool tobeRemoved)
        {
            this.hasMoved = hasMoved;
            this.tobeRemoved = tobeRemoved;
        }
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
        public Func<EntityController.EntityStepData> step;
        public List<Entity> childEntities;
        public bool hasParent;
        public bool hasNonStandardPosition = false;
        public Vector3 position;

        public Entity(float x, float y, Tuple<int, int> chunk, GameObject obj, Func<EntityController.EntityStepData> step, string objName, bool hasParent, List<Entity> childEntities)
        {
            this.childEntities = childEntities;
            this.x = x;
            this.y = y;
            this.chunk = chunk;
            this.obj = obj;
            this.step = step;
            this.objName = objName;
            this.hasParent = hasParent;
        }
    }

    public bool ToggleAtPosition(Vector3 position)
    {
        GameObject obj = GetObjectAtPosition(position);
        if (obj != null)
        {
            ToggleableScript toggleableScript;
            if ((toggleableScript = obj.GetComponent<ToggleableScript>()) != null)
            {
                toggleableScript.Toggle();
                return true;
            }
        }
        return false;
    }

    public void RemoveEntityAtPosition(Vector3 position)
    {
        GameObject obj = GetObjectAtPosition(position);
        if (obj != null)
        {
            EntityScript entityScript;
            if ((entityScript = obj.GetComponent<EntityScript>()) != null)
            {
                Entity entity = entityScript.selfEntity;
                RemoveEntity(entity);
            }
        }
    }

    public void RemoveEntity(Entity entity)
    {
        chunkEntities[entity.chunk].Remove(entity);
        entity.obj.GetComponent<EntityScript>().remove();
        entity.obj.SetActive(false);
    }

    public GameObject GetObjectAtPosition(Vector3 position)
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

    public Entity AddEntity(float x, float y, string objName, bool hasParent = false, List<Entity> childEntities = null)
    {
        //add to array but don't load
        Tuple<int, int> chunk = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

        
        Entity entity = new Entity(x, y, chunk, null, null, objName, hasParent, childEntities);

        AddChunkEntityListing(entity, chunk);

        if (terrainController.terrainArray.nextLoadedChunks.Contains(entity.chunk) || terrainController.terrainArray.loadedChunks.Contains(entity.chunk))
        {
            //adding entity to chunk that is already loaded, so entity will stay unloaded unless we load it now
            LoadEntity(entity);
        } else
        {
            //Debug.LogFormat("added unloaded entity: {0}", entity.objName);
        }
        return entity;
    }

    public void UnloadChunkEntities(Tuple<int, int> chunk)
    {
        if (chunkEntities.ContainsKey(chunk))
        {
            foreach (Entity entity in chunkEntities[chunk])
            {
                if (!entity.hasParent)
                {
                    UnloadEntity(entity);
                }
            }
        }
    }

    public void LoadChunkEntities(Tuple<int, int> chunk)
    {
        if (chunkEntities.ContainsKey(chunk))
        {
            foreach (Entity entity in chunkEntities[chunk])
            {
                if (!entity.hasParent)
                {
                    LoadEntity(entity);
                }
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
            Vector2 position = GetEntityPosition(entity);
            entity.x = position.x;
            entity.y = position.y;

            //Debug.LogFormat("1childentities {0}", entity.childEntities.Count);

            EntityScript entityScript = entity.obj.GetComponent<EntityScript>();
            entityScript.selfEntity = null;
            entityScript.unInitialize();

            //Debug.LogFormat("2childentities {0}", entity.childEntities.Count);

            if (entity.childEntities != null)
            {
                foreach (Entity childEntity in entity.childEntities)
                {
                    UnloadEntity(childEntity);
                }
            }
            Debug.LogFormat("unloaded {0}", entity.objName);
            entity.obj.SetActive(false);
            entity.obj = null;
        }
    }

    public void LoadEntity(Entity entity)
    {
        Vector2 position = new Vector2(entity.x, entity.y);

        if (entity.obj != null)
        {
            //Debug.LogFormat("duplicate instantiation of one entity: {0}", entity.objName);
        } else
        {
            entity.obj = objectPooler.GetObjectFromPool(objectPools[entity.objName]);
            if (entity.obj == null)
            {
                objectPooler.IncreaseObjectPoolSize(objectPools[entity.objName], objNames[entity.objName], objectPoolInitialSizes[entity.objName]);
                entity.obj = objectPooler.GetObjectFromPool(objectPools[entity.objName]);
            }
            entity.obj.transform.position = position;
            entity.obj.SetActive(true);
        }

        if (entity.childEntities != null)
        {
            foreach (Entity childEntity in entity.childEntities)
            {
                LoadEntity(childEntity);
            }
        }

        EntityScript entityScript = entity.obj.GetComponent<EntityScript>();
        entity.step = entityScript.step;
        entityScript.selfEntity = entity;
        entityScript.initialize();
    }

    public void EntityMovedSoUpdateChunk(Entity entity)
    {
        //check if chunk changed
        Vector2 position = GetEntityPosition(entity);
        
        Tuple<int, int> newChunk = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        if (!terrainController.ChunksEqual(entity.chunk, newChunk))
        {
            //will need to be careful making this particular part threaded
            entitiesWhichChangedChunk.Add(new Tuple<Entity, Tuple<int, int>>(entity, newChunk));
        }
    }

    public void AddChunkEntityListing(Entity entity, Tuple<int, int> chunk)
    {
        if (!chunkEntities.ContainsKey(chunk))
        {
            chunkEntities[chunk] = new List<Entity>();
        }
        chunkEntities[chunk].Add(entity);
    }

    public Vector2 GetEntityPosition(Entity entity)
    {
        Vector2 position;
        if (entity.hasNonStandardPosition)
        {
            position = entity.position;
        }
        else
        {
            position = entity.obj.transform.position;
        }
        return position;
    }

    public List<Entity> GetCurrentEntities()
    {
        List<Entity> currentEntities = new List<Entity>();
        foreach (Tuple<int, int> chunk in terrainController.terrainArray.loadedChunks)
        {
            if (chunkEntities.ContainsKey(chunk))//if there are any entities there
            {
                foreach (Entity entity in chunkEntities[chunk])
                {
                    currentEntities.Add(entity);
                }
            }
        }
        return currentEntities;
    }

    public void ChangeEntityChunk(Entity entity, Tuple<int, int> newChunk)
    {
        chunkEntities[entity.chunk].Remove(entity);
        entity.chunk = newChunk;
        AddChunkEntityListing(entity, newChunk);
    }

    void FixedUpdate()
    {
        if (terrainController.isGenerated)
        {
            List<Entity> currentEntities = GetCurrentEntities();
            foreach(Entity entity in currentEntities)
            {
                if (entity.obj == null && !entity.hasParent)
                {
                    //not loaded and not a child 
                    Debug.LogFormat("entity not loaded chunk: {0}, x:{1}, y:{2}", entity.chunk, entity.x, entity.y);
                }
                else
                {
                    EntityStepData stepData = entity.step();

                    if (stepData.tobeRemoved)
                    {
                        entitiesToBeRemoved.Add(entity);
                    }
                    else if (!entity.hasParent)//only update chunk for non-child entities
                    {
                        EntityMovedSoUpdateChunk(entity);
                    }
                }
            }

            foreach (Tuple<Entity, Tuple<int, int>> data in entitiesWhichChangedChunk)
            {
                Entity entity = data.Item1;
                Tuple<int, int> newChunk = data.Item2;

                if (!entity.hasParent)
                {
                    if (!terrainController.terrainArray.loadedChunks.Contains(newChunk))
                    {
                        //if new chunk is not loaded, we also need to unload the entity
                        UnloadEntity(entity);
                    }

                    if (entity.childEntities != null)
                    {
                        foreach(Entity childEntity in entity.childEntities)
                        {
                            ChangeEntityChunk(childEntity, newChunk);
                        }
                    }

                    ChangeEntityChunk(entity, newChunk);
                }
            }
            entitiesWhichChangedChunk.Clear();

            foreach (Entity entity in entitiesToBeRemoved)
            {
                RemoveEntity(entity);
            }
            entitiesToBeRemoved.Clear();
        }
    }
}
