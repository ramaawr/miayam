using UnityEngine;

// =============================================================================
// BackgroundParallax.cs — Script untuk efek gerak latar belakang berbasis mouse
// =============================================================================
// Tempelkan script ini ke background Main Menu (misal panel gambar atau sprite).
// Saat mouse digerakkan, background akan bergeser sedikit ke arah berlawanan,
// menciptakan efek kedalaman 3D (paralaks) yang dinamis dan premium.
// =============================================================================
public class BackgroundParallax : MonoBehaviour
{
    [Header("Pengaturan Paralaks")]
    [Tooltip("Kekuatan gerakan paralaks. Semakin besar nilainya, semakin jauh background bergeser.")]
    public float kekuatanParalaks = 30f;

    [Tooltip("Kecepatan penghalusan gerakan (smooth speed) agar tidak patah-patah.")]
    public float kecepatanPenghalusan = 5f;

    // Posisi awal objek transform sebelum paralaks aktif
    private Vector3 posisiAwal;

    private void Start()
    {
        // Simpan posisi lokal awal saat game dimulai
        posisiAwal = transform.localPosition;
    }

    private void Update()
    {
        // 1. Dapatkan posisi mouse saat ini di layar
        Vector3 posisiMouse = Input.mousePosition;

        // 2. Hitung offset/selisih mouse dari titik tengah layar
        // Nilainya akan berkisar antara -0.5 hingga 0.5
        float offsetX = (posisiMouse.x / Screen.width) - 0.5f;
        float offsetY = (posisiMouse.y / Screen.height) - 0.5f;

        // 3. Tentukan target posisi baru berdasarkan offset mouse
        // Kita gunakan perkalian minus (-) agar background bergerak berlawanan arah dengan mouse (paralaks alami)
        Vector3 targetPosisi = new Vector3(
            posisiAwal.x - (offsetX * kekuatanParalaks),
            posisiAwal.y - (offsetY * kekuatanParalaks),
            posisiAwal.z
        );

        // 4. Geser posisi objek secara halus dari posisi saat ini ke target posisi
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosisi, Time.deltaTime * kecepatanPenghalusan);
    }
}
