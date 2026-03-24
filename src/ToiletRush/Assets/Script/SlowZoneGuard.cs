using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NPCSlowZone : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float slowAmount = 0.4f;

    [Header("Zone Music")]
    public AudioClip zoneMusic;

    [Header("Zone UI")]
    public Image zoneImage;          // ลาก Image จาก Canvas มาใส่
    public Sprite zoneSprite;        // รูปที่จะโชว์
    public float fadeSpeed = 3f;

    [Header("Zone Particles")]
    public ParticleSystem sparklePrefab; // พาติคอลดาวประกาย
    public int sparkleCount = 10;         // จำนวนพาติคอลต่อ spawn
    public float sparkleSpawnRadius = 2f; // รัศมีรอบ trigger
    public float sparkleSpeedMin = 0.5f;  // ความเร็วต่ำสุด
    public float sparkleSpeedMax = 1.5f;  // ความเร็วสูงสุด
    public float sparkleLifetime = 1.5f;  // อายุของพาติคอล

    private Coroutine fadeRoutine;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();

        if (player != null)
        {
            player.SetSlowZone(true);

            // เข้าโซน
            ZoneAudioController.Instance.EnterZone(zoneMusic);

            // แสดง UI
            ShowZoneUI();

            // Spawn sparkle particles รอบผู้เล่น
            SpawnSparkleParticles(player.transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // ให้ spawn พาติคอลตลอดเวลาเมื่อเดินอยู่ในโซน
        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();
        if (player != null)
        {
            SpawnSparkleParticles(player.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();

        if (player != null)
        {
            player.SetSlowZone(false);

            // ออกจากโซน
            ZoneAudioController.Instance.ExitZone();

            // ซ่อน UI
            HideZoneUI();
        }
    }

    // ---------- UI ----------
    void ShowZoneUI()
    {
        if (zoneImage == null) return;

        zoneImage.sprite = zoneSprite;
        zoneImage.gameObject.SetActive(true);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeImage(1f));
    }

    void HideZoneUI()
    {
        if (zoneImage == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeImage(0f));
    }

    IEnumerator FadeImage(float targetAlpha)
    {
        Color c = zoneImage.color;

        while (!Mathf.Approximately(c.a, targetAlpha))
        {
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            zoneImage.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        zoneImage.color = c;

        if (targetAlpha == 0f)
            zoneImage.gameObject.SetActive(false);
    }

    // ---------- Particles ----------
    void SpawnSparkleParticles(Vector3 playerPosition)
    {
        if (sparklePrefab == null) return;

        for (int i = 0; i < sparkleCount; i++)
        {
            // สุ่มตำแหน่งรอบ player
            Vector3 randomOffset = Random.insideUnitSphere * sparkleSpawnRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y); // ให้ลอยขึ้น

            Vector3 spawnPos = playerPosition + randomOffset;

            // Instantiate Particle
            ParticleSystem sparkle = Instantiate(sparklePrefab, spawnPos, Quaternion.identity);

            // ปรับค่า velocity ให้มันลอยขึ้น/ออกแบบสุ่ม
            var main = sparkle.main;
            main.startSpeed = Random.Range(sparkleSpeedMin, sparkleSpeedMax);
            main.startLifetime = sparkleLifetime;
            main.simulationSpace = ParticleSystemSimulationSpace.World; // ลอยไปตามโลก

            // ปรับ shape ให้กระจายรอบ spawn
            var shape = sparkle.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            // ปรับ random velocity over lifetime เล็กน้อย
            var vel = sparkle.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            vel.y = new ParticleSystem.MinMaxCurve(0f, 0.3f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);

            // ทำให้พาติคอลหายเอง
            Destroy(sparkle.gameObject, sparkleLifetime);
        }
    }
}