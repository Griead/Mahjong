using UnityEngine.UI;

public class StartUIView :BaseUIView
{
    private Button StartButton;
    public override void OnAwake()
    {
        StartButton = transform.Find("StartButton").GetComponent<Button>();
    }

    public override void OnStart()
    {
        StartButton.onClick.AddListener(StartClick);
    }

    public override void Show(object[] parameters)
    {
    }

    private void StartClick()
    {
        MahjongGameManager.Instance.ReStart();
    }
}