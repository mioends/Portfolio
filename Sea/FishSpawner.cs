using Unity.VisualScripting;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] private FishInfo[] fishInfos; // 0=A, 1=B, 2=C
    [SerializeField]  private int spawnCount = 10;
    [SerializeField] private GameObject spawnPointParent;
    [SerializeField] private FishSpawnPointOption[] spawnPoints;
    [SerializeField] private Transform fishPoolParent; // �θ� ������ Transform

    public PlayerCheck playerCheck;

    private void Awake()
    {
        // ������ FishSpawnPoints ã�Ƽ� ����
        spawnPointParent = GameObject.Find("FishSpawnPoints");

        // ������ FishPool ã�Ƽ� ����
        GameObject fishPoolObj = GameObject.Find("FishPool");
        if (fishPoolObj != null)
        {
            fishPoolParent = fishPoolObj.transform;
        }

        playerCheck = FindAnyObjectByType<PlayerCheck>();
    }

    private void Start()
    {
        if (spawnPointParent != null)
        {
            // spawnPointParent�� �ڽ� FishSpawnPointOption ������Ʈ
            spawnPoints = spawnPointParent.GetComponentsInChildren<FishSpawnPointOption>();
        }

        // ���� ����Ʈ �� ã�� ��� return
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < spawnCount; i++)
        {
            // ���� ����Ʈ ���� ����
            FishSpawnPointOption sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // ����Ʈ Ÿ�Կ� ���� ����� ����
            FishInfo chosen = ChooseFish(sp.pointType);

            // ���� (�θ� fishPoolParent �� ����)
            Fish newFish = Instantiate(chosen.prefab, sp.transform.position, Quaternion.identity, fishPoolParent);
            newFish.Initialize(chosen.data);
        }

        // playerCheck.OnReturnCancle += FishDInstantiate;
    }

    private FishInfo ChooseFish(SpawnPointType type)
    {
        int r = Random.Range(0, 100);

        switch (type)
        {
            case SpawnPointType.Type1:
                return (r < 80) ? fishInfos[0] : fishInfos[1];

            case SpawnPointType.Type2:
                return (r < 80) ? fishInfos[1] : fishInfos[2];

            case SpawnPointType.Type3:
                if (r < 70) return fishInfos[2];
                else return (r < 85) ? fishInfos[0] : fishInfos[1];

            default:
                return fishInfos[0];
        }
    }
    //void FishDInstantiate()
    //{
    //    // �÷��̾� ������ 5��ŭ, Y�� 0.3 ����
    //    Vector3 spawnPos = playerCheck.transform.position + playerCheck.transform.forward * 3f;
    //    // spawnPos.y -= 0.3f;

    //    // �÷��̾ �ٶ󺸵��� ȸ��
    //    Quaternion spawnRot = Quaternion.LookRotation(playerCheck.transform.position - spawnPos);

    //    // Fish ����
    //    Fish newFish = Instantiate(fishInfos[3].prefab, spawnPos, spawnRot, fishPoolParent);
    //    newFish.Initialize(fishInfos[3].data);

    //}
}
