#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using Unity.VisualScripting;
using UnityEngine.Assertions.Must;

public enum ConversionType
{ 
    Items,
    Dialogs
}
[Serializable]
public class DialogRowData
{
    public int? id;
    public string characterName;
    public string text;
    public int? nextId;
    public string protraitPath;
    public string choiceText;
    public int? choiceNextId;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePath = "";
    private string outputFolder = "Asset/ScriptableObjects/Items";
    private bool createdDatabase = true;
    private ConversionType conversionType = ConversionType.Items;

    [MenuItem("Tools/JSON to Scriptable Objects")]
    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }
    void OnGUI()
    {
        GUILayout.Label("JOS to Scriptable object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion : " , conversionType);

        if (conversionType == ConversionType.Items && outputFolder == "Assets/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Items";
        }
        else if (conversionType == ConversionType.Dialogs && outputFolder == "Assets/ScriptableObjects")
        {
            outputFolder = "Assets/ScriptableObjects/Items";
        }
        if (GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFilePath);
        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder : ", outputFolder);
        createdDatabase = EditorGUILayout.Toggle("Create Database Asset", createdDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Converter to Scriptable Objects"))
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Pease Select a JSON file first", "OK");
                return;
            }
            ConverterJsonToScriptableObjects();
        }
            
    }
    private void ConverterJsonToScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        //JSON 파일 열기
        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            //JSON 파싱
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();

            foreach (ItemData itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parseType))
                {
                    itemSO.itemType = parseType;
                }
                else
                {
                    Debug.LogWarning($"아이템 {itemData.itemName}의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"아이템 {itemData.nameEng}의 아이콘을 찾을 수 없습니다.: {itemData.iconPath}");
                    }
                }

                string assetPath = $"{outputFolder}/Item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                //이름 지정
                itemSO.name = $"{outputFolder}/Item_{itemData.id.ToString("D4")}_{itemData.nameEng}";
                createdItems.Add(itemSO );

                EditorUtility.SetDirty(itemSO);

                if (createdDatabase &&  createdItems.Count > 0)
                {
                    ItemDatabaseSO dataBase = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                    dataBase.items = createdItems;

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Sucess", $"Created {createdItems.Count} scriptable objects!", "OK");
                }
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Converter JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
    private void ConvertJsonToDialogScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        string JsonText = File.ReadAllText(jsonFilePath);

        try
        {
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(JsonText);

            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> creatDialogs = new List<DialogSO>();
            //대화 항목 생성
            foreach(var rowData in rowDataList)
            {
                //id 있는 행을 대화로 처리
                DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();

                //데이터 복사
                dialogSO.id = rowData.id.Value;
                dialogSO.characterName = rowData.characterName;
                dialogSO.text = rowData.text;
                dialogSO.nextId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                dialogSO.portraitPath = rowData.protraitPath;
                dialogSO.choices = new List<DialogChoiceSO>();

                //초상화 로드 (경로가 있을 경우)
                if (!string.IsNullOrEmpty(rowData.protraitPath))
                {
                    dialogSO.portrait = Resources.Load<Sprite>(rowData.protraitPath);

                    if (dialogSO.portrait == null)
                    {
                        Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                    }
                }
                dialogMap[dialogSO.id] = dialogSO;
                creatDialogs.Add(dialogSO);
            }
            foreach (var rowData in rowDataList)
            {
                //id가 없고 choiceText가 있는 행은 선택지로 처리
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    //이전 행의 ID를 부모ID로 사용 (연속 되는 선택지의 경우)
                    int parentId = -1;

                    //이 선택지 바로 위에 있는 대화(id가 있는 항목)를 찾음
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >=0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }
                    //부모 ID를 찾지 못했거나 부모 ID가 -1인 경우 (첫 번째 항목)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"선택지{rowData.choiceText}의 부모 대화를 찾을 수 없습니다.");
                    }
                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        //선택지 에셋 저장
                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                        EditorUtility.SetDirty(choiceSO);
                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}를 연결할 대화 (ID : {parentId}를 찾을 수 없습니다");
                    }
                }
            }
            //3단계 : 대화 스크립터블 오브젝트 저장
            foreach(var dialog in creatDialogs)
            {
                //스크립터블 오브젝트 저장 - ID 4자리 숫자
                string assetPath = $"{outputFolder}/Dialog {dialog.id.ToString("D4")}asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";
                EditorUtility.SetDirty (dialog);
            }
            //데이터 베이스 생성 해야함.
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error" , $"Failed to Convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
}
#endif