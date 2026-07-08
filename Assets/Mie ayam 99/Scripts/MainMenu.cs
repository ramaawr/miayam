using UnityEngine;
using UnityEngine.SceneManagement; // Dibutuhkan untuk memindahkan scene

// =============================================================================
// MainMenu.cs — Script untuk mengatur navigasi halaman utama (Main Menu)
// =============================================================================
// Script ini ditempelkan ke tombol atau objek di scene Main Menu.
// Saat tombol ditekan, script ini akan memindahkan scene ke gameplay.
//
// Standar Penulisan:
//   - Logika ramah pemula (Beginner-Friendly): method sederhana, referensi langsung.
//   - Mudah dikembangkan (Extensibility): nama scene dideklarasikan sebagai variabel public.
//   - Penjelasan lengkap (Inline Comments): menggunakan Bahasa Indonesia yang santai.
// =============================================================================
public class MainMenu : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene gameplay yang ingin dituju ketika tombol di-click")]
    public string namaSceneGameplay = "newest";

    // =========================================================================
    // OnMouseDown — Dipanggil otomatis oleh Unity saat objek ini diklik
    // (Bila tombol berupa sprite 2D biasa yang dipasangi Collider2D)
    // =========================================================================
    private void OnMouseDown()
    {
        MulaiGame();
    }

    // =========================================================================
    // MulaiGame — Fungsi utama untuk memuat scene kedua (gameplay)
    // Bisa dihubungkan ke event OnClick() pada UI Button di Canvas
    // =========================================================================
    public void MulaiGame()
    {
        Debug.Log("MainMenu: Memulai game, memuat scene: " + namaSceneGameplay);
        
        // Memuat scene kedua berdasarkan nama yang dimasukkan di Inspector
        SceneManager.LoadScene(namaSceneGameplay);
    }
}
