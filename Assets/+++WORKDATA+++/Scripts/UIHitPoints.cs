using TMPro;
using UnityEngine;

public class UIHitPoints : MonoBehaviour
{
    [SerializeField] private TMP_Text hitpointsTextmesh;

    

    private void Awake()
    {
        if (hitpointsTextmesh == null)
            hitpointsTextmesh = GetComponentInChildren<TMP_Text>();
    }

    public void UpdateHitpoints(int newHitpoints)
    {
        if (hitpointsTextmesh != null)
            hitpointsTextmesh.text = newHitpoints.ToString();
    }
}
