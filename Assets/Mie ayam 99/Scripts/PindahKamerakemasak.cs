using UnityEngine;
using UnityEngine.EventSystems;

// =============================================================================
// PindahKamerakemasak.cs — Trigger Pindah ke Area Dapur (Masak)
// =============================================================================
// Script ini ditempel ke objek yang bisa diklik (misal: kompor, pintu, dll)
// Saat diklik, kamera pindah ke Area Dapur secara smooth lewat NavigasiKamera
//
// PERUBAHAN dari versi lama:
//   Dulu  → langsung set Camera.main.transform.position (teleport)
//   Sekarang → panggil NavigasiKamera.PindahKeDapur() (smooth lerp)
// =============================================================================

public class PindahKamerakemasak : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    private void OnMouseDown()
    {
        // TAMENG PELINDUNG: Cegah klik dapur jika buku harian sedang terbuka
        if (DiaryManager.instance != null && DiaryManager.instance.IsBukuHarianTerbuka)
        {
            return;
        }

        // Cegah klik tembus ke objek di belakang UI (misal: Tombol Next Day menutupi area ini)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // Cari script NavigasiKamera yang ada di Main Camera
        NavigasiKamera kamera = FindObjectOfType<NavigasiKamera>();

        if (kamera != null)
        {
            // Pindah ke area dapur (masak) secara smooth
            kamera.PindahKeDapur();
            Debug.Log("masuk area masak bos");
        }
        else
        {
            // Fallback: kalau NavigasiKamera belum di-setup, pakai cara lama
            Camera.main.transform.position = new Vector3(-279.4427f, -78.2141f, -116.2688f);
            Debug.LogWarning("NavigasiKamera tidak ditemukan! Pakai teleport langsung.");
        }
    }
}
