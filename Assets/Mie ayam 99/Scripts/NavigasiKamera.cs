using UnityEngine;

// =============================================================================
// NavigasiKamera.cs — Pengatur Perpindahan Kamera Antar Area (Smooth)
// =============================================================================
// Script ini ditempel ke Main Camera.
// Menggantikan sistem teleport kamera lama dengan gerakan smooth (Lerp).
//
// Cara kerja:
//   1. Ada 2 posisi tujuan: PosisiDapur dan PosisiDepan
//   2. Saat fungsi PindahKeDapur() atau PindahKeDepan() dipanggil,
//      kamera mulai bergerak smooth menuju tujuan
//   3. Selama bergerak, input pemain diblokir (SedangTransisi = true)
//      supaya tidak bisa klik apa-apa saat kamera gerak
//
// Cara pakai dari script lain:
//   NavigasiKamera kamera = FindObjectOfType<NavigasiKamera>();
//   kamera.PindahKeDepan();
//
// Atau kalau sudah di-assign lewat Inspector:
//   navigasiKamera.PindahKeDapur();
// =============================================================================

public class NavigasiKamera : MonoBehaviour
{
    // =========================================================================
    // POSISI TUJUAN — Diatur dari Inspector sesuai layout scene
    // Isi dengan koordinat kamera yang sudah ada di scene
    // =========================================================================
    [Header("Posisi Kamera Tiap Area")]
    [Tooltip("Posisi kamera saat di area dapur/masak")]
    public Vector3 PosisiDapur = new Vector3(-279.4427f, -78.2141f, -116.2688f);

    [Tooltip("Posisi kamera saat di area depan/pelanggan (gerobak)")]
    public Vector3 PosisiDepan = new Vector3(-279.4427f, -474.2f, -116.2688f);

    // =========================================================================
    // KECEPATAN TRANSISI — Makin besar angkanya, makin cepat gerak kamera
    // Bisa di-tweak dari Inspector sampai terasa pas
    // =========================================================================
    [Header("Pengaturan Transisi")]
    [Tooltip("Kecepatan perpindahan kamera (semakin besar = semakin cepat)")]
    public float KecepatanTransisi = 5f;

    // Jarak minimum untuk dianggap "sudah sampai" di tujuan
    // Pakai angka kecil supaya tidak perlu posisi 100% persis
    [Tooltip("Jarak minimum agar kamera dianggap sudah sampai")]
    public float JarakMinimumSampai = 0.05f;

    // =========================================================================
    // STATUS INTERNAL — Tidak perlu disentuh dari Inspector
    // =========================================================================
    private Vector3 posisiTujuan;        // Ke mana kamera mau pergi
    private bool sedangTransisi = false; // Apakah kamera sedang bergerak?

    // Properti publik supaya script lain bisa cek apakah kamera masih gerak
    // Berguna untuk mencegah interaksi saat transisi
    public bool SedangTransisi
    {
        get { return sedangTransisi; }
    }

    // =========================================================================
    // START — Saat game dimulai, set tujuan ke posisi kamera saat ini
    // Ini mencegah kamera langsung loncat saat game baru mulai
    // =========================================================================
    void Start()
    {
        posisiTujuan = transform.position;
    }

    // =========================================================================
    // UPDATE — Setiap frame, gerakkan kamera menuju tujuan secara smooth
    // Menggunakan Vector3.Lerp yang menghasilkan gerakan melambat di akhir
    // (ease-out), terasa natural dan tidak kaku
    // =========================================================================
    void Update()
    {
        // Cuma gerak kalau sedang dalam transisi
        if (sedangTransisi)
        {
            // Lerp = Linear Interpolation
            // Kamera bergerak dari posisi sekarang menuju tujuan
            // Makin dekat ke tujuan, makin pelan (efek ease-out)
            transform.position = Vector3.Lerp(
                transform.position,     // Dari posisi sekarang
                posisiTujuan,           // Menuju tujuan
                KecepatanTransisi * Time.deltaTime  // Kecepatan x waktu
            );

            // Cek apakah sudah cukup dekat dengan tujuan
            float jarakSisa = Vector3.Distance(transform.position, posisiTujuan);

            if (jarakSisa < JarakMinimumSampai)
            {
                // Snap ke posisi tujuan yang persis (biar tidak ada sisa jarak)
                transform.position = posisiTujuan;

                // Tandai transisi selesai
                sedangTransisi = false;

                Debug.Log("Kamera sampai di tujuan!");
            }
        }
    }

    // =========================================================================
    // PindahKeDapur — Dipanggil saat pemain mau ke area masak
    // Bisa dipanggil dari: PindahKamerakemasak, atau script lain
    // =========================================================================
    public void PindahKeDapur()
    {
        posisiTujuan = PosisiDapur;
        sedangTransisi = true;

        // Update status global di GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.SedangDiDapur = true;
        }

        Debug.Log("Kamera mulai pindah ke DAPUR...");
    }

    // =========================================================================
    // PindahKeDepan — Dipanggil saat pemain mau ke area pelanggan
    // Bisa dipanggil dari: PindahKamerakegerobak, TombolAntarPesanan, dll
    // =========================================================================
    public void PindahKeDepan()
    {
        posisiTujuan = PosisiDepan;
        sedangTransisi = true;

        // Update status global di GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.SedangDiDapur = false;
        }

        Debug.Log("Kamera mulai pindah ke DEPAN (pelanggan)...");
    }
}
