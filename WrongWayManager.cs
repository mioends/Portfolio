using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WrongWayManager : MonoBehaviour
{
    [Header("Wrong Way UI")]
    [SerializeField] private Image wrongWayImage;
    float blinkDuration = 3f;
    float blinkInterval = 0.3f;

    [Header("Reference")]
    [SerializeField] private RaceManager raceManager;

    [Header("Sound")]
    [SerializeField] private AudioClip warningSound;

    private AudioSource audioSource;

    private int lastFlagIndex = -1;
    private Coroutine warningCoroutine;

    private void Start()
    {
        if (raceManager == null)
            raceManager = FindFirstObjectByType<RaceManager>();

        if (wrongWayImage != null)
            wrongWayImage.enabled = false;

        audioSource = GetComponent<AudioSource>();
    }



    public bool CheckFlagPassed(int flagIndex, bool returningToSkippedFlag)
    {
        if (raceManager == null)
            return false;

        if (returningToSkippedFlag)
        {
            lastFlagIndex = flagIndex;

            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }

            if (wrongWayImage != null)
                wrongWayImage.enabled = false;

            return false;
        }

        if (lastFlagIndex == -1)
        {
            lastFlagIndex = flagIndex;
            return false;
        }

        int lastIndex = raceManager.Flags.Length - 1;

        bool isReverse = flagIndex < lastFlagIndex;

        if (lastFlagIndex == 0 && flagIndex == lastIndex)
            isReverse = true;
        else if (lastFlagIndex == lastIndex && flagIndex == 0)
            isReverse = false;

        if (isReverse)
        {
            if (warningCoroutine != null)
                StopCoroutine(warningCoroutine);

            warningCoroutine = StartCoroutine(PlayWarning());
        }
        else if (flagIndex > lastFlagIndex)
        {
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }

            if (wrongWayImage != null)
                wrongWayImage.enabled = false;
        }

        lastFlagIndex = flagIndex;

        return isReverse;
    }

    private IEnumerator PlayWarning()
    {
        if (audioSource != null && warningSound != null)
            audioSource.PlayOneShot(warningSound);

        float timer = 0f;
        bool visible = false;

        while (timer < blinkDuration)
        {
            visible = !visible;

            if (wrongWayImage != null)
                wrongWayImage.enabled = visible;

            yield return new WaitForSeconds(blinkInterval);

            timer += blinkInterval;
        }

        if (wrongWayImage != null)
            wrongWayImage.enabled = false;

        warningCoroutine = null;
    }

    public void ResetWrongWay()
    {
        lastFlagIndex = -1;

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }

        if (wrongWayImage != null)
            wrongWayImage.enabled = false;
    }
}