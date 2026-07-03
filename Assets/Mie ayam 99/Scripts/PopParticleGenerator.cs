using UnityEngine;
using UnityEditor;

public class PopParticleGenerator
{
    [MenuItem("Tools/Mie Ayam 99/Buat Prefab Partikel Pop")]
    public static void CreatePopParticlePrefab()
    {
        // 1. Buat GameObject
        GameObject particleObj = new GameObject("PopParticleEffect");

        // 2. Tambah Particle System
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        
        // 3. Konfigurasi Main
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startColor = Color.white;
        main.playOnAwake = true;
        
        // Sangat Penting: Hancurkan otomatis setelah selesai agar tidak terjadi memory leak!
        main.stopAction = ParticleSystemStopAction.Destroy; 

        // 4. Konfigurasi Emission (Ledakan/Burst saat masak selesai)
        var emission = ps.emission;
        emission.rateOverTime = 0; 
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 15) }); 

        // 5. Konfigurasi Shape (Menyebar melingkar untuk 2D)
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        // 6. Konfigurasi Color over Lifetime (Efek memudar & berubah ke warna biru aksen)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(new Color(0.29f, 0.56f, 0.85f), 1.0f) // Warna biru mewah
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = grad;

        // 7. Render (Material & Layer)
        ParticleSystemRenderer renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        // Gunakan material partikel bawaan yang ringan (kotak/lingkaran dasar)
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.sortingOrder = 100; // Pastikan muncul di atas semua sprite 2D lainnya
        
        // 8. Simpan ke folder Assets
        string path = "Assets/Mie ayam 99/PopParticleEffect.prefab";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        PrefabUtility.SaveAsPrefabAsset(particleObj, path);
        
        // 9. Hapus dari Scene
        GameObject.DestroyImmediate(particleObj);

        Debug.Log("✅ Prefab Partikel 'PopParticleEffect' berhasil dibuat! Silakan cek di: " + path);
    }
}
