using UnityEditor;
using UnityEngine;
using System.IO;

public static class MakePrefabIcons
{
    const int ICON_SIZE = 512;
    const string SAVE_DIR = "Assets/99.Resources/ClothesIcons";

    [MenuItem("Tools/Clothes/Make Icons From Selected Prefabs")]
    public static void MakeIcons()
    {
        if (!AssetDatabase.IsValidFolder(SAVE_DIR)) // ClothesIcons 폴더 여부 체크
        {
            Directory.CreateDirectory(SAVE_DIR); // 없으면 새로 생성
            AssetDatabase.Refresh(); // 에셋 새로고침 (없으면 유니티가 못찾을 수 있음)
        }

        // 아이콘 전용 레이어(씬에 배치된 다른 오브젝트까지 같이 캡쳐돼서 레이어 설정해줌
        int iconLayer = 30;

        foreach (var guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) continue;

            // --- 임시 루트/카메라/라이트 생성 ---
            var root = new GameObject("~IconBakeRoot")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);

            // 인스턴스 프리팹과 자식들을 iconLayer로 설정
            foreach (var t in inst.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = iconLayer;

            // 렌더러 바운즈 계산
            Renderer[] renderers = inst.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Object.DestroyImmediate(root); continue;
            }
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers)
            {
                b.Encapsulate(r.bounds);
            }

            // 카메라
            var camGO = new GameObject("~IconCam") { hideFlags = HideFlags.HideAndDontSave };
            camGO.transform.SetParent(root.transform);
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; // 카메라가 프레임을 그릴 때 배경을 단색으로 지우기
            cam.backgroundColor = new Color(0, 0, 0, 0); // 배경 투명
            cam.orthographic = false;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 100f;
            cam.fieldOfView = 30f;

            // 카메라는 오직 iconLayer만 찍도록
            cam.cullingMask = 1 << iconLayer;

            // 라이트
            var lightGO = new GameObject("~IconLight") { hideFlags = HideFlags.HideAndDontSave };
            lightGO.transform.SetParent(root.transform);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(45, 135, 0);

            // 카메라 위치(프레임에 꽉 차게)
            Vector3 center = b.center;
            float radius = b.extents.magnitude;
            float dist = radius / Mathf.Sin(Mathf.Deg2Rad * cam.fieldOfView * 0.5f);

            // 앞면 방향
            Vector3 dir = inst.transform.forward; // 정면에서 바라보도록

            // 약간의 여백(패딩) -> 토끼 귀나 쥐 의상은 패딩이 필요함 수동으로 조정해줘야할듯
            // float padding = 1.2f;
            // cam.transform.position = center + dir.normalized * dist * padding;
            cam.transform.position = center + dir.normalized * dist;
            cam.transform.LookAt(center);

            // 렌더 타겟
            var rt = new RenderTexture(ICON_SIZE, ICON_SIZE, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            // 렌더 → 텍스처 복사
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(ICON_SIZE, ICON_SIZE, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, ICON_SIZE, ICON_SIZE), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;

            // 저장
            string fileName = Path.GetFileNameWithoutExtension(path) + ".png";
            string savePath = Path.Combine(SAVE_DIR, fileName).Replace("\\", "/");
            File.WriteAllBytes(savePath, tex.EncodeToPNG()); // 파일 경로, 저장할 내용

            // 정리
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(root);

            // 스프라이트로 임포트 (Sprite Mode = Single)
            AssetDatabase.ImportAsset(savePath);
            var ti = (TextureImporter)AssetImporter.GetAtPath(savePath);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true; // 배경 투명
            ti.mipmapEnabled = false; // 밉맵 끄기
            ti.SaveAndReimport(); // 인스펙터창에서 옵션 바꾸고 Apply 누르는 것과 동일
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("프리팹->아이콘 전환", "아이콘 생성 완료", "OK");
    }
}
