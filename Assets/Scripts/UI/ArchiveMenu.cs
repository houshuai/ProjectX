using UnityEngine;
using UnityEngine.UI;

public class ArchiveMenu : Menu<ArchiveMenu>
{
    private Archive[] archives;

    protected override void Awake()
    {
        base.Awake();

        archives = Archive.Load();
        GameController.Instance.archives = archives;

        var selectArchives = GetComponentsInChildren<SelectArchive>();
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

    public void OnOk()
    {
        if (Archive.current == null)
        {
            return;
        }

        if (Archive.current.isNew)
        {
            NewArchiveMenu.Open();
        }
        else
        {
            MenuController.Instance.CloseAll();
            GameController.Instance.LoadGame();
        }
    }

    public void OnDelete()
    {
        if (Archive.current == null)
        {
            return;
        }

        Archive.current.Initial();
        var selectArchives = GetComponentsInChildren<SelectArchive>();
        for (int i = 0; i < selectArchives.Length; i++)
        {
            if (Archive.current == selectArchives[i].archive)
            {
                var texts = selectArchives[i].gameObject.GetComponentsInChildren<Text>();
                texts[0].text = "New";
                texts[1].rectTransform.sizeDelta = new Vector2(0, 0);
            }
        }
        Archive.Save(archives);
    }
}
