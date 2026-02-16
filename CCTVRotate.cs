using UnityEngine;
using System.Collections;

public class CCTVRotate : MonoBehaviour
{
    public float rotationSpeed = 20f; // 360도 / 30초 = 12도/초
    readonly string playerTag = "Player";

    private bool isPaused = false;
    private Coroutine deathCoroutine;
    private SpriteRenderer[] currentSprites;
    private Color[] originalColors;
    AudioSource source;

    WaitForSeconds deadWs = new WaitForSeconds(0.5f);

    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = 0.5f;
        source.pitch = 0.25f;
    }

    void Update()
    {
        if (!isPaused)
        {
            // 원래대로 계속 회전
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            PlayerControl pc = collision.GetComponent<PlayerControl>();
            if (pc == null || pc.isDead) return; // 죽은 플레이어는 무시

            source.volume = 1.0f;
            // 회전 멈춤
            isPaused = true;

            if (deathCoroutine == null)
            {
                deathCoroutine = StartCoroutine(KillPlayerWithRedEffect(collision.gameObject, 1f));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            PlayerControl pc = collision.GetComponent<PlayerControl>();
            if (pc == null || pc.isDead) return; // 죽은 플레이어는 무시

            // 회전 재개
            isPaused = false;

            // 코루틴 취소
            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }

            // 색상 원래대로 복구
            if (currentSprites != null && originalColors != null)
            {
                for (int i = 0; i < currentSprites.Length; i++)
                {
                    if (currentSprites[i] != null)
                        currentSprites[i].color = originalColors[i];
                }
            }

            currentSprites = null;
            originalColors = null;

            source.volume = 0.5f; // 플레이어가 콜라이더 밖으로 나가면 볼륨 복귀
        }
    }

    private IEnumerator KillPlayerWithRedEffect(GameObject player, float delay)
    {
        currentSprites = player.GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;

        // 원래 색상 저장
        originalColors = new Color[currentSprites.Length];
        for (int i = 0; i < currentSprites.Length; i++)
        {
            originalColors[i] = currentSprites[i].color;
        }

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / delay;

            // 색상 점점 빨갛게 변하게
            for (int i = 0; i < currentSprites.Length; i++)
            {
                if (currentSprites[i] != null)
                {
                    Color targetColor = Color.red;
                    currentSprites[i].color = Color.Lerp(originalColors[i], targetColor, t);
                }
            }

            yield return null;
        }

        // Dead 호출
        PlayerControl pc = player.GetComponent<PlayerControl>();
        if (pc != null && !pc.isDead) // 이미 죽은 상태가 아니면 Dead 호출
        {
            pc.Dead();
        }

        source.volume = 0.5f; // 플레이어 사망 시 볼륨 복귀

        // CCTV 회전 재개
        isPaused = false;

        // 잠시 대기 후 오브젝트 삭제
        yield return deadWs;
        Destroy(player);

        deathCoroutine = null;
    }

}
