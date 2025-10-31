using Unity.VisualScripting;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] private FishInfo[] fishInfos; // 0=A, 1=B, 2=C
    [SerializeField]  private int spawnCount = 10;
    [SerializeField] private GameObject spawnPointParent;
    [SerializeField] private FishSpawnPointOption[] spawnPoints;
    [SerializeField] private Transform fishPoolParent; // 부모 지정할 Transform

    public PlayerCheck playerCheck;

    private void Awake()
    {
        // 씬에서 FishSpawnPoints 찾아서 참조
        spawnPointParent = GameObject.Find("FishSpawnPoints");

        // 씬에서 FishPool 찾아서 참조
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
            // spawnPointParent의 자식 FishSpawnPointOption 컴포넌트
            spawnPoints = spawnPointParent.GetComponentsInChildren<FishSpawnPointOption>();
        }

        // 스폰 포인트 못 찾을 경우 return
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < spawnCount; i++)
        {
            // 스폰 포인트 랜덤 선택
            FishSpawnPointOption sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 포인트 타입에 따라 물고기 랜덤
            FishInfo chosen = ChooseFish(sp.pointType);

            // 생성 (부모를 fishPoolParent 로 지정)
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
    //    // 플레이어 앞으로 5만큼, Y는 0.3 낮게
    //    Vector3 spawnPos = playerCheck.transform.position + playerCheck.transform.forward * 3f;
    //    // spawnPos.y -= 0.3f;

    //    // 플레이어를 바라보도록 회전
    //    Quaternion spawnRot = Quaternion.LookRotation(playerCheck.transform.position - spawnPos);

    //    // Fish 생성
    //    Fish newFish = Instantiate(fishInfos[3].prefab, spawnPos, spawnRot, fishPoolParent);
    //    newFish.Initialize(fishInfos[3].data);

    //}
}
