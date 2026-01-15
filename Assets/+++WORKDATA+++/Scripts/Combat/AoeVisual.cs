using UnityEngine;

public class AoeVisual : MonoBehaviour
{
    [SerializeField] private AoeDamageArea aoe;
    [SerializeField] private float yOffset = 0.05f;
    [SerializeField] private float minRadiusToShow = 5f;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (!rend)
            Debug.LogError("[AoeVisual] Kein Renderer gefunden.", this);
    }

    void LateUpdate()
    {
        if (!aoe || !rend) return;

        float radius = aoe.GetRadius();

        // Sichtbarkeit abhängig vom Radius
        bool visible = radius >= minRadiusToShow;
        if (rend.enabled != visible)
            rend.enabled = visible;

        if (!visible)
            return;

        // Durchmesser = Radius * 2
        transform.localScale = new Vector3(
            radius * 2f,
            transform.localScale.y,
            radius * 2f
        );

        var p = transform.localPosition;
        p.y = yOffset;
        transform.localPosition = p;
    }
}


