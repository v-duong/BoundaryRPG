using UnityEngine;
using UnityEngine.UI;

public class HeroDetailWindow : MonoBehaviour
{
    [SerializeField]
    private ScrollRect mainScrollRect;

    [SerializeField]
    private HeroMainDetailsPage heroMainDetailsPage;

    [SerializeField]
    public HeroEquipmentPage heroEquipmentPage;

    [SerializeField]
    private Button detailsPageButton;

    [SerializeField]
    private Button equipmentPageButton;

    [SerializeField]
    private Button abilitiesPageButton;

    [SerializeField]
    private Button bonusesPageButton;

    private GameObject lastActivePage;
    private Button lastActiveButton;

    private Hero hero;

    public void OpenWindow(Hero hero)
    {
        this.hero = hero;
        OpenDetailsPage();
        MenuUIManager.Instance.OpenWindow(this.gameObject);
    }

    private void ResetPages()
    {
        if (lastActivePage != null)
            lastActivePage.gameObject.SetActive(false);
        if (lastActiveButton != null)
            lastActiveButton.image.color = new Color(0.282353f, 0.282353f, 0.282353f);
    }

    private void SetActiveButton(Button button)
    {
        lastActiveButton = button;
        button.image.color = new Color(0.46666f, 0.46666f, 0.46666f);
    }

    public void OpenDetailsPage()
    {
        ResetPages();
        lastActivePage = heroMainDetailsPage.gameObject;
        SetActiveButton(detailsPageButton);
        mainScrollRect.content = heroMainDetailsPage.transform as RectTransform;

        heroMainDetailsPage.ShowPage(hero);
    }

    public void OpenEquipmentPage()
    {
        ResetPages();
        lastActivePage = heroEquipmentPage.gameObject;
        SetActiveButton(equipmentPageButton);
        mainScrollRect.content = heroEquipmentPage.transform as RectTransform;

        heroEquipmentPage.ShowPage(hero);
    }
}