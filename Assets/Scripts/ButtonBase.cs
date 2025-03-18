using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBase : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Suspect _assignedSuspect;

    public void SetAssignedSuspect(Suspect assignedSuspect)
    {
        _assignedSuspect = assignedSuspect;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        Debug.Log($"{name} was deselected");
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log($"{name} was selected");
        CaseManager.Instance.SuspectSelected(_assignedSuspect);
    }
}
