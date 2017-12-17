using UnityEngine;
using UnityEngine.UI;

public class ArchiveMenu : Menu<ArchiveMenu>
{
    public Archive[] archives;

    private SelectArchive[] selectArchives;

    protected override void Awake()
    {
        base.Awake();

        selectArchives = GetComponentsInChildren<SelectArchive>();
        for (int i = 0; i < archives.Length; i++)
        {
            selectArchives[i].archive = archives[i];
            var texts = selectArchives[i].gameObject.GetComponentsInChildren<Text>();
            if (archives[i].isNew)
            {
                texts[0].text = "New";
                texts[1].rectTransform.sizeDelta = new Vector2(0, 0);
            }
            else
            {
                texts[0].text = archives[i].title;
                texts[1].text = archives[i].currScene;
            }
        }
    }

    public void OnArchive(Archive archive)
    {
        if (archive.isNew)
        {
            NewArchiveMenu.Open(archive);
        }
        else
        {
            MenuController.Instance.CloseAll();
            GameController.Instance.LoadGame(archive);
        }
    }
}
