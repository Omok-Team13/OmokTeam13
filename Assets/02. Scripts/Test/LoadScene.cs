using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public ClothesManager clothes;

    public void OnClickLoadScene()
    {
        if (!clothes) clothes = FindFirstObjectByType<ClothesManager>();

        DontDestroyOnLoad(clothes.gameObject);

        SceneManager.LoadScene("Single Room");
    }
}
