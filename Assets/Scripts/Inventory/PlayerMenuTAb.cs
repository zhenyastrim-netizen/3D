using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuTabs : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject inventoryTab;
    [SerializeField] private GameObject upgradesTab;
    [SerializeField] private GameObject characterTab;

    [Header("Buttons")]
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button characterButton;

    private void Awake()
    {
        inventoryButton.onClick.AddListener(OpenInventory);
        upgradesButton.onClick.AddListener(OpenUpgrades);
        characterButton.onClick.AddListener(OpenCharacter);
    }

    private void OnEnable()
    {
        OpenInventory();
    }

    public void OpenInventory()
    {
        SetActiveTab(inventoryTab);
    }

    public void OpenUpgrades()
    {
        SetActiveTab(upgradesTab);
    }

    public void OpenCharacter()
    {
        SetActiveTab(characterTab);
    }

    private void SetActiveTab(GameObject activeTab)
    {
        inventoryTab.SetActive(activeTab == inventoryTab);
        upgradesTab.SetActive(activeTab == upgradesTab);
        characterTab.SetActive(activeTab == characterTab);
    }

    private void OnDestroy()
    {
        inventoryButton.onClick.RemoveListener(OpenInventory);
        upgradesButton.onClick.RemoveListener(OpenUpgrades);
        characterButton.onClick.RemoveListener(OpenCharacter);
    }
}