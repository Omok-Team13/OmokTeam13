using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;

public class ClothesUIButtonBinder : MonoBehaviour
{
    public GameManager.OnCustom gameManager; //게임매니저의 델리게이트
    public ClothesManager clothes;

    // Scroll View의 Content
    public Transform content;

    // 옷 선택 포인트
    public Dictionary<SlotType, Image> selectedBySlot = new ();
    public Color32 normalColor = new Color32(255, 255, 255, 150);
    public Color32 selectedColor = new Color32(255, 0, 0, 150);

    void Start()
    {
        BindAll();
        gameManager += OnClickSave; //함수 연결
    }

    void OnEnable()
    {
        if (!clothes) clothes = FindFirstObjectByType<ClothesManager>();
        if (clothes)
        {
            clothes.Unequipped += ClearSelection;
            clothes.UnequippedAll += ClearAllSelections;
        }
    }

    void OnDisable()
    {
        if (clothes)
        {
            clothes.Unequipped -= ClearSelection;
            clothes.UnequippedAll -= ClearAllSelections;
        }
    }
    public void BindAll()
    {
        if (!clothes) clothes = FindFirstObjectByType<ClothesManager>();
        if (!content) content = transform;
        if (!clothes)
        {
            Debug.LogWarning("[ClothesUIButtonBinder] ClothesManager 찾을 수 없음.");
            return;
        }

        var buttons = content.GetComponentsInChildren<Button>();
        int bound = 0, failed = 0;

        foreach (var btn in buttons)
        {
            string id = null;

            // Button의 Image 스프라이트 이름 사용
            var img = btn.GetComponent<Image>();
            var sprite = img ? img.sprite : null;
            if (sprite) id = sprite.name;

            if (string.IsNullOrEmpty(id))
            {
                failed++;
                continue;
            }

            var parentImage = btn.transform.parent ? btn.transform.parent.GetComponent<Image>() : null;

            // 미장착시 = 기본색
            if (parentImage) parentImage.color = normalColor;

            string captured = id; // 클로저 방지

            btn.onClick.AddListener(() =>
            {
                var catalog = clothes ? clothes.catalog : null;
                if (catalog == null || catalog.byId == null) return;
                if (!catalog.byId.TryGetValue(captured, out var info)) return;

                var slot = info.slot;

                // 이미 옷이 장착되어 있으면 해제
                bool alreadySelectedThis = selectedBySlot.TryGetValue(slot, out var prevImg) && prevImg == parentImage;
                if (alreadySelectedThis)
                {
                    clothes.Unequip(slot); // 슬롯 해제
                    ClearSelection(slot); // 색상 리셋(기본색)
                    return;
                }

                // 옷 장착 & 이미지 색상 포인트
                clothes.EquipById(captured);

                if (selectedBySlot.TryGetValue(slot, out var oldImg) && oldImg)
                    oldImg.color = normalColor;

                if (parentImage)
                {
                    parentImage.color = selectedColor;
                    selectedBySlot[slot] = parentImage;
                }
            });

            bound++;
        }

        Debug.Log($"[Button UI] 바인딩 완료 : {bound}개 / 실패 : {failed}개");
    }

    // 슬롯 비우기 & 전체 비우기
    public void ClearSelection(SlotType slot)
    {
        if (selectedBySlot.TryGetValue(slot, out var img) && img)
            img.color = normalColor;
        selectedBySlot.Remove(slot);
    }

    public void ClearAllSelections()
    {
        foreach (var kv in selectedBySlot)
            if (kv.Value) kv.Value.color = normalColor;
        selectedBySlot.Clear();
    }

    public void OnClickSave()
    {
        if (!clothes) clothes = FindFirstObjectByType<ClothesManager>();

        if (!GameSession.IsMultiplayer)
        {
            DontDestroyOnLoad(clothes.gameObject); // 싱글은 착장 그대로 오브젝트로 보내기
        }
        else
        {
            GameSession.OutfitIds = clothes.GetCurrentLoadoutIds(); // // 멀티는 오브젝트를 들고가지 않고 id 스냅샷만 저장
        }
    }
}
