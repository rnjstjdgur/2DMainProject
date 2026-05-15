using UnityEngine;

public enum DaniTechUIRootType
{
    None = 0,
    BackgroundUI,
    MainUI,
    ContentUI,
    PopupUI,
    VeryFrontUI
}

public enum DaniTechUIType
{
    DNSimplePopup,
    DNMainUI,
    DNMyProfilePopup, // 신규UI추가 1) 새로운 UIType을 추가한다
    DNInventory,
    DNLoadingUI,
    DNDialogueUI,
    DNInfoBookUI
}

public static class DaniTechUIManagerExtension
{
    public static string GetUIPath(this DaniTechUIManager uiManager, DaniTechUIRootType uiRootType, DaniTechUIType uiType)
    {
        string path = string.Empty; // "" == string.Empty

        // 신규UI추가 2) Resources.Load를 할 경로를 직접 명시한다
        // 해당 경로는 프로젝트창에서 Resources/Prefabs/UI폴더 내에 있는 RootType 폴더명과 UIType 프리팹 이름과 동일해야 한다! (ex. ContentUI/DNMyProfilePopup)
        path = $"Prefabs/UI/{uiRootType}/{uiType}";
        return path;
    }

    public static void ShowStartupUIOnGameStart(this DaniTechUIManager uiManager)
    {
        uiManager.OpenLoadingUI();
        uiManager.OpenUI(DaniTechUIRootType.MainUI, DaniTechUIType.DNMainUI);
        // 게임 로비 UI를 여기서 오픈해주자 -> uiManager.
        // MainUI도
    }

    public static void OpenSimplePopup(this DaniTechUIManager uiManager, string msg)
    {
        var uiBase = uiManager.OpenPopupUI(DaniTechUIType.DNSimplePopup);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }

        if (uiBase is DaniTech_SimplePopup simplePopup)
        {
            simplePopup.SetUI(msg);
        }
    }

    // 신규UI추가 3) 이렇게 어떤 팝업을 열고, 열때 전달해야하는 파라미터가 있다면 이렇게 전달한다.
        // 추가하기 편하게 그냥 빼둔 확장 메서드이므로, uiManager과 this는 우선 넘어가자
    public static void OpenMyProfilePopup(this DaniTechUIManager uiManager, string characterDataId)
    {
        // 신규UI추가 4) 이렇게 UI 타입을 던져서 UI 생성을 요청한다
        var uiBase = uiManager.OpenPopupUI(DaniTechUIType.DNMyProfilePopup);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }

        if (uiBase is DaniTech_MyProfilePopup myProfilePopup)
        {
            myProfilePopup.RefreshCharacterUI(characterDataId);
        }
    }

    public static void OpenInventoryPopup(this DaniTechUIManager uiManger)
    {
        var uiBase = uiManger.OpenContentUI(DaniTechUIType.DNInventory);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void OpenLoadingUI(this DaniTechUIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(DaniTechUIRootType.VeryFrontUI, DaniTechUIType.DNLoadingUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseLoadingUI(this DaniTechUIManager uiManager)
    {
        uiManager.CloseUI(DaniTechUIRootType.VeryFrontUI, DaniTechUIType.DNLoadingUI);
    }

    public static void OpenDialogueUI(this DaniTechUIManager uiManager, string startDialogueId)
    {
        var uiBase = uiManager.OpenContentUI(DaniTechUIType.DNDialogueUI);
        if(uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }

        if (uiBase is DaniTech_DialogueUI dialogueUi)
        {
            dialogueUi.StartDialogue(startDialogueId);
        }
    }
}

