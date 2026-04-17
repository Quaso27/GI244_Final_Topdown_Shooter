using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public Slider soundSlider;
    public Slider musicSlider;
    public GameObject optionsPanel;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
                ResumeGame();
            else
                OpenOptions();
        }
    }
    // ฟังก์ชันสำหรับปุ่ม Resume
    public void ResumeGame()
    {
        optionsPanel.SetActive(false);
        Time.timeScale = 1f; // กลับมาเดินเกมต่อ
    }

    // ฟังก์ชันสำหรับปุ่มเปิดหน้า Options (เรียกจากปุ่ม Gear ในหน้าหลัก)
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        Time.timeScale = 0f; // หยุดเกมชั่วคราวขณะตั้งค่า
    }

    // ฟังก์ชันสำหรับปุ่ม Quit
    public void QuitGame()
    {
        Application.Quit(); // ปิดเกม (ใช้ได้ตอน Build ออกมาแล้ว)
        Debug.Log("Quit Game");
    }
}