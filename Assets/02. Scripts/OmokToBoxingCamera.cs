using UnityEngine;

public class OmokToBoxingCamera : MonoBehaviour
{
    public Throw throwComp; // 캐릭터에 붙은 Throw
    public WallAnimControll wallCtrl; // 벽 애니/전환 담당

    void Awake()
    {
        if (!throwComp) throwComp = FindFirstObjectByType<Throw>(FindObjectsInactive.Include);
        if (!wallCtrl) wallCtrl = FindFirstObjectByType<WallAnimControll>(FindObjectsInactive.Include);
    }

    public void CallThrow()
    {
        if (!throwComp) throwComp = FindFirstObjectByType<Throw>(FindObjectsInactive.Include);
        if (throwComp) throwComp.ThrowBoard();
    }

    public void WallFall()
    {
        if (!wallCtrl) wallCtrl = FindFirstObjectByType<WallAnimControll>(FindObjectsInactive.Include);
        if (wallCtrl) wallCtrl.WallFallOver();
    }
}
