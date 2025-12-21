using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CombatInfo : MonoBehaviour
{
    [SerializeField] private GameObject combatCardIconPrefab;
    [SerializeField] private GameObject cardIconRendering;
    private List<ActionClass> combatCards = new();
    private ActionClass activeActionClass;
    public HorizontalLayoutGroup buffList;
    public Animator diceAnimator;
    public SpriteRenderer diceRollSprite;
    public List<Sprite> loadedSprites = new();
    public GameObject diceRollText;
    public GameObject buffIconPrefab;
    public HealthBar healthBar;
    public GameObject crosshair;
    public TMP_Text damagePopupText;
    private float ROTATION_SPEED = 30f;

    public Canvas buffListCanvas;

    private int FadeSortingOrder => CombatFadeScreenHandler.Instance.FADE_SORTING_ORDER;
    private string FadeSortingLayer => CombatFadeScreenHandler.Instance.FADE_SORTING_LAYER;

    public void Awake()
    {
        buffListCanvas = buffList.gameObject.GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        EntityClass.OnEntityDeath += RemoveCardFromTarget;
    }

    private void OnDisable()
    {
        EntityClass.OnEntityDeath -= RemoveCardFromTarget;
    }

    void RemoveCardFromTarget(EntityClass entity)
    {
        List<ActionClass> removedActions = combatCards.Where(card => card.Target == entity).ToList();
        foreach (ActionClass action in removedActions)
        {
            DeactivateCombatSprite(action);
        }
    }


    public void Start()
    {
        diceRollText.GetComponent<MeshRenderer>().sortingOrder = diceRollSprite.sortingOrder + 1;
        buffListCanvas.overrideSorting = true;
        buffListCanvas.sortingLayerName = FadeSortingLayer;
        buffListCanvas.sortingOrder = -1;
        diceRollSprite.sortingLayerName = FadeSortingLayer;
        diceRollText.GetComponent<MeshRenderer>().sortingLayerName = FadeSortingLayer;
        damagePopupText.GetComponent<MeshRenderer>().sortingLayerName = FadeSortingLayer;
    }

    public void DisplayDamage(int damage)
    {
        damagePopupText.text = "-" + damage.ToString();
        damagePopupText.gameObject.SetActive(true);
        Color originalColor = damagePopupText.color;
        damagePopupText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        StartCoroutine(FadeText(damagePopupText, 1f));
    }

    IEnumerator FadeText(TMP_Text textObject, float fadeDuration)
    {
        Color originalColor = textObject.color;
        float elapsedTime = 0f;

        float durationAtFullOpacity = 0.2f;
        yield return new WaitForSeconds(durationAtFullOpacity);

        fadeDuration = fadeDuration - durationAtFullOpacity;
        // Loop over the duration of the fade
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;

            // Use Color.Lerp to change the alpha of the text color
            textObject.color = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0), normalizedTime);

            yield return null;
        }
        damagePopupText.gameObject.SetActive(false);
    }

    public void Update()
    {
        crosshair.transform.Rotate(Vector3.forward * ROTATION_SPEED * Time.deltaTime);
    }

    /* 
     Ideally sets the rolled dice value to the sprite of the value, since thats not available it uses a default sprite.
     */
    public void SetDice(int value)
    {
        diceAnimator.enabled = false;
        diceRollSprite.sprite = loadedSprites[0];
        diceRollText.GetComponent<TMP_Text>().text = value.ToString();
    }

    // Sets the appropriate color of the dice
    public void setDiceColor(Color color)
    {
        diceRollText.GetComponent<TMP_Text>().color = color;
    }

    public void ActivateCombatSprite(ActionClass actionClass)
    {
        diceAnimator.enabled = true;
        diceRollText.GetComponent<TextMeshPro>().text = null;
        if (!combatCards.Contains(actionClass))
        {
            combatCards.Add(actionClass);
            RenderCombatIcons();
        }
        activeActionClass = actionClass;
    }

    public void AddCombatSprite(ActionClass actionClass)
    {
        combatCards.Add(actionClass);
        RenderCombatIcons();
    }
    public void DeactivateCombatSprite(ActionClass actionClass)
    {
        combatCards.Remove(actionClass);
        RenderCombatIcons();
    }


    private void RenderCombatIcons()
    {
        int num = combatCards.Count;
        float iconHeight = combatCardIconPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
        float totalHeight = num * iconHeight;
        float startY = totalHeight / 2 - iconHeight / 2;

        UnrenderCombatIcons();
        for (int i = 0; i < num; i++)
        {
            GameObject combatIcon = Instantiate(combatCardIconPrefab);
            combatIcon.transform.SetParent(cardIconRendering.transform);
            combatIcon.transform.localScale = Vector3.one;
            combatIcon.transform.localPosition = new Vector3(0, startY - i * iconHeight, 0);
            combatIcon.GetComponent<CombatCardUI>().SetActionClass(combatCards[num - i - 1]); //Reverse the order of rendering 
            combatIcon.GetComponent<CombatCardUI>().DeEmphasize();
        }
    }
    private void UnrenderCombatIcons()
    {
        foreach (Transform child in cardIconRendering.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void EmphasizeCombatIcon()
    {
        bool onlyHighlightOne = true;
        foreach (Transform child in cardIconRendering.transform)
        {
            CombatCardUI combatUI = child.GetComponent<CombatCardUI>();
            if (child.GetComponent<CombatCardUI>().ActionClass == activeActionClass && onlyHighlightOne)
            {
                combatUI.Emphasize();
                onlyHighlightOne = false;
            }
            else
            {
                combatUI.DeEmphasize();
            }
        }
    }

    public void EnableDice()
    {
        diceAnimator.enabled = true;
        diceRollSprite.enabled = true;
        diceRollText.GetComponent<TMP_Text>().enabled = true;
    }

    public void DisableDice()
    {
        diceAnimator.enabled = false;
        diceRollSprite.enabled = false;
        diceRollText.GetComponent<TMP_Text>().enabled = false;
        diceRollText.GetComponent<TextMeshPro>().text = null;
    }

    public void EnableHealthBar()
    {
        healthBar.gameObject.SetActive(true);
    }

    public void DisableHealthBar()
    {
        healthBar.gameObject.SetActive(false);
    }

    public void ActivateCrosshair()
    {
        if (!crosshair.activeSelf)
        {
            crosshair.SetActive(true);
        }
    }

    public void DeactivateCrosshair()
    {
        if (crosshair.activeSelf)
        {
            crosshair.SetActive(false);
        }
    }

    public void DeactivateCardIcon()
    {
        cardIconRendering.SetActive(false);
    }

    public void ActivateCardIcon()
    {
        cardIconRendering.SetActive(true);
    }

    public void Emphasize()
    {
        EmphasizeCombatIcon();
        damagePopupText.GetComponent<MeshRenderer>().sortingOrder = FadeSortingOrder + 1;
        diceRollSprite.sortingOrder = FadeSortingOrder + 1;
        buffListCanvas.sortingOrder = FadeSortingOrder + 2;
        diceRollText.GetComponent<MeshRenderer>().sortingOrder = diceRollSprite.sortingOrder + 1;
        healthBar.Emphasize();
    }
    public void DeEmphasize()
    {
        foreach (Transform child in cardIconRendering.transform)
        {
            child.GetComponent<CombatCardUI>().DeEmphasize();
        }
        damagePopupText.GetComponent<MeshRenderer>().sortingOrder = FadeSortingOrder - 1;
        diceRollSprite.sortingOrder = FadeSortingOrder - 1;
        buffListCanvas.sortingOrder = FadeSortingOrder - 1;
        diceRollText.GetComponent<MeshRenderer>().sortingOrder = FadeSortingOrder - 1;
        healthBar.DeEmphasize();
    }

    public void SetMaxHealth(int maxHealth)
    {
        healthBar.setMaxHealth(maxHealth);
    }

    public void SetHealth(int health)
    {
        healthBar.setHealth(health);
    }
    //Flips the CombatInfo so that the Icon is on the Right of the entity
    public void FaceLeft()
    {
        FlipTransform(cardIconRendering.transform, false);
        FlipTransform(damagePopupText.transform, false);
        FlipTransform(diceRollText.transform, false);
        FlipTransform(healthBar.transform, false);

        foreach (Transform child in buffList.transform)
        {
            FlipTransform(child, false);
        }

        foreach (Transform cardUI in cardIconRendering.transform)
        {
            CombatCardUI cardIcon = cardUI.GetComponent<CombatCardUI>();
            cardIcon.FaceLeft();
        }
        diceRollSprite.flipX = true;

        CombatManager.Instance.UpdateCameraBounds(); //Bad placement here
    }
    //Flips the CombatInfo so that the Icon is on the LEFT of the entity
    public void FaceRight()
    {
        FlipTransform(cardIconRendering.transform, true);
        FlipTransform(damagePopupText.transform, true);
        FlipTransform(diceRollText.transform, true);
        FlipTransform(healthBar.transform, true);

        foreach (Transform child in buffList.transform)
        {
            FlipTransform(child, true);
        }


        foreach (Transform cardUI in cardIconRendering.transform)
        {
            CombatCardUI cardIcon = cardUI.GetComponent<CombatCardUI>();
            cardIcon.FaceRight();
        }
        diceRollSprite.flipX = false;

        CombatManager.Instance.UpdateCameraBounds(); //Bad placement here, but I cant think of where else id put it

    }

    public void FlipTransform(Transform transform, bool faceRight)
    {
        if (faceRight) //Face Right
        {
            Vector3 flippedTransform = transform.localScale;
            flippedTransform.x = Mathf.Abs(flippedTransform.x);
            transform.localScale = flippedTransform;
        }
        else
        {
            Vector3 flippedTransform = transform.localScale;
            flippedTransform.x = -Mathf.Abs(flippedTransform.x);
            transform.localScale = flippedTransform;
        }
    }

    private bool IsFacingRight()
    {
        return this.gameObject.transform.lossyScale.x > 0;
    }

    public void UpdateBuffs(Dictionary<string, StatusEffect> buffs)
    {
        foreach (Transform child in buffList.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (string str in buffs.Keys)
        {
            if (buffs[str].Stacks == 0) continue;

            GameObject instance = Instantiate(buffIconPrefab);
            BuffIcons buffIcon = instance.GetComponent<BuffIcons>();
            buffIcon.transform.SetParent(buffList.transform, false);
            buffIcon.SetIcon(buffs[str].GetIcon());
            buffIcon.SetText(buffs[str].Stacks.ToString());
            buffIcon.buffName = str;
            buffIcon.stacks = buffs[str].Stacks;
            if (IsFacingRight())
            {
                FlipTransform(buffIcon.transform, true);
            }
            else
            {
                FlipTransform(buffIcon.transform, false);
            }
        }
    }
}
