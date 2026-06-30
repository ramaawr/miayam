using UnityEngine;
using UnityEngine.EventSystems;

// =============================================================================
// PindahKamerakegerobak.cs — Trigger Pindah ke Area Depan (Pelanggan)
// =============================================================================
// Script ini ditempel ke objek yang bisa diklik (misal: gerobak, pintu, dll)
// Saat diklik, kamera pindah ke Area Depan secara smooth lewat NavigasiKamera
//
// PERUBAHAN dari versi lama:
//   Dulu  → langsung set Camera.main.transform.position (teleport)
//   Sekarang → panggil NavigasiKamera.PindahKeDepan() (smooth lerp)
// =============================================================================

public class pindahkamerakegerobak : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    private void OnMouseDown()
    {
        // Cegah klik tembus ke objek di belakang UI (misal: Tombol UI menutupi area ini)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // Cari script NavigasiKamera yang ada di Main Camera
        NavigasiKamera kamera = FindObjectOfType<NavigasiKamera>();

        if (kamera != null)
        {
            // Pindah ke area depan (pelanggan) secara smooth
            kamera.PindahKeDepan();
            Debug.Log("masuk area gerobak bos");
        }
        else
        {
            // Fallback: kalau NavigasiKamera belum di-setup, pakai cara lama
            Camera.main.transform.position = new Vector3(-279.4427f, -474.2f, -116.2688f);
            Debug.LogWarning("NavigasiKamera tidak ditemukan! Pakai teleport langsung.");
        }
    }
}
