using UnityEngine.TerrainTools;
using UnityEngine.UI;

public class HuUIView : BaseUIView
{
    private Button CancelBtn;
    private Button ConfirmBtn;
    
    public override void OnAwake()
    {
        CancelBtn = transform.Find("Cancel").GetComponent<Button>();
        ConfirmBtn = transform.Find("Confirm").GetComponent<Button>();
    }

    public override void OnStart()
    {
        CancelBtn.onClick.AddListener(Cancel);
        ConfirmBtn.onClick.AddListener(Confirm);
    }

    public override void Show(object[] parameters)
    {
        
    }

    private void Cancel()
    {
        GameProgress.ChoiceResult = false;
        
        GameProgress.ChoiceOver = true;
    }

    private void Confirm()
    {
        GameProgress.ChoiceResult = true;
        
        GameProgress.ChoiceOver = true;
    }
}