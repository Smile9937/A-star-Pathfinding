using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownHandler : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private UnityEvent onClickEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        onClickEvent?.Invoke();
    }
}