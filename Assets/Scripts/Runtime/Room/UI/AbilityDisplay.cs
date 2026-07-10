using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AbilityDisplay : MonoBehaviour, IEventListener<AbilityChangedEvent>, IEventListener<AbilityStateChangedEvent>
{
    [SerializeField] private Image icon;
    [SerializeField] private List<GameObject> charges;

    private void OnEnable()
    {
        EventBus<AbilityChangedEvent>.Register(this);
        EventBus<AbilityStateChangedEvent>.Register(this);
    }

    private void OnDisable()
    {
        EventBus<AbilityChangedEvent>.Unregister(this);
        EventBus<AbilityStateChangedEvent>.Unregister(this);
    }

    private void Awake()
    {
        foreach (var charge in charges)
        {
            charge.SetActive(false);
        }
    }

    public void OnEvent(AbilityChangedEvent e)
    {
        icon .sprite = e.Icon;
        for (int i = 0; i < e.Charges; i++)
        {
            if (i < charges.Count)
            {
                charges[i].SetActive(true);
            }
        }
    }

    public void OnEvent(AbilityStateChangedEvent e)
    {
        for (int i = 0; i < charges.Count; i++)
        {
            charges[i].transform.GetChild(0).gameObject.SetActive(i < e.Charges);
        }
    }
}
