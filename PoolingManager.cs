using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    public static PoolingManager p_instance;
    public GameObject bulletPrefab;
    public GameObject firePos;
    public GameObject coinPrefabs;
    public GameObject headPrefabs;

    [SerializeField] private int bulletMaxPool = 20;
    [SerializeField] private int coinMaxPool = 200;
    [SerializeField] private int headMaxPool = 200;

    [SerializeField] private List<GameObject> bulletPool = new List<GameObject>();
    [SerializeField] private List<GameObject> coinPool = new List<GameObject>();
    [SerializeField] private List<GameObject> headPool = new List<GameObject>();

    public List<Transform> SpawnList;
    public List<GameObject> enemyPool;
    private int ZombieKnightMaxPool = 3;
    public GameObject ZombieKnightPrefab;
    NPCDamage nPCDamage;
    void Awake()
    {
        if (p_instance == null)
            p_instance = this;
        else if (p_instance != this)
            Destroy(this.gameObject);

        var spawnPos = GameObject.Find("SpawnPoints").gameObject;
        if (spawnPos != null)
        {
            spawnPos.GetComponentsInChildren<Transform>(SpawnList);
        }
        SpawnList.RemoveAt(0);

        nPCDamage = FindAnyObjectByType<NPCDamage>();

        CreateBullet();
        CreateZombieKnight();
        CreateCoin();
        CreateHead();
    }
    private void Start()
    {
        InvokeRepeating("ZombieKnightSpawn", 0.02f, 3.0f);
    }
    private void CreateBullet()
    {
        GameObject BulletObjectPools = new GameObject("BulletPools");
        // BulletObjectPools.transform.SetParent(firePos.transform);
        for (int i = 0; i < bulletMaxPool; i++)
        {
            var bullet = Instantiate(bulletPrefab, BulletObjectPools.transform);
            bullet.name = $"총알 {i + 1}발";
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }
    public GameObject GetBullet()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (bulletPool[i].activeSelf == false)
            {
                return bulletPool[i];
            }
        }
        return null;
    }
    private void CreateCoin()
    {
        GameObject CoinObjectPools = new GameObject("CoinPools");
        for (int i = 0; i < coinMaxPool; i++)
        {
            var coin = Instantiate(coinPrefabs, CoinObjectPools.transform);
            coin.name = $"{i + 1}골드";
            coin.SetActive(false);
            coinPool.Add(coin);
        }
    }
    public List<GameObject> GetCoins(int count)
    {
        List<GameObject> coinsToReturn = new List<GameObject>();
        int given = 0;

        foreach (var coin in coinPool)
        {
            if (!coin.activeSelf)
            {
                coinsToReturn.Add(coin);
                given++;
                if (given >= count)
                    break;
            }
        }

        return coinsToReturn;
    }

    private void CreateHead()
    {
        GameObject HeadObjectPools = new GameObject("HeadPools");
        for (int i = 0; i < headMaxPool; i++)
        {
            var headPre = Instantiate(headPrefabs, HeadObjectPools.transform);
            headPre.name = $"머리 {i + 1}개";
            headPre.SetActive(false);
            headPool.Add(headPre);
        }
    }
    public GameObject GetHead()
    {
        foreach (var head in headPool)
        {
            if (!head.activeSelf)
            {
                return head;
            }
        }
        return null;
    }

    private void CreateZombieKnight()
    {
        GameObject ZombieKnightObjectPools = new GameObject("ZombieKnightPools");
        
        for (int i = 0; i < ZombieKnightMaxPool; i++)
        {
            var zombie = Instantiate(ZombieKnightPrefab, ZombieKnightObjectPools.transform);
            zombie.name = $"좀비기사 {i + 1}명";
            zombie.SetActive(false);
            enemyPool.Add(zombie);
        }
    }
    public void ZombieKnightSpawn()
    {
        if (!UIManager.u_Instance.isZombieKillQuestClear || UIManager.u_Instance.doIWantKillNPC && !nPCDamage.isNPCDie)
        {
            foreach (var zombieKnight in enemyPool)
            {
                if (zombieKnight.activeSelf == false)
                {
                    // SpawnList에서 랜덤한 위치 선택
                    int randomIndex = Random.Range(0, SpawnList.Count);
                    Transform spawnPoint = SpawnList[randomIndex];
                    if (spawnPoint.gameObject.GetComponent<SpawnStop>().playerInRange) continue;

                    zombieKnight.transform.position = spawnPoint.position;
                    zombieKnight.transform.rotation = spawnPoint.rotation;
                    zombieKnight.SetActive(true);
                    break;
                }
            }
        }
    }
}
