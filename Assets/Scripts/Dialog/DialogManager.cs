using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [Header("Dialog References")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button nextButton;

    private DialogSO currentDialog;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (dialogDatabase !=  null)
        {
            dialogDatabase.Initailize();
        }
        else
        {
            Debug.LogError("Dialog Database is not assigned to Dialog Manager");
        }
        if (nextButton != null)
        {
            //nextButton.onClick.AddListener(NextDialog);
        }
        else
        {
            Debug.LogError("Next Button is Not assigned!");
        }
    }
    void Start()
    {
        CloseDialog();
        StartDialog(1);
    }
    //ID로 대화 시작
    public void StartDialog(int dialogId)
    {
        DialogSO dialog = dialogDatabase.GetDialogByld(dialogId);
        if (dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogId} not found");
        }
    }
    //DialogSO로 대화 시작
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }
    public void NextDialog()
    {
        if (currentDialog != null && currentDialog.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogByld(currentDialog.nextId);
            if (nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }
    public void ShowDialog()
    {
        if (currentDialog == null) return;
        characterNameText.text = currentDialog.characterName;
        dialogText.text = currentDialog.text;
    }
    public void CloseDialog()
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
    }
}
