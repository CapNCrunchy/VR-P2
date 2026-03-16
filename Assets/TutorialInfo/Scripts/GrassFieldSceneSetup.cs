using UnityEngine;

/// <summary>
/// Immersive Grass Field — relaxation scene with:
/// - Rolling hills terrain with lush grass + clover
/// - Animated butterflies with wing flapping
/// - Wildflowers scattered across the field
/// - Drifting clouds
/// - Warm golden-hour lighting + soft fog
/// - Floating pollen/dust particles
/// - A cozy shady tree to explore
/// </summary>
public class GrassFieldSceneSetup : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int terrainWidth = 500;
    public int terrainLength = 500;
    public int terrainHeight = 40;

    [Header("Grass Settings")]
    public int grassDensity = 12;

    [Header("Scene Objects")]
    public int butterflyCount = 12;
    public int flowerCount = 80;
    public int cloudCount = 10;

    private Terrain _terrain;

    void Start()
    {
        _terrain = CreateTerrain();
        CreateSun();
        CreateSkyAmbience();
        CreateAmbientFog();
        CreateWindZone();
        CreateFlowers();
        CreateButterflies();
        CreateClouds();
        CreateFloatingDust();
        CreateCentralTree();
    }

    // ─── TERRAIN ───────────────────────────────────────────────────────────

    Terrain CreateTerrain()
    {
        TerrainData data = new TerrainData();
        data.heightmapResolution = 513;
        data.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

        float[,] heights = new float[513, 513];
        for (int x = 0; x < 513; x++)
            for (int z = 0; z < 513; z++)
            {
                float nx = x / 513f, nz = z / 513f;
                heights[x, z] = Mathf.PerlinNoise(nx * 2.5f + 10f, nz * 2.5f + 10f) * 0.06f
                               + Mathf.PerlinNoise(nx * 6f, nz * 6f) * 0.02f
                               + Mathf.PerlinNoise(nx * 14f, nz * 14f) * 0.005f;
            }
        data.SetHeights(0, 0, heights);

        TerrainLayer grassLayer = new TerrainLayer();
        grassLayer.diffuseTexture = MakeTex(new Color(0.22f, 0.42f, 0.12f), new Color(0.30f, 0.52f, 0.16f));
        grassLayer.tileSize = new Vector2(8f, 8f);
        data.terrainLayers = new TerrainLayer[] { grassLayer };

        DetailPrototype blade = new DetailPrototype();
        blade.prototypeTexture = MakeGrassBladeTex();
        blade.renderMode = DetailRenderMode.GrassBillboard;
        blade.healthyColor = new Color(0.28f, 0.62f, 0.14f);
        blade.dryColor     = new Color(0.55f, 0.58f, 0.08f);
        blade.minWidth = 0.7f; blade.maxWidth = 1.3f;
        blade.minHeight = 0.55f; blade.maxHeight = 1.1f;
        blade.noiseSpread = 0.5f;

        DetailPrototype clover = new DetailPrototype();
        clover.prototypeTexture = MakeCloverTex();
        clover.renderMode = DetailRenderMode.GrassBillboard;
        clover.healthyColor = new Color(0.2f, 0.55f, 0.18f);
        clover.dryColor     = new Color(0.38f, 0.5f, 0.1f);
        clover.minWidth = 0.5f; clover.maxWidth = 0.9f;
        clover.minHeight = 0.2f; clover.maxHeight = 0.45f;
        clover.noiseSpread = 0.7f;

        data.detailPrototypes = new DetailPrototype[] { blade, clover };
        data.SetDetailResolution(1024, 16);

        int[,] bladeMap  = new int[1024, 1024];
        int[,] cloverMap = new int[1024, 1024];
        for (int x = 0; x < 1024; x++)
            for (int z = 0; z < 1024; z++)
            {
                bladeMap[x, z]  = grassDensity;
                cloverMap[x, z] = Random.value > 0.6f ? Random.Range(2, 6) : 0;
            }
        data.SetDetailLayer(0, 0, 0, bladeMap);
        data.SetDetailLayer(0, 0, 1, cloverMap);

        GameObject terrainObj = Terrain.CreateTerrainGameObject(data);
        terrainObj.name = "GrassField";
        Terrain t = terrainObj.GetComponent<Terrain>();
        t.detailObjectDistance = 200f;
        t.basemapDistance = 500f;
        t.drawInstanced = true;
        terrainObj.transform.position = new Vector3(-terrainWidth / 2f, 0f, -terrainLength / 2f);
        return t;
    }

    // ─── LIGHTING ──────────────────────────────────────────────────────────

    void CreateSun()
    {
        GameObject sunObj = new GameObject("Sun");
        Light sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.93f, 0.72f);
        sun.intensity = 1.25f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.6f;
        sunObj.transform.rotation = Quaternion.Euler(38f, -55f, 0f);
    }

    void CreateSkyAmbience()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.5f, 0.72f, 1.0f);
        RenderSettings.ambientEquatorColor = new Color(0.48f, 0.62f, 0.35f);
        RenderSettings.ambientGroundColor  = new Color(0.18f, 0.25f, 0.10f);
    }

    void CreateAmbientFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.68f, 0.82f, 0.92f);
        RenderSettings.fogDensity = 0.004f;
    }

    void CreateWindZone()
    {
        GameObject w = new GameObject("WindZone");
        WindZone wz = w.AddComponent<WindZone>();
        wz.mode = WindZoneMode.Directional;
        wz.windMain = 0.35f;
        wz.windTurbulence = 0.25f;
        wz.windPulseMagnitude = 0.6f;
        wz.windPulseFrequency = 0.045f;
        w.transform.rotation = Quaternion.Euler(0f, 40f, 0f);
    }

    // ─── FLOWERS ───────────────────────────────────────────────────────────

    void CreateFlowers()
    {
        Color[] cols = {
            new Color(1f, 0.9f, 0.1f),
            new Color(1f, 0.4f, 0.7f),
            new Color(0.6f, 0.3f, 1f),
            new Color(1f, 0.55f, 0.1f),
            new Color(1f, 1f, 1f),
        };
        GameObject parent = new GameObject("Flowers");
        for (int i = 0; i < flowerCount; i++)
        {
            float rx = Random.Range(-terrainWidth * 0.45f, terrainWidth * 0.45f);
            float rz = Random.Range(-terrainLength * 0.45f, terrainLength * 0.45f);
            float ry = SampleTerrainHeight(rx, rz);
            GameObject f = MakeFlower(cols[Random.Range(0, cols.Length)]);
            f.transform.SetParent(parent.transform);
            f.transform.position = new Vector3(rx, ry, rz);
            f.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float s = Random.Range(0.25f, 0.5f);
            f.transform.localScale = Vector3.one * s;
            f.AddComponent<GentleSway>();
        }
    }

    GameObject MakeFlower(Color col)
    {
        GameObject root = new GameObject("Flower");
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.transform.SetParent(root.transform);
        stem.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        stem.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
        stem.GetComponent<Renderer>().material.color = new Color(0.2f, 0.5f, 0.1f);
        Destroy(stem.GetComponent<Collider>());

        int petalCount = 6;
        for (int p = 0; p < petalCount; p++)
        {
            float angle = p * (360f / petalCount) * Mathf.Deg2Rad;
            GameObject petal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            petal.transform.SetParent(root.transform);
            petal.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.35f, 0.82f, Mathf.Sin(angle) * 0.35f);
            petal.transform.localScale = new Vector3(0.3f, 0.08f, 0.3f);
            petal.GetComponent<Renderer>().material.color = col;
            Destroy(petal.GetComponent<Collider>());
        }

        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        center.transform.SetParent(root.transform);
        center.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        center.transform.localScale = Vector3.one * 0.28f;
        center.GetComponent<Renderer>().material.color = new Color(1f, 0.85f, 0.1f);
        Destroy(center.GetComponent<Collider>());
        return root;
    }

    // ─── BUTTERFLIES ───────────────────────────────────────────────────────

    void CreateButterflies()
    {
        Color[] cols = {
            new Color(0.9f, 0.6f, 0.1f),
            new Color(0.5f, 0.2f, 0.8f),
            new Color(0.1f, 0.6f, 0.9f),
            new Color(1f, 0.3f, 0.5f),
            new Color(0.9f, 0.9f, 0.3f),
        };
        GameObject parent = new GameObject("Butterflies");
        for (int i = 0; i < butterflyCount; i++)
        {
            float rx = Random.Range(-80f, 80f);
            float rz = Random.Range(-80f, 80f);
            float ry = SampleTerrainHeight(rx, rz) + Random.Range(1f, 3.5f);
            GameObject bf = MakeButterfly(cols[Random.Range(0, cols.Length)]);
            bf.transform.SetParent(parent.transform);
            bf.transform.position = new Vector3(rx, ry, rz);
            ButterflyFlight flight = bf.AddComponent<ButterflyFlight>();
            flight.baseY = ry;
        }
    }

    GameObject MakeButterfly(Color col)
    {
        GameObject root = new GameObject("Butterfly");

        GameObject lWing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lWing.transform.SetParent(root.transform);
        lWing.transform.localPosition = new Vector3(-0.18f, 0f, 0f);
        lWing.transform.localScale = new Vector3(0.35f, 0.04f, 0.25f);
        lWing.GetComponent<Renderer>().material.color = col;
        Destroy(lWing.GetComponent<Collider>());

        GameObject rWing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rWing.transform.SetParent(root.transform);
        rWing.transform.localPosition = new Vector3(0.18f, 0f, 0f);
        rWing.transform.localScale = new Vector3(0.35f, 0.04f, 0.25f);
        rWing.GetComponent<Renderer>().material.color = col;
        Destroy(rWing.GetComponent<Collider>());

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(root.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        body.GetComponent<Renderer>().material.color = Color.black;
        Destroy(body.GetComponent<Collider>());
        return root;
    }

    // ─── CLOUDS ────────────────────────────────────────────────────────────

    void CreateClouds()
    {
        GameObject parent = new GameObject("Clouds");
        for (int i = 0; i < cloudCount; i++)
        {
            GameObject cloud = MakeCloud();
            cloud.transform.SetParent(parent.transform);
            cloud.transform.position = new Vector3(
                Random.Range(-200f, 200f),
                Random.Range(60f, 110f),
                Random.Range(-200f, 200f)
            );
            float s = Random.Range(8f, 20f);
            cloud.transform.localScale = Vector3.one * s;
            CloudDrift drift = cloud.AddComponent<CloudDrift>();
            drift.speed = Random.Range(0.5f, 1.5f);
        }
    }

    GameObject MakeCloud()
    {
        GameObject root = new GameObject("Cloud");
        Color col = new Color(1f, 1f, 1f, 0.9f);
        int blobs = Random.Range(4, 7);
        for (int b = 0; b < blobs; b++)
        {
            GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blob.transform.SetParent(root.transform);
            blob.transform.localPosition = new Vector3(
                Random.Range(-0.6f, 0.6f),
                Random.Range(-0.15f, 0.2f),
                Random.Range(-0.2f, 0.2f)
            );
            float bs = Random.Range(0.4f, 0.9f);
            blob.transform.localScale = new Vector3(bs * 1.4f, bs * 0.7f, bs);
            blob.GetComponent<Renderer>().material.color = col;
            Destroy(blob.GetComponent<Collider>());
        }
        return root;
    }

    // ─── FLOATING POLLEN ───────────────────────────────────────────────────

    void CreateFloatingDust()
    {
        GameObject dustObj = new GameObject("FloatingPollen");
        ParticleSystem ps = dustObj.AddComponent<ParticleSystem>();
        dustObj.transform.position = Vector3.zero;

        var main = ps.main;
        main.maxParticles = 200;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 14f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.01f, 0.05f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.98f, 0.8f, 0.3f),
            new Color(0.9f, 1f, 0.7f, 0.5f)
        );
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(120f, 8f, 120f);
        shape.position = new Vector3(0f, 2f, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.2f);
        vel.y = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        vel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
    }

    // ─── COZY TREE ─────────────────────────────────────────────────────────

    void CreateCentralTree()
    {
        GameObject treeRoot = new GameObject("CozyTree");
        treeRoot.transform.position = new Vector3(15f, SampleTerrainHeight(15f, 20f), 20f);

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(treeRoot.transform);
        trunk.transform.localPosition = new Vector3(0f, 3f, 0f);
        trunk.transform.localScale = new Vector3(0.7f, 3f, 0.7f);
        trunk.GetComponent<Renderer>().material.color = new Color(0.38f, 0.24f, 0.12f);

        AddLeafBlob(treeRoot, new Vector3(0f, 7.5f, 0f),    new Vector3(5f, 3f, 5f),     new Color(0.18f, 0.50f, 0.13f));
        AddLeafBlob(treeRoot, new Vector3(1f, 9.5f, 0.5f),  new Vector3(3.5f, 2.5f, 3.5f), new Color(0.22f, 0.55f, 0.15f));
        AddLeafBlob(treeRoot, new Vector3(-0.5f, 11f, -0.5f), new Vector3(2.5f, 2f, 2.5f), new Color(0.25f, 0.58f, 0.17f));
    }

    void AddLeafBlob(GameObject parent, Vector3 localPos, Vector3 scale, Color col)
    {
        GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blob.transform.SetParent(parent.transform);
        blob.transform.localPosition = localPos;
        blob.transform.localScale = scale;
        blob.GetComponent<Renderer>().material.color = col;
    }

    // ─── HELPERS ───────────────────────────────────────────────────────────

    float SampleTerrainHeight(float worldX, float worldZ)
    {
        if (_terrain == null) return 0f;
        return _terrain.SampleHeight(new Vector3(worldX, 0f, worldZ));
    }

    Texture2D MakeTex(Color a, Color b)
    {
        int size = 128;
        Texture2D t = new Texture2D(size, size);
        Color[] px = new Color[size * size];
        for (int i = 0; i < px.Length; i++)
            px[i] = Color.Lerp(a, b, Random.value * 0.5f + (i % size) / (float)size * 0.5f);
        t.SetPixels(px); t.Apply(); return t;
    }

    Texture2D MakeGrassBladeTex()
    {
        int w = 64, h = 128;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            float t = y / (float)h;
            float halfW = Mathf.Lerp(w * 0.42f, w * 0.05f, t);
            for (int x = 0; x < w; x++)
            {
                bool inBlade = Mathf.Abs(x - w * 0.5f) < halfW;
                px[y * w + x] = inBlade
                    ? new Color(Mathf.Lerp(0.45f, 0.8f, t) * 0.32f, Mathf.Lerp(0.45f, 0.8f, t), Mathf.Lerp(0.45f, 0.8f, t) * 0.08f, 1f)
                    : Color.clear;
            }
        }
        tex.SetPixels(px); tex.Apply(); return tex;
    }

    Texture2D MakeCloverTex()
    {
        int w = 64, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w * h];
        Vector2 center = new Vector2(w / 2f, h / 2f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool inLeaf = Vector2.Distance(new Vector2(x, y), center) < w * 0.42f;
                px[y * w + x] = inLeaf ? new Color(0.18f, 0.52f, 0.12f, 1f) : Color.clear;
            }
        tex.SetPixels(px); tex.Apply(); return tex;
    }
}

// ─── BEHAVIOUR SCRIPTS (all in one file for convenience) ──────────────────

public class GentleSway : MonoBehaviour
{
    float _speed, _amount, _offset;
    Quaternion _base;
    void Start()
    {
        _speed = Random.Range(0.6f, 1.4f);
        _amount = Random.Range(3f, 8f);
        _offset = Random.Range(0f, Mathf.PI * 2f);
        _base = transform.rotation;
    }
    void Update()
    {
        float s = Mathf.Sin(Time.time * _speed + _offset) * _amount;
        transform.rotation = _base * Quaternion.Euler(s, 0f, s * 0.4f);
    }
}

public class ButterflyFlight : MonoBehaviour
{
    public float baseY;
    float _speed, _radius, _offset, _flapSpeed;
    Transform _lWing, _rWing;
    void Start()
    {
        _speed = Random.Range(0.3f, 0.7f);
        _radius = Random.Range(4f, 12f);
        _offset = Random.Range(0f, Mathf.PI * 2f);
        _flapSpeed = Random.Range(3f, 6f);
        if (transform.childCount >= 2) { _lWing = transform.GetChild(0); _rWing = transform.GetChild(1); }
    }
    void Update()
    {
        float t = Time.time * _speed + _offset;
        transform.position = new Vector3(
            transform.parent.position.x + Mathf.Sin(t) * _radius,
            baseY + Mathf.Sin(t * 1.3f) * 0.6f,
            transform.parent.position.z + Mathf.Sin(t * 0.7f) * _radius
        );
        Vector3 dir = new Vector3(Mathf.Cos(t), 0f, Mathf.Cos(t * 0.7f) * 0.7f);
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        if (_lWing && _rWing)
        {
            float flap = Mathf.Sin(Time.time * _flapSpeed) * 45f;
            _lWing.localRotation = Quaternion.Euler(0f, -flap, 0f);
            _rWing.localRotation = Quaternion.Euler(0f,  flap, 0f);
        }
    }
}

public class CloudDrift : MonoBehaviour
{
    public float speed = 1f;
    Vector3 _dir;
    void Start() => _dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-0.3f, 0.3f)).normalized;
    void Update()
    {
        transform.position += _dir * speed * Time.deltaTime;
        if (Mathf.Abs(transform.position.x) > 250f)
        {
            Vector3 p = transform.position;
            p.x = -Mathf.Sign(p.x) * 225f;
            transform.position = p;
        }
    }
}