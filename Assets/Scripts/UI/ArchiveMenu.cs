using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ArchiveMenu : Menu<ArchiveMenu>
{
    protected override void Awake()
    {
        base.Awake();

        Archive[] archives = null;
        var fileName = Application.persistentDataPath + "/saves.arc";
        if (File.Exists(fileName))
        {
            var bf = new BinaryFormatter();
            using (var file = File.Open(fileName, FileMode.Open))
            {
                archives = (Archive[])bf.Deserialize(file);
            }
        }
        else
        {
            archives = new Archive[] { new Archive(), new Archive(), new Archive() };
        }
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

    public void OnArchive()
    {
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
}
