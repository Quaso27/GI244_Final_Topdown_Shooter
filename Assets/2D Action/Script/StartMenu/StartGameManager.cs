using UnityEngine;
using UnityEngine.SceneManagement; // สำคัญมาก: ใช้สำหรับเปลี่ยนฉาก

public class StartGameManager : MonoBehaviour
{
    // ตั้งชื่อฉากที่ต้องการให้โหลดหลังจากคลิก (เช่น SampleScene)
    public string sceneToLoad = "Game";

    void Update()
    {
        // เช็กว่าผู้เล่นคลิกเมาส์ซ้ายหรือเปล่า
        if (Input.GetMouseButtonDown(0))
        {
            // สั่งให้โหลดฉากถัดไป
            LoadGameScene();
        }
    }

    void LoadGameScene()
    {
        // สั่งให้โหลดฉากที่ตั้งชื่อไว้
        SceneManager.LoadScene(sceneToLoad);
    }
}