using UnityEngine;

// Script ini berfungsi untuk memainkan efek suara setiap kali pemain menekan tombol klik kiri mouse di mana saja.
// Script ini sangat ramah pemula dan bisa dipasang di GameObject apa saja (misalnya Main Camera atau GameObject kosong bernama "AudioManager").
public class GlobalClickSound : MonoBehaviour
{
    [Header("Pengaturan Suara")]
    [Tooltip("Masukkan file suara (AudioClip) klik di sini melalui Inspector")]
    // Variabel ini digunakan untuk menyimpan efek suara (AudioClip) yang akan dimainkan.
    // [SerializeField] memungkinkan kita untuk mengisi suara langsung dari jendela Inspector Unity
    // tanpa harus membuat variabelnya menjadi public.
    [SerializeField] private AudioClip clickSound;

    [Tooltip("Volume suara klik (0.0 sampai 1.0)")]
    // Variabel untuk mengatur seberapa keras suara kliknya.
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    // AudioSource adalah komponen wajib di Unity yang bertugas mengeluarkan suara.
    // Kita akan membuatnya secara otomatis melalui script agar lebih praktis.
    private AudioSource audioSource;

    void Start()
    {
        // Saat game dimulai, kita menambahkan komponen AudioSource secara otomatis ke dalam GameObject ini.
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Kita mematikan 'Play On Awake' agar suara tidak langsung berbunyi saat game baru mulai.
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // Mengecek apakah tombol kiri mouse (0 adalah kode untuk klik kiri) baru saja ditekan pada frame (saat) ini.
        if (Input.GetMouseButtonDown(0))
        {
            // Memastikan file suara sudah dimasukkan di Inspector sebelum mencoba memainkannya.
            // Ini penting untuk mencegah error "NullReferenceException" jika kita lupa memasukkan file suaranya.
            if (clickSound != null)
            {
                // Memainkan efek suara satu kali. 
                // Kita menggunakan PlayOneShot() dan bukan Play() biasa agar suaranya bisa bertumpuk dengan baik
                // jika pemain menekan klik dengan sangat cepat secara beruntun.
                audioSource.PlayOneShot(clickSound, volume);
            }
            else
            {
                // Menampilkan peringatan di console Unity jika suara belum dimasukkan.
                Debug.LogWarning("Perhatian: Suara klik belum dimasukkan di Inspector pada script GlobalClickSound!");
            }
        }
    }
}
