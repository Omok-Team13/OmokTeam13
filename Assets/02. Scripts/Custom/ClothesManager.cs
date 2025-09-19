using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClothesManager : MonoBehaviour
{
    public Transform characterRoot;
    public Animator targetAnimator;
    public ClothesCatalog catalog;

    // 슬롯별 현재 장착 인스턴스 보관(중복 방지)
    private readonly Dictionary<SlotType, GameObject> equipped = new();

    // 슬롯 선택/해지 포인트용
    public event System.Action<SlotType> Unequipped;
    public event System.Action UnequippedAll;

    // FullBody가 착용 중일 때 금지되는 슬롯
    public static readonly HashSet<SlotType> BlockedWhenFullBodyOn = new()
    {
        SlotType.Top, SlotType.Bottom
    };

    // 멀티플레이용 착장 정보 저장
    private readonly Dictionary<SlotType, string> equippedIds = new();

    void Start()
    {
        characterRoot = transform;
        catalog = FindFirstObjectByType<ClothesCatalog>();
    }

    // 아이템 ID 유무 확인 후 옷 입히기 함수 호출
    public void EquipById(string itemId)
    {
        if (!catalog || catalog.byId == null || !catalog.byId.TryGetValue(itemId, out var info))
        {
            Debug.LogWarning($"찾을 수 없는 의류 id: {itemId}");
            return;
        }

        var slot = info.slot;

        // FullBody가 입어져 있을 때 장착하려는 슬롯이 충돌 슬롯이면
        if (IsFullBodyOn && BlockedWhenFullBodyOn.Contains(slot))
            Unequip(SlotType.FullBody); // FullBody 벗기고 옷 입히기

        // FullBody 입힐 때 다른 의상 착용 먼저 해제
        if (slot == SlotType.FullBody)
        {
            foreach (var s in BlockedWhenFullBodyOn)
                Unequip(s);
        }

        Equip(info.prefab, info.slot);

        equippedIds[info.slot] = itemId; // 착장 기억
    }

    // 프리팹을 슬롯에 장착
    public void Equip(GameObject prefab, SlotType slot)
    {
        if (!prefab) return;

        if (!characterRoot) characterRoot = transform;

        // 같은 슬롯 기존 장착 제거(중복 방지)
        if (equipped.TryGetValue(slot, out var old) && old)
        {
            Destroy(old);
            equipped.Remove(slot);
        }

        // 새 인스턴스 생성 (Character 아래)
        var inst = Instantiate(prefab, characterRoot);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        // SkinnedMeshRenderer가 있으면 본 바인딩, 없으면 적절한 본에 부착
        var smrs = inst.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (smrs.Length > 0)
        {
            foreach (var smr in smrs)
                BindToSkeleton(smr, targetAnimator);
        }
        else
        {
            var parentBone = GetAttachBoneForSlot(slot, targetAnimator) ?? characterRoot;
            inst.transform.SetParent(parentBone, worldPositionStays: false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            inst.transform.localScale = Vector3.one;
        }

        equipped[slot] = inst;
    }

    //  슬롯 비우기 & 전체 비우기
    public void Unequip(SlotType slot)
    {
        if (equipped.TryGetValue(slot, out var go) && go)
            Destroy(go);
        equipped.Remove(slot);
        Unequipped?.Invoke(slot);
        equippedIds.Remove(slot);
    }

    public void UnequipAll()
    {
        foreach (var kv in equipped)
            if (kv.Value) Destroy(kv.Value);
        equipped.Clear();
        UnequippedAll?.Invoke();
        equippedIds.Clear();
    }

    // 본 바인딩 : 의상에 있는 본을 캐릭터 본으로 갈아끼우기
    public static void BindToSkeleton(SkinnedMeshRenderer clothing, Animator target)
    {
        if (!clothing || !target) return;

        // 옷 인스턴스 하위 트랜스폼은 boneMap에서 제외
        var clothingSet = new HashSet<Transform>(clothing.transform.GetComponentsInChildren<Transform>(true));

        // 캐릭터 쪽 본들만 이름→본 딕셔너리로 (중복 이름은 첫 번째 것만 사용)
        var boneMap = new Dictionary<string, Transform>(System.StringComparer.Ordinal);
        foreach (var t in target.transform.GetComponentsInChildren<Transform>(true))
        {
            if (clothingSet.Contains(t)) continue; // 옷 쪽 본 제외
            if (!boneMap.ContainsKey(t.name))
                boneMap.Add(t.name, t); // 중복 방지
        }

        // rootBone 매칭 (이름이 같으면 그대로 / 없으면 Hips 폴백)
        if (clothing.rootBone && boneMap.TryGetValue(clothing.rootBone.name, out var mappedRoot))
        {
            clothing.rootBone = mappedRoot;
        }
        else
        {
            var hips = target.GetBoneTransform(HumanBodyBones.Hips);
            if (hips) clothing.rootBone = hips;
        }

        // bones 배열 전부 이름 매칭 (없으면 rootBone으로 폴백)
        var src = clothing.bones; // 스킨 메시가 사용하는 모든 뼈 Transform들의 배열
        var dst = new Transform[src.Length]; // src와 같은 크기의 빈 배열 생성
        for (int i = 0; i < src.Length; i++)
        {
            var s = src[i];
            if (s && boneMap.TryGetValue(s.name, out var mapped))
                dst[i] = mapped;
            else
                dst[i] = clothing.rootBone; // 안전 폴백
        }
        clothing.bones = dst;
    }


    // 스킨 없는 액세서리의 기본 부착 본 선택
    static Transform GetAttachBoneForSlot(SlotType slot, Animator anim)
    {
        if (!anim) return null;
        return slot switch
        {
            SlotType.Hat or SlotType.Hair or SlotType.Glasses or SlotType.FaceAccessory
                => anim.GetBoneTransform(HumanBodyBones.Head),
            SlotType.Top
                => anim.GetBoneTransform(HumanBodyBones.UpperChest)
                ?? anim.GetBoneTransform(HumanBodyBones.Chest)
                ?? anim.GetBoneTransform(HumanBodyBones.Spine),
            SlotType.Bottom
                => anim.GetBoneTransform(HumanBodyBones.Hips),
            SlotType.Shoes
                => anim.GetBoneTransform(HumanBodyBones.LeftFoot)
                ?? anim.GetBoneTransform(HumanBodyBones.Hips),
            SlotType.Faces
                => anim.GetBoneTransform(HumanBodyBones.Head),
            _ => null
        };
    }

    public string[] GetCurrentLoadoutIds()
    {
        return equippedIds.Values.ToArray();
    }

    public bool IsFullBodyOn =>
    equipped.TryGetValue(SlotType.FullBody, out var fb) && fb;
}
