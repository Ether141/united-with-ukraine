using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[GameManagerMember]
public class PlotItems : MonoBehaviour
{
    [SerializeField] private Image[] slots;

    public string[] Items { get; } = new string[3];
    public int ItemsCount => Items.Where(item => !string.IsNullOrEmpty(item)).Count();

    public bool HasCollectedItem(string itemName) => Items.Any(i => i == itemName);

    public void AddItem(string itemName, Sprite itemIcon)
    {
        if (ItemsCount >= 3)
            return;

        int index = ItemsCount;
        Items[index] = itemName;
        slots[index].sprite = itemIcon;
        slots[index].gameObject.SetActive(true);
    }
}
