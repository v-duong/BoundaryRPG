using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroArchetypeHeader : MonoBehaviour
{
    [SerializeField]
    private Image header;

    [SerializeField]
    private TextMeshProUGUI nameText;

    public Image lowerBannerImage;

    private HeroArchetypeData archetypeData;

    public void SetArchetype(HeroArchetypeData archetypeData)
    {
        this.archetypeData = archetypeData;
        header.color = Helpers.GetArchetypeStatColor(archetypeData.Base);
        nameText.text = archetypeData.Base.LocalizedName;
    }

    public void OpenArchetypeWindow()
    {
        MenuUIManager.Instance.OpenArchetypeWindow(archetypeData.Base, archetypeData);
    }
}