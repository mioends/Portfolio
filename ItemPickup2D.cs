using UnityEngine;
using TMPro;
using System.Collections;

public class ItemPickup2D : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupDistance = 1.5f;

    [Header("Runtime Info")]
    public ItemData itemData;
    public bool isHeld = false;

    private Transform player;
    private PlayerInputSystem playerInput;
    private Transform originalParent;
    public PlayerStatus interactor;

    private bool isSubscribed = false;
    private bool pickupCooldown = false;
    public bool isRising = false;

    public TextMeshProUGUI nameTxt;
    private string playerTag = "Player";
    private Collider2D[] colliders;

    public Color originalColor;
    public bool hasOriginalColorSaved = false;

    private Rigidbody2D rb;

    private WaitForSeconds ws2 = new WaitForSeconds(0.2f);

    private ItemAutoReturn autoReturn;
    ItemEffect effect;
    PetFollowAdvanced petFollow;

    public SpriteRenderer itemSprite;
    [HideInInspector] public int originalOrder;

    public AudioClip ItemPickUpSound;

    private void Awake()
    {
        originalParent = transform.parent;

        Transform canvas = transform.GetChild(0);
        if (canvas != null)
            nameTxt = canvas.GetChild(0).GetComponent<TextMeshProUGUI>();

        colliders = GetComponentsInChildren<Collider2D>(true);
        rb = GetComponent<Rigidbody2D>();
        autoReturn = GetComponent<ItemAutoReturn>();
        effect = GetComponent<ItemEffect>();
        petFollow = GetComponent<PetFollowAdvanced>();
        itemSprite = GetComponent<SpriteRenderer>();
        if (itemSprite != null)
            originalOrder = itemSprite.sortingOrder;

        if (nameTxt != null)
            nameTxt.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        isRising = false;
    }
    private void OnDisable()
    {
        SafeUnsubscribe();
        player = null;
        playerInput = null;

        if (interactor != null && interactor.currentItem == this)
            interactor.currentItem = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (pickupCooldown) return;
        if (!collision.collider.CompareTag(playerTag)) return;
        if (isHeld) return;
        if (isRising) return;

        interactor = collision.collider.GetComponent<PlayerStatus>();
        if (interactor == null) return;

        if (interactor.isDead || interactor.killedBySoldier) return;

        if (interactor.currentItem != null && interactor.currentItem != this)
            return;

        if (nameTxt != null)
            nameTxt.gameObject.SetActive(true);

        if (interactor.currentItem == null)
        {
            interactor.currentItem = this;
            player = collision.transform;
            playerInput = collision.collider.GetComponent<PlayerInputSystem>();
            SubscribeInput();

            
            PlayerItemActionUIController playerUI = collision.gameObject.GetComponent<PlayerItemActionUIController>();
            if (itemData.itemID == 204 && playerUI != null)
            {
                // actionUI 활성화 Pet 문구 적용
                playerUI.OnPickUpItem(this); // 기본 동작
                playerUI.ShowPetText(); // Pet 텍스트 강제 적용
            }

            if (!interactor.hasEverPickedItem)
                interactor.grabInfo?.SetActive(true);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;
        if (isHeld) return;
        if (isRising) return;

        // 플레이어 가져오기
        var ps = collision.collider.GetComponent<PlayerStatus>();
        if (ps == null) return;

        if (ps.isDead || ps.killedBySoldier) return;

        // 이미 다른 아이템을 보고 있고 그게 this가 아니라면 무시
        if (ps.currentItem != null && ps.currentItem != this)
            return;

        // 플레이어가 현재 아이템을 들고 있지 않다면 이 아이템을 바라보게 함
        if (ps.currentItem == null)
        {
            ps.currentItem = this;

            // nameTxt 활성화
            if (nameTxt != null && !isHeld)
                nameTxt.gameObject.SetActive(true);

            // 입력 연결
            if (playerInput == null)
                playerInput = collision.collider.GetComponent<PlayerInputSystem>();

            if (player == null)
                player = collision.transform;

            SubscribeInput();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        var ps = collision.collider.GetComponent<PlayerStatus>();
        if (ps != null)
        {
            if (ps.isDead || ps.killedBySoldier) return;

            // 들고 있지 않을 때만 currentItem을 해제
            if (!isHeld && ps.currentItem == this)
                ps.currentItem = null;

            // 들고 있을 때는 grabInfo 꺼지면 안 됨
            if (!isHeld)
                ps.grabInfo?.SetActive(false);
        }

        // 들고 있을 때는 Unsubscribe 하면 안 됨
        if (!isHeld)
            SafeUnsubscribe();

        PlayerItemActionUIController playerUI = collision.gameObject.GetComponent<PlayerItemActionUIController>();
        if (!isHeld && itemData.itemID == 204 && playerUI != null)
        {
            playerUI.actionUI.SetActive(false);
        }

        if (!isHeld && nameTxt != null)
            nameTxt.gameObject.SetActive(false);
    }

    private void SubscribeInput()
    {
        if (playerInput == null || isSubscribed) return;

        playerInput.onInteractPressed.AddListener(OnInteractPressed);
        playerInput.onConsumptionPressed.AddListener(OnConsumptionPressed);
        isSubscribed = true;
    }

    private void SafeUnsubscribe()
    {
        if (playerInput == null || !isSubscribed) return;

        playerInput.onInteractPressed.RemoveListener(OnInteractPressed);
        playerInput.onConsumptionPressed.RemoveListener(OnConsumptionPressed);
        isSubscribed = false;
    }


    public void OnInteractPressed()
    {
        if (player == null || interactor == null) return;
        if (isRising) return;

        if (!isHeld)
        {
            float distance = Vector2.Distance(player.position, transform.position);
            if (distance > pickupDistance) return;
            PickUp();
        }
        else
        {
            PutDown();
        }
    }

    private void PickUp()
    {
        if (player == null) return;

        isHeld = true;

        if (!interactor.hasEverPickedItem)
            interactor.hasEverPickedItem = true;

        if (autoReturn != null)
            autoReturn.StopAutoReturn();

        Transform holdPoint = player.childCount > 0 ? player.GetChild(0) : player;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;

        if (rb != null)
            rb.simulated = false;

        interactor.playerAnimator.SetBool(interactor.hashHold, true);

        var ui = player.GetComponent<PlayerItemActionUIController>();
        if (ui != null)
            ui.OnPickUpItem(this);

        foreach (var col in colliders)
            col.isTrigger = true;

        if (nameTxt != null)
            nameTxt.gameObject.SetActive(false);

        interactor.grabInfo?.SetActive(false);

        if (petFollow != null) petFollow.enabled = false;

        // 플레이어 픽업 사운드 재생
        PlayerControl pc = player.GetComponent<PlayerControl>();
        if (pc.sfxSource != null && pc != null && ItemPickUpSound != null)
            pc.sfxSource.PlayOneShot(ItemPickUpSound);
    }

    public void ForcePickUp(Transform targetPlayer)
    {
        player = targetPlayer;
        interactor = player.GetComponent<PlayerStatus>();
        playerInput = player.GetComponent<PlayerInputSystem>();
        if (petFollow != null) petFollow.enabled = false;

        SubscribeInput();
        PickUp();
    }

    public void PutDown()
    {
        if (player == null) return;

        isHeld = false;
        transform.SetParent(originalParent);
        transform.position = player.position + new Vector3(0, -0.2f, 0);

        if (rb != null)
            rb.simulated = true;

        var ui = player.GetComponent<PlayerItemActionUIController>();
        if (ui != null)
            ui.OnDropItem();

        foreach (var col in colliders)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (itemSprite != null)
            itemSprite.sortingOrder = originalOrder;

        // 들고 있는 아이템 초기화
        if (interactor != null && interactor.currentItem == this)
            interactor.currentItem = null;

        interactor.playerAnimator.SetBool(interactor.hashHold, false);

        SafeUnsubscribe();

        if (autoReturn != null)
            autoReturn.StartAutoReturn();

        if (petFollow != null) petFollow.enabled = true;

        StartCoroutine(PickupCooldownRoutine());

        // 내려놓기 사운드 재생
        PlayerControl pc = player.GetComponent<PlayerControl>();
        if (pc.sfxSource != null && pc != null && ItemPickUpSound != null)
            pc.sfxSource.PlayOneShot(ItemPickUpSound);
    }

    private IEnumerator PickupCooldownRoutine()
    {
        pickupCooldown = true;
        yield return ws2;
        pickupCooldown = false;
    }


    public void ClearPickupCooldown()
    {
        pickupCooldown = false;
    }

    public void ForceDropBySystem()
    {
        if (!isHeld)
            return;

        isHeld = false;

        transform.SetParent(originalParent);

        if (rb != null)
            rb.simulated = true;

        foreach (var col in colliders)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (interactor != null)
        {
            if (interactor.currentItem == this)
                interactor.currentItem = null;

            interactor.grabInfo?.SetActive(false);
        }

        SafeUnsubscribe();

        if (itemSprite != null)
            itemSprite.sortingOrder = originalOrder;

        if (petFollow != null) petFollow.enabled = false;

        if (autoReturn != null)
            autoReturn.StartAutoReturn();
    }

    private void OnConsumptionPressed()
    {
        if (petFollow != null && !isHeld)
        {
            petFollow.gameObject.GetComponent<TurtlePettingTrigger>().Pet();
        }

        if (!isHeld) return;

        // 아이템 소비 실행
        if (effect != null)
            effect.itemConsume();

        // Read 타입이면 손에서 내려놓지 않음
        if (itemData != null && itemData.effectType == ItemEffectType.Read)
        {
            return; // 그대로 들고 있게 함
        }

        // 나머지 타입은 소비 후 손에서 내려놓기
        if (interactor != null)
        {
            interactor.playerAnimator.SetBool(interactor.hashHold, false);

            if (interactor.currentItem == this)
                interactor.currentItem = null;
        }
    }

    public void ResetPickupState()
    {
        isHeld = false;

        transform.SetParent(null);

        if (rb != null)
            rb.simulated = true;

        foreach (var col in colliders)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (nameTxt != null)
            nameTxt.gameObject.SetActive(false);

        pickupCooldown = false;
    }
}
