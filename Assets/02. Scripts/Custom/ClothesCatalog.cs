using System.Collections.Generic;
using UnityEngine;

public class ClothesCatalog : MonoBehaviour
{
    // 폴더명 → 슬롯 매핑
    static readonly Dictionary<string, SlotType> FolderToSlot = new()
    {
        { "Costumes",          SlotType.FullBody },
        { "Face Accessories",  SlotType.FaceAccessory },
        { "Glasses",           SlotType.Glasses },
        { "Gloves",            SlotType.Gloves },
        { "Hairstyle",         SlotType.Hair },
        { "Hairstyle Single",  SlotType.Hair },
        { "Hat",               SlotType.Hat },
        { "Hat Single",        SlotType.Hat },
        { "Outwear",           SlotType.Top },
        { "Pants",             SlotType.Bottom },
        { "Shoes",             SlotType.Shoes },
        { "Shorts",            SlotType.Bottom },
        { "Socks",             SlotType.Shoes },
        // 제외: Body / Faces / Mascots(Costumes 합침)
    };

    public class ItemInfo
    {
        public string id; // 아이콘/프리팹 파일명
        public SlotType slot; // 부위 (중복 방지용)
        public GameObject prefab; // 사용할 프리팹
    }

    // itemId → ItemInfo
    public Dictionary<string, ItemInfo> byId{ get; private set; } = new();

    void Start()
    {
        Build();
    }

    public void Build()
    {
        byId.Clear();

        foreach (var kv in FolderToSlot) // 폴더명(Key)과 해당 슬롯타입(Value)을 하나씩 꺼냄
        {
            string folderName = kv.Key; // 예: "Glasses"
            SlotType slot = kv.Value; // 예: SlotType.Glasses

            var prefabs = Resources.LoadAll<GameObject>(folderName); // 해당 폴더 안에 있는 모든 프리팹 불러오기

            foreach (var go in prefabs) // 불러온 프리팹들에 ItemInfo 값 적용
            {
                if (!go) continue;
                string id = go.name;

                byId[id] = new ItemInfo
                {
                    id = id,
                    slot = slot,
                    prefab = go
                };
            }
        }

        Debug.Log($"[Catalog] 로드된 프리팹(의상) 수 : {byId.Count}");
    }
}