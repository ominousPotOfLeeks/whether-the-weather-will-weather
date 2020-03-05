using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{

    public Dictionary<Tuple<int, int>, List<Entity>> chunkEntities =
            new Dictionary<Tuple<int, int>, List<Entity>>();
    public Dictionary<Tuple<int, int>, int> chunkEntitiesListLengths =
            new Dictionary<Tuple<int, int>, int>();

    public Dictionary<GameObject, Tuple<Tuple<int, int>, int>> objPointers = 
            new Dictionary<GameObject, Tuple<Tuple<int, int>, int>>();

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
        public GameObject obj;
        public string objName;
        public Action step;

        public Entity(int x, int y, Tuple<int, int> chunk, GameObject obj, Action step, string objName)
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
        Tuple<int, int> chunk = terrainController.terrainArray.GetChunkCoords(x, y);
        Vector3Int position = new Vector3Int(x, y, 0);

        GameObject thing = objNames[objName];

        GameObject obj = Instantiate(thing, position, Quaternion.identity);
        SheepScript sh = thing.GetComponent<SheepScript>();
        Entity entity = new Entity(x, y, chunk, obj, sh.Step, objName);

        if (!chunkEntities.ContainsKey(chunk))
        {
            List<Entity> positionObjects = new List<Entity>();
            chunkEntities[chunk] = positionObjects;
            chunkEntitiesListLengths[chunk] = 0;
        }
        chunkEntities[chunk].Add(entity);//*/
        chunkEntitiesListLengths[chunk] += 1;
        objPointers.Add(obj, new Tuple<Tuple<int, int>, int>(chunk, chunkEntitiesListLengths[chunk]));
        //activeEntities.Add(new Tuple<Tuple<int, int>, int> (chunk, chunkEntitiesListLengths[chunk]));
    }

    public void RemoveEntity(GameObject obj)
    {
        Tuple<Tuple<int, int>, int> ptr = objPointers[obj];
        chunkEntities[ptr.Item1].RemoveAt(ptr.Item2);
        objPointers.Remove(obj);
        Destroy(obj);
    }

    public void UnloadChunkEntities(Tuple<int, int> chunk)
    {
        if (chunkEntities.ContainsKey(chunk))
        {
            foreach (Entity entity in chunkEntities[chunk])
            {
                UnloadEntity(entity, chunk);
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
    public void UnloadEntity(Entity entity, Tuple<int, int> chunk)
    {
        Vector2 position = entity.obj.transform.position;
        entity.x = position.x;
        entity.y = position.y;
        entity.chunk = chunk;
        Destroy(entity.obj);
        entity.obj = null;
    }

    public void LoadEntity(Entity entity)
    {
        Vector2 position = new Vector2(entity.x, entity.y);
        entity.obj = Instantiate(objNames[entity.objName], position, Quaternion.identity);
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
                        entity.step();
                    }
                }
            }
        }
        
        /*foreach (Tuple<Tuple<int, int>, int> entityPointer in activeEntities)
        {
            chunkEntities[entityPointer.Item1][entityPointer.Item2].step();
        }//*/
    }
}
