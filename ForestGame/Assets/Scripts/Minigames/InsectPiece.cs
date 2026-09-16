using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InsectPiece : MonoBehaviour, IPointerClickHandler
{
    public event Action<InsectPiece> OnSquashed;

    [SerializeField] private float hitboxPadding = 10f;

    private RectTransform rect, bounds;
    private Vector2 direction;
    private float speed;
    private bool squashed;

    public void Initialize(RectTransform bounds, Vector2 direction, float speed)
    {
        rect = GetComponent<RectTransform>();
        this.bounds = bounds;
        this.direction = direction;
        this.speed = speed;

        CreateHitbox();
    }

    private void CreateHitbox()
    {
        GameObject hitbox = new GameObject("Hitbox");

        hitbox.transform.SetParent(transform, false);

        RectTransform hitboxRect = hitbox.AddComponent<RectTransform>();
        hitboxRect.anchorMin = Vector2.zero;
        hitboxRect.anchorMax = Vector2.one;
        hitboxRect.offsetMin = new Vector2(-hitboxPadding, -hitboxPadding);
        hitboxRect.offsetMax = new Vector2(hitboxPadding, hitboxPadding);

        Image image = hitbox.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;
    }

    private void Update()
    {
        if (squashed) return;

        Vector2 pos = rect.anchoredPosition + direction * speed * Time.deltaTime;

        if (pos.x < bounds.rect.xMin || pos.x > bounds.rect.xMax)
            direction.x *= -1f;

        if (pos.y < bounds.rect.yMin || pos.y > bounds.rect.yMax)
            direction.y *= -1f;

        rect.anchoredPosition = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (squashed) return;

        squashed = true;
        OnSquashed?.Invoke(this);
        Destroy(gameObject);
    }
}