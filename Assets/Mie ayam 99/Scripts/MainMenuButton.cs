using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// =============================================================================
// MainMenuButton.cs — Script untuk efek hover, klik, dan highlight pada tombol
// =============================================================================
// Script ini ditempelkan pada GameObject tombol UI yang memiliki EventSystem.
// Script ini memberikan efek membesar perlahan, berganti warna, dan
// menampilkan bar highlight mirip menu Dragon Age / The Walking Dead.
// =============================================================================
public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Komponen Visual")]
    [Tooltip("Komponen teks TextMeshPro yang akan diubah warnanya dan ukurannya")]
    public TextMeshProUGUI teksTombol;
    
    [Tooltip("Garis sorot / background highlight yang muncul saat tombol disorot")]
    public RectTransform barHighlight;

    [Header("Pengaturan Warna & Skala")]
    [Tooltip("Warna teks saat keadaan normal (biasanya putih bersih)")]
    public Color warnaNormal = Color.white;
    
    [Tooltip("Warna teks saat disorot mouse (aksen biru sesuai AGENTS.md)")]
    public Color warnaHover = new Color(0.1f, 0.6f, 1.0f, 1.0f); // Biru terang

    [Tooltip("Skala ukuran tombol saat keadaan normal")]
    public Vector3 skalaNormal = Vector3.one;

    [Tooltip("Skala ukuran tombol saat disorot mouse (membesar sedikit)")]
    public Vector3 skalaHover = new Vector3(1.08f, 1.08f, 1.08f);

    [Tooltip("Skala ukuran tombol saat ditekan/diklik (mengecil sedikit)")]
    public Vector3 skalaTekan = new Vector3(0.95f, 0.95f, 0.95f);

    [Header("Pengaturan Warna Background")]
    [Tooltip("Warna background saat keadaan normal (hitam transparan tipis)")]
    public Color warnaBgNormal = new Color(0.0f, 0.0f, 0.0f, 0.3f);

    [Tooltip("Warna background saat disorot mouse (lebih gelap)")]
    public Color warnaBgHover = new Color(0.0f, 0.0f, 0.0f, 0.6f);

    [Tooltip("Warna background saat ditekan/diklik (sangat gelap)")]
    public Color warnaBgTekan = new Color(0.0f, 0.0f, 0.0f, 0.85f);

    [Header("Pengaturan Efek Transisi")]
    [Tooltip("Kecepatan transisi perubahan warna dan skala (semakin besar semakin cepat)")]
    public float kecepatanTransisi = 10f;

    [Tooltip("Apakah bar highlight harus bergeser sedikit saat muncul? (Slide-in effect)")]
    public bool gunakanEfekGeser = true;

    [Tooltip("Jarak pergeseran horizontal awal bar highlight (dalam pixel)")]
    public float jarakGeserHighlight = -15f;

    [Header("Pengaturan Suara (Opsional)")]
    [Tooltip("Sumber suara untuk memutar efek suara tombol")]
    public AudioSource audioSource;
    
    [Tooltip("Efek suara saat tombol disorot mouse")]
    public AudioClip suaraHover;
    
    [Tooltip("Efek suara saat tombol diklik")]
    public AudioClip suaraKlik;

    // Variabel internal untuk melacak status tombol
    private bool isHovered = false;
    private bool isPressed = false;
    
    // Target warna, skala, dan posisi highlight saat ini
    private Color targetWarna;
    private Vector3 targetSkala;
    private Vector3 posisiHighlightNormal;
    private Vector3 posisiHighlightGeser;
    private float targetHighlightAlpha;

    private CanvasGroup highlightCanvasGroup;
    private UnityEngine.UI.Image highlightImage;
    private Color targetBgWarna;

    private void Start()
    {
        // Set nilai awal target sesuai kondisi normal
        targetWarna = warnaNormal;
        targetSkala = skalaNormal;

        if (teksTombol != null)
        {
            teksTombol.color = warnaNormal;
        }

        transform.localScale = skalaNormal;

        // Inisialisasi bar highlight jika ada
        if (barHighlight != null)
        {
            highlightImage = barHighlight.GetComponent<UnityEngine.UI.Image>();

            // Ambil atau tambahkan CanvasGroup untuk memudarkan (fade) highlight
            highlightCanvasGroup = barHighlight.GetComponent<CanvasGroup>();
            if (highlightCanvasGroup == null)
            {
                highlightCanvasGroup = barHighlight.gameObject.AddComponent<CanvasGroup>();
            }

            // Set posisi awal highlight
            posisiHighlightNormal = barHighlight.localPosition;
            posisiHighlightGeser = posisiHighlightNormal + new Vector3(jarakGeserHighlight, 0, 0);

            // Sembunyikan highlight di awal (Sekrung selalu tampil penuh)
            highlightCanvasGroup.alpha = 1f;
            targetHighlightAlpha = 1f;
            if (gunakanEfekGeser)
            {
                barHighlight.localPosition = posisiHighlightGeser;
            }

            // Set warna awal background
            targetBgWarna = warnaBgNormal;
            if (highlightImage != null)
            {
                highlightImage.color = warnaBgNormal;
            }
        }
    }

    private void Update()
    {
        // Transisi skala tombol secara halus
        transform.localScale = Vector3.Lerp(transform.localScale, targetSkala, Time.deltaTime * kecepatanTransisi);

        // Transisi warna teks secara halus
        if (teksTombol != null)
        {
            teksTombol.color = Color.Lerp(teksTombol.color, targetWarna, Time.deltaTime * kecepatanTransisi);
        }

        // Transisi warna dan posisi highlight secara halus
        if (barHighlight != null && highlightCanvasGroup != null)
        {
            // CanvasGroup alpha selalu 1 karena kita ingin background selalu terlihat
            highlightCanvasGroup.alpha = 1f;

            // Transisi warna background secara halus
            if (highlightImage != null)
            {
                highlightImage.color = Color.Lerp(highlightImage.color, targetBgWarna, Time.deltaTime * kecepatanTransisi);
            }

            // Efek geser (slide-in)
            if (gunakanEfekGeser)
            {
                Vector3 targetPosisi = isHovered ? posisiHighlightNormal : posisiHighlightGeser;
                barHighlight.localPosition = Vector3.Lerp(barHighlight.localPosition, targetPosisi, Time.deltaTime * kecepatanTransisi);
            }
        }
    }

    // =========================================================================
    // Pointer Events — Diaktifkan otomatis oleh EventSystem Unity
    // =========================================================================

    // Saat kursor mouse masuk ke area tombol (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetWarna = warnaHover;
        targetSkala = skalaHover;
        targetBgWarna = warnaBgHover;

        // Putar suara hover jika ada
        if (audioSource != null && suaraHover != null)
        {
            audioSource.PlayOneShot(suaraHover);
        }
    }

    // Saat kursor mouse keluar dari area tombol
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        targetWarna = warnaNormal;
        targetSkala = skalaNormal;
        targetBgWarna = warnaBgNormal;
    }

    // Saat tombol diklik / ditekan (Click Down)
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetSkala = skalaTekan;
        targetBgWarna = warnaBgTekan;

        // Putar suara klik jika ada
        if (audioSource != null && suaraKlik != null)
        {
            audioSource.PlayOneShot(suaraKlik);
        }
    }

    // Saat tombol dilepas setelah diklik (Click Up)
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPressed)
        {
            isPressed = false;
            targetSkala = isHovered ? skalaHover : skalaNormal;
            targetBgWarna = isHovered ? warnaBgHover : warnaBgNormal;
        }
    }
}
