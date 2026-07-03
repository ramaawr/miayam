using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Class data untuk disetel di Inspector Unity
[System.Serializable]
public class DataBukuHarian
{
    [Tooltip("ID Level atau Hari Ke- (misal: 1)")]
    public int LevelID;
    
    [Tooltip("Judul catatan buku harian")]
    public string JudulCatatan;
    
    [Tooltip("Isi cerita atau resume hari tersebut")]
    // TextArea memudahkan developer/pemula untuk mengetik cerita panjang di Inspector
    [TextArea(5, 10)]
    public string IsiCatatan;
}

public class DiaryManager : MonoBehaviour
{
    // Singleton agar LevelManager bisa memanggil fungsi di script ini dengan mudah
    public static DiaryManager instance;

    [Header("Pengaturan Data")]
    [Tooltip("Isi dengan cerita untuk tiap-tiap level secara berurutan")]
    public DataBukuHarian[] DaftarBukuHarian;
    
    [Header("Referensi UI (Desain Minimal Luxury)")]
    [Tooltip("Wadah utama (Panel) buku harian. Pastikan background Putih Bersih.")]
    public GameObject PanelBukuHarian;
    
    [Tooltip("Teks judul halaman. Gunakan warna Biru Tegas.")]
    public TextMeshProUGUI TeksJudul;
    
    [Tooltip("Teks isi cerita.")]
    public TextMeshProUGUI TeksIsiCatatan;
    
    [Tooltip("Wujud 2D/GameObject tombol halaman sebelumnya")]
    public GameObject TombolSebelumnya;
    
    [Tooltip("Wujud 2D/GameObject tombol halaman berikutnya")]
    public GameObject TombolBerikutnya;
    
    [Tooltip("Wujud 2D/GameObject tombol untuk menutup dan lanjut main")]
    public GameObject TombolTutup;
    
    // Variabel pengingat (memory) posisi halaman pemain saat ini
    private int halamanSekarang = 0;
    
    // Variabel pembatas agar pemain tidak bisa membaca halaman masa depan (level yang belum dimainkan)
    private int batasHalamanMaksimal = 0;

    // Properti publik (Tameng Pelindung) agar script lain tahu kalau buku harian sedang terbuka
    public bool IsBukuHarianTerbuka
    {
        get { return PanelBukuHarian != null && PanelBukuHarian.activeSelf; }
    }

    void Awake()
    {
        // Memastikan hanya ada satu DiaryManager
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    void Start()
    {
        // Pastikan UI disembunyikan saat awal game agar tidak menutupi layar
        if (PanelBukuHarian != null)
            PanelBukuHarian.SetActive(false);
    }
    
    void Update()
    {
        // Deteksi apabila pemain mengeklik layar (Mouse Kiri / Tap Layar)
        if (Input.GetMouseButtonDown(0))
        {
            // Ubah posisi klik di layar menjadi posisi di dunia game 2D
            Vector2 posisiMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Tembakkan garis lurus (Raycast) ke arah klik untuk melihat benda 2D apa yang kena
            RaycastHit2D hit = Physics2D.Raycast(posisiMouse, Vector2.zero);
            
            // Jika ada objek ber-Collider 2D yang tertembak/diklik
            if (hit.collider != null)
            {
                GameObject objekYangDiklik = hit.collider.gameObject;
                
                // Periksa apakah objek itu adalah tombol buku harian kita (dan tombolnya sedang aktif/muncul)
                if (TombolSebelumnya != null && objekYangDiklik == TombolSebelumnya && TombolSebelumnya.activeSelf)
                {
                    HalamanSebelumnya();
                }
                else if (TombolBerikutnya != null && objekYangDiklik == TombolBerikutnya && TombolBerikutnya.activeSelf)
                {
                    HalamanBerikutnya();
                }
                else if (TombolTutup != null && objekYangDiklik == TombolTutup && TombolTutup.activeSelf)
                {
                    TutupBukuHarian();
                }
            }
        }
    }
    
    // Fungsi ini dipanggil khusus oleh LevelManager setelah pemain menekan tombol Lanjut di Popup Result
    public void TampilkanBukuHarian(int levelYangBaruSelesai)
    {
        // KUNCI LOGIKA: Batas maksimal halaman yang diizinkan untuk dibuka adalah level yang baru saja selesai.
        // Pemain level 1 hanya bisa buka halaman index 0 (karena array mulai dari 0).
        batasHalamanMaksimal = levelYangBaruSelesai;
        
        // Langsung buka halaman cerita dari level yang barusan diselesaikan
        halamanSekarang = levelYangBaruSelesai;
        
        // Tampilkan panel UI Buku Harian
        if (PanelBukuHarian != null)
            PanelBukuHarian.SetActive(true);
            
        // Perbarui teks dan tombol
        PerbaruiTampilanHalaman();
    }
    
    // Fungsi untuk memperbarui teks di layar sesuai data halamanSekarang
    private void PerbaruiTampilanHalaman()
    {
        // Cek keamanan: apakah halaman yang mau dibuka ada di dalam daftar yang dibikin developer?
        if (halamanSekarang >= 0 && halamanSekarang < DaftarBukuHarian.Length)
        {
            DataBukuHarian data = DaftarBukuHarian[halamanSekarang];
            
            // Ganti teks judul dan isi sesuai data
            if (TeksJudul != null) TeksJudul.text = data.JudulCatatan;
            if (TeksIsiCatatan != null) TeksIsiCatatan.text = data.IsiCatatan;
        }
        else
        {
            // Jika dev lupa/belum mengisi cerita di Inspector, tampilkan teks darurat agar game tidak rusak
            if (TeksJudul != null) TeksJudul.text = "Hari Ke-" + (halamanSekarang + 1);
            if (TeksIsiCatatan != null) TeksIsiCatatan.text = "Catatan untuk hari ini belum ditulis.";
        }
        
        // ATURAN TOMBOL SEBELUMNYA:
        // Tombol hanya dimunculkan kalau pemain tidak berada di halaman paling pertama (index 0)
        if (TombolSebelumnya != null)
        {
            TombolSebelumnya.SetActive(halamanSekarang > 0);
        }
        
        // ATURAN TOMBOL BERIKUTNYA:
        // Tombol hanya dimunculkan kalau pemain belum mencapai 'batasHalamanMaksimal'
        if (TombolBerikutnya != null)
        {
            TombolBerikutnya.SetActive(halamanSekarang < batasHalamanMaksimal && halamanSekarang < DaftarBukuHarian.Length - 1);
        }
    }
    
    // Dipanggil saat pemain pencet tombol Back/Sebelumnya
    public void HalamanSebelumnya()
    {
        if (halamanSekarang > 0)
        {
            halamanSekarang--;
            PerbaruiTampilanHalaman();
        }
    }
    
    // Dipanggil saat pemain pencet tombol Next/Berikutnya
    public void HalamanBerikutnya()
    {
        if (halamanSekarang < batasHalamanMaksimal)
        {
            halamanSekarang++;
            PerbaruiTampilanHalaman();
        }
    }
    
    // Dipanggil saat pemain selesai baca dan mau lanjut main
    public void TutupBukuHarian()
    {
        // 1. Sembunyikan panel buku harian
        if (PanelBukuHarian != null)
            PanelBukuHarian.SetActive(false);
            
        // 2. Panggil fungsi di LevelManager untuk melakukan Fade Out dan berpindah ke level/hari selanjutnya
        if (LevelManager.instance != null)
        {
            LevelManager.instance.LanjutKeLevelBerikutnya();
        }
    }
}
