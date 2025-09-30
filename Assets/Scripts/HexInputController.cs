using UnityEngine;

public class HexInputController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask hexLayerMask;
    [SerializeField] private LayerMask creatureLayerMask;

    private void Update()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            TryHandleClick(Input.touches[0].position);
    }

    private void TryHandleClick(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        // —начала провер€ем, попали ли по существу
        if (Physics.Raycast(ray, out var hitCreature, 100f, creatureLayerMask))
        {
            if (hitCreature.collider.TryGetComponent<Creature>(out var creature))
            {
                BattlefieldManager.Instance.OnCreatureClicked(creature);
                return;
            }
        }

        // »наче Ч тапаем по €чейке
        if (Physics.Raycast(ray, out var hit, 100f, hexLayerMask))
        {
            if (hit.collider.TryGetComponent<HexCell>(out var cell))
                BattlefieldManager.Instance.OnCellClicked(cell);
        }
    }
}
