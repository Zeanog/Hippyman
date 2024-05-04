using Neo.Utility;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField]
    protected Neo.Grid grid;

    [SerializeField]
    protected Transform pelletsParent;

    [SerializeField]
    protected GameObject pelletPrefab;

    [SerializeField]
    protected GameObject powerPelletPrefab;

    public Vector2Int[] PowerPelletLocations { get; protected set; }

    [SerializeField]
    protected List<GameObject> bonusPrefabs = new List<GameObject>();
    protected AValuePickup<int> currentBonusPickup;

    protected float pickupReactivationDelay = 5f;
    protected Timer activatePickupTimer;

    protected void Awake()
    {
        activatePickupTimer = Timer.Create("ActivatePickup", pickupReactivationDelay);
        activatePickupTimer.OnElapsed.AddListener(ActivateRandomValuePickup);
        currentBonusPickup = null;

        PowerPelletLocations = new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, grid.NumTiles.y - 2), new Vector2Int(grid.NumTiles.x - 1, 1), new Vector2Int(grid.NumTiles.x - 1, grid.NumTiles.y - 2) };

        Game.Instance.AddListener<AValuePickup<int>>("OnCollected", OnValuePickupCollected);
    }

    protected void Start()
    {
        GeneratePellets(pelletsParent);

        activatePickupTimer.Restart();
    }

    protected void GeneratePellets(Transform parent)
    {
        bool shouldCreatePowerPellet(int x, int y)
        {
            foreach (var loc in PowerPelletLocations)
            {
                if (loc.x == x && loc.y == y)
                {
                    return true;
                }
            }

            return false;
        }

        using (var pelletListSlip = DataStructureLibrary<List<GameObject>>.Instance.CheckOut())
        {
            pelletListSlip.Value.Clear();

            Vector2Int gridLoc = new Vector2Int();
            for (int iy = 0; iy < grid.NumTiles.y; ++iy)
            {
                gridLoc.y = iy;
                for (int ix = 0; ix < grid.NumTiles.x; ++ix)
                {
                    gridLoc.x = ix;
                    if (grid.TileIsObstructed(gridLoc))
                    {
                        continue;
                    }

                    GameObject go = null;
                    grid.GridToWorld(ix, iy, out Vector3 worldPos);

                    if (shouldCreatePowerPellet(ix, iy))
                    {
                        go = Instantiate(powerPelletPrefab);
                    }
                    else
                    {
                        go = Instantiate(pelletPrefab);
                        pelletListSlip.Value.Add(go);
                    }

                    var pos = go.transform.position;
                    pos.x = worldPos.x;
                    pos.z = worldPos.z;
                    go.transform.position = pos;
                    go.transform.rotation = Quaternion.identity;
                    go.transform.SetParent(parent, true);
                }
            }

            float totalValueOnceAllCollected = 10f;// Dime bag  :D
            foreach (var pelletGO in pelletListSlip.Value)
            {
                var points = pelletGO.GetComponent<Pellet>();
                points.Value = (1f / pelletListSlip.Value.Count) * totalValueOnceAllCollected;
            }
        }
    }

    protected void ActivateRandomValuePickup()
    {
        if (currentBonusPickup != null)
        {
            return;
        }

        if (bonusPrefabs.Count > 0)
        {
            var index = UnityEngine.Random.Range(0, bonusPrefabs.Count);
            GameObject go = Instantiate(bonusPrefabs[index]);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.SetParent(transform, false);
            currentBonusPickup = go.GetComponent<AValuePickup<int>>();
            currentBonusPickup.gameObject.SetActive(true);
        }
    }

    protected void DestroyBonusPickup()
    {
        if(currentBonusPickup != null)
        {
            Destroy(currentBonusPickup.gameObject);
        }
        currentBonusPickup = null;
    }

    protected void OnValuePickupCollected(object sender, object evtData)
    {
        System.Diagnostics.Debug.Assert(sender.Equals(currentBonusPickup));
        DestroyBonusPickup();
        activatePickupTimer.Restart();
    }
}