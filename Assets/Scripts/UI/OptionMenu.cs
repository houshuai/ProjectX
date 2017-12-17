public class OptionMenu : Menu<OptionMenu>
{
    public void OnAudio()
    {
        AudioMenu.Open();
    }

    public void OnVisual()
    {
        VisualMenu.Open();
    }
}
