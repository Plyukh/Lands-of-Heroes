using UnityEngine;

public class HexInputController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask hexLayerMask;

    private void Update()
    {
        // Ловим левый клик мыши или первый тап
        if (Input.GetMouseButtonDown(0))
            TryHandleClick(Input.mousePosition);

        // Для тачей на мобильных
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            TryHandleClick(Input.touches[0].position);
    }

    private void TryHandleClick(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 100f, hexLayerMask))
        {
            if (hit.collider.TryGetComponent<HexCell>(out var cell))
                BattlefieldManager.Instance.OnCellClicked(cell);
        }
    }
}
