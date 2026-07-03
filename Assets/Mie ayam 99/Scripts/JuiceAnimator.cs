using System.Collections;
using UnityEngine;

// Script pendamping ini berfungsi untuk memberikan mikro-animasi (Juice) pada objek UI atau elemen Game.
// Dapat dipasang pada GameObject yang ingin dianimasikan atau dipanggil oleh script utama secara publik.
public class JuiceAnimator : MonoBehaviour
{
    [Header("Pengaturan Animasi Memasak (Wobble)")]
    [SerializeField] private float wobbleSpeed = 5f;
    [SerializeField] private float wobbleAmount = 0.05f;

    [Header("Pengaturan Animasi Pop & UI")]
    [SerializeField] private float popDuration = 0.3f;
    // Default curve dengan efek overshoot/bounce: naik melebihi 1, lalu kembali ke 1.
    [SerializeField] private AnimationCurve popCurve = new AnimationCurve(
        new Keyframe(0f, 0f), 
        new Keyframe(0.7f, 1.15f), 
        new Keyframe(1f, 1f)
    );
    
    [Header("Pengaturan Portrait Breathing")]
    [SerializeField] private float breathingSpeed = 2f;
    [SerializeField] private float breathingAmount = 5f; // Gerakan naik-turun dalam piksel/unit

    [Header("Efek Partikel (Opsional)")]
    [Tooltip("Masukkan prefab partikel (seperti bintang/asap) yang akan dimainkan saat efek Pop (masakan matang)")]
    [SerializeField] private GameObject particlePrefab;

    private Coroutine currentCoroutine;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Awake()
    {
        // Simpan ukuran dan posisi asli objek saat pertama kali aktif
        originalScale = transform.localScale;
        // Jika UI, posisi biasanya bergantung pada RectTransform, tapi kita simpan localPosition untuk fleksibilitas
        originalPosition = transform.localPosition;
    }

    // 1. Animasi Wobble/Breathing saat proses memasak
    public void StartCookingWobble()
    {
        StopCurrentAnimation();
        currentCoroutine = StartCoroutine(WobbleRoutine());
    }

    private IEnumerator WobbleRoutine()
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * wobbleSpeed;
            // Menggunakan fungsi Sinus (Mathf.Sin) untuk gerakan membesar dan mengecil secara halus
            // Perhitungan: skala asli + (skala asli * nilai Sinus * seberapa besar wobble-nya)
            float scaleModifier = 1f + (Mathf.Sin(time) * wobbleAmount);
            transform.localScale = originalScale * scaleModifier;
            yield return null;
        }
    }

    // Fungsi utilitas untuk menghentikan animasi secara paksa dan mengembalikan posisi/skala
    public void StopAnimation()
    {
        StopCurrentAnimation();
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
    }

    // 2. Animasi Pop (saat masakan selesai / matang)
    public void PlayPopAnimation()
    {
        StopCurrentAnimation();
        // Membesar 20% secara cepat (1.2f) lalu kembali normal
        currentCoroutine = StartCoroutine(PopRoutine(originalScale * 1.2f, originalScale, 0.2f, false));
        
        // Memainkan efek partikel otomatis jika ada prefab yang dimasukkan
        if (particlePrefab != null)
        {
            Instantiate(particlePrefab, transform.position, Quaternion.identity);
        }
    }

    // 3. Animasi Dialog / UI Pop-in (dengan pantulan/bounce elegan)
    public void PlayPopInUI()
    {
        StopCurrentAnimation();
        // Memulai dari skala 0
        transform.localScale = Vector3.zero;
        // Menggunakan "true" untuk mengaktifkan AnimationCurve yang memiliki pantulan
        currentCoroutine = StartCoroutine(PopRoutine(Vector3.zero, originalScale, popDuration, true));
    }

    // 4. Animasi Order Bubble NPC (Membesar dari sumbu Y saja)
    public void PlayBubblePopUp()
    {
        StopCurrentAnimation();
        // Sumbu Y dimulai dari 0
        Vector3 startScale = new Vector3(originalScale.x, 0f, originalScale.z);
        transform.localScale = startScale;
        currentCoroutine = StartCoroutine(PopRoutine(startScale, originalScale, popDuration, true));
    }

    // Coroutine inti untuk mengurus pergerakan skala transisi
    private IEnumerator PopRoutine(Vector3 startScale, Vector3 targetScale, float duration, bool useCurve)
    {
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            // Menggunakan pecahan eksak (1f / duration) agar perkalian dilakukan, alih-alih pembagian pada setiap frame
            float t = timeElapsed * (1f / duration); 
            
            if (useCurve)
            {
                // Evaluasi nilai berdasarkan kurva animasi (untuk pantulan)
                t = popCurve.Evaluate(t);
            }

            // Gunakan LerpUnclamped agar kurva yang nilainya lebih dari 1 (overshoot) dapat diterapkan
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // Pastikan ukuran akhirnya tepat (tidak meleset karena frame)
        transform.localScale = targetScale;
    }

    // 5. Animasi Portrait Breathing (naik turun secara halus)
    public void StartPortraitBreathing()
    {
        StopCurrentAnimation();
        currentCoroutine = StartCoroutine(PortraitBreathingRoutine());
    }

    private IEnumerator PortraitBreathingRoutine()
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * breathingSpeed;
            // Gerakan naik turun sumbu Y menggunakan Mathf.Sin
            float yOffset = Mathf.Sin(time) * breathingAmount;
            transform.localPosition = originalPosition + new Vector3(0f, yOffset, 0f);
            yield return null;
        }
    }

    private void StopCurrentAnimation()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }
}
