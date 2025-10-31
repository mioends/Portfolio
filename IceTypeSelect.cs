using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class IceTypeSelect : MonoBehaviour
{
    public GameObject iceCanvas;
    public List<GameObject> targetObjects;
    private List<GameObject> shuffledObjects;
    private int currentIndex = 0;

    private Coroutine sequenceRoutine;

    BoxCollider iceBigCollider;
    XRGrabInteractable XRGrabInteractable;
    Rigidbody rb;

    private void Start()
    {
        iceCanvas.SetActive(false);
        iceBigCollider = GetComponent<BoxCollider>();
        XRGrabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        XRGrabInteractable.onSelectExited.AddListener(OnReleased);
    }

    private void OnReleased(XRBaseInteractor interactor)
    {
        iceCanvas.SetActive(true);
    }

    public void OnClickIceBtn(GameObject chosen)
    {
        chosen.SetActive(true);
        iceCanvas.SetActive(false);
        iceBigCollider.enabled = false;
        XRGrabInteractable.enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;

        // 랜덤 순서 셔플
        shuffledObjects = new List<GameObject>(targetObjects);
        for (int i = 0; i < shuffledObjects.Count; i++)
        {
            GameObject temp = shuffledObjects[i];
            int randomIndex = Random.Range(i, shuffledObjects.Count);
            shuffledObjects[i] = shuffledObjects[randomIndex];
            shuffledObjects[randomIndex] = temp;
        }

        currentIndex = 0;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = StartCoroutine(HighlightSequence(chosen));
    }

    private IEnumerator HighlightSequence(GameObject chosen)
    {
        while (currentIndex < shuffledObjects.Count)
        {
            GameObject obj = shuffledObjects[currentIndex];

            if (obj != null)
            {
                // 다음에 부술 얼음 빨간색
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = Color.red;
                }
                obj.GetComponent<IceHit>().enabled = true;
                obj.GetComponent<BoxCollider>().enabled = true;

                // 오브젝트가 비활성화될 때까지 대기
                yield return new WaitUntil(() => obj.activeSelf == false);
            }
            currentIndex++;
        }

        // 모든 얼음이 비활성화된 후 실행
        Collider[] collider = chosen.GetComponentsInChildren<Collider>();
        XRGrabInteractable[] xrGrab = chosen.GetComponentsInChildren<XRGrabInteractable>();
        Rigidbody[] Rb = chosen.GetComponentsInChildren<Rigidbody>();

        if (collider != null)
        {
            foreach (var col in collider)
            {
                col.enabled = true;
            }
        }
        if (xrGrab != null)
        {
            foreach (var grab in xrGrab)
            {
                grab.enabled = true;
            }
        }
        if (Rb != null)
        {
            foreach (var rb in Rb)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }
        }
    }
}
