using System.Collections;
using UnityEngine;

public class ThunderWall : MonoBehaviour
{
    readonly string playerTag = "Player";
    private bool isProcessing = false; // 중복 호출 방지
    AudioSource source;
    WaitForSeconds ws1 = new WaitForSeconds(1);
    public GameObject tutorial;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = 0.6f;
        tutorial.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            source.volume = 1f;
            PlayerControl pc = collision.gameObject.GetComponent<PlayerControl>();
            if (pc != null && !isProcessing)
            {
                if (!pc.isDead)
                {
                    isProcessing = true;
                    tutorial.SetActive(false);
                    StartCoroutine(HitEffect(pc));
                }
            }
        }
    }

    private IEnumerator HitEffect(PlayerControl pc)
    {
        // 움직임 멈추기
        pc.isPaused = true;

        // 플레이어 자식의 모든 SpriteRenderer 가져오기
        SpriteRenderer[] renderers = pc.GetComponentsInChildren<SpriteRenderer>();

        // 각 렌더러의 원래 색 저장
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }

        Color black = Color.black;
        Color white = Color.white;

        float elapsed = 0f;
        float duration = 0.5f;
        float interval = 0.1f;

        while (elapsed < duration)
        {
            // 색상 토글 (검은색 ↔ 흰색)
            Color target = (Mathf.FloorToInt(elapsed / interval) % 2 == 0) ? black : white;
            foreach (var sr in renderers)
            {
                sr.color = target;
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // 원래 색으로 복구
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = originalColors[i];
        }

        // 최종적으로 죽음 처리
        pc.Dead();

        isProcessing = false;

        yield return ws1;
        source.volume = 0.6f;
    }
}
