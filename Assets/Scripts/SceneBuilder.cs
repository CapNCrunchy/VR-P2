using UnityEngine;

/// <summary>
/// DROP THIS ONE SCRIPT onto any empty GameObject in your scene.
/// It builds the ENTIRE scene in code — terrain, grass, butterflies,
/// playground, trees, flowers, sky, lighting — no imports needed.
/// </summary>
public class SceneBuilder : MonoBehaviour
{
    void Start()
    {
        SetupLighting();
        BuildGround();
        BuildTrees(20);
        BuildFlowers(40);
        BuildPlayground();
        SpawnButterflies(14);
        BuildFence();
        PositionCamera();
        CreatePollenParticles();
        SpawnCollectibles(12);
        SetupCollectibleManager();
    }

    // ─────────────────────────────────────────────────────────
    // LIGHTING & SKY
    // ─────────────────────────────────────────────────────────
    void SetupLighting()
    {
        // Sky colour
        Camera.main.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
        Camera.main.clearFlags = CameraClearFlags.SolidColor;

        // Sun (directional light already in scene — just configure it)
        Light sun = FindFirstObjectByType<Light>();
        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sun.color   = new Color(1f, 0.95f, 0.8f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
        }

        // Ambient
        RenderSettings.ambientLight = new Color(0.5f, 0.6f, 0.4f);
        RenderSettings.fogColor     = new Color(0.7f, 0.85f, 0.7f);
        RenderSettings.fog          = true;
        RenderSettings.fogMode      = FogMode.Linear;
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance   = 120f;
    }

    // ─────────────────────────────────────────────────────────
    // GROUND PLANE
    // ─────────────────────────────────────────────────────────
    void BuildGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10f, 1f, 10f); // 100x100 units
        ground.transform.position   = Vector3.zero;
        ground.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.55f, 0.15f));

        // Add some low rolling bumps using smaller planes at slight angles
        for (int i = 0; i < 6; i++)
        {
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = "Hill_" + i;
            float x = Random.Range(-30f, 30f);
            float z = Random.Range(-30f, 30f);
            float s = Random.Range(8f, 18f);
            hill.transform.position   = new Vector3(x, -s * 0.85f, z);
            hill.transform.localScale = new Vector3(s, s, s);
            hill.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.55f, 0.15f));
            // Remove collider on hills so camera doesn't clip
            Destroy(hill.GetComponent<Collider>());
        }
    }

    // ─────────────────────────────────────────────────────────
    // TREES  (cone trunk + sphere canopy)
    // ─────────────────────────────────────────────────────────
    void BuildTrees(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle  = (i / (float)count) * Mathf.PI * 2f;
            float radius = Random.Range(18f, 38f);
            float x = Mathf.Cos(angle) * radius + Random.Range(-4f, 4f);
            float z = Mathf.Sin(angle) * radius + Random.Range(-4f, 4f);

            MakeTree(new Vector3(x, 0f, z));
        }
    }

    void MakeTree(Vector3 pos)
    {
        GameObject root = new GameObject("Tree");
        root.transform.position = pos;

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(root.transform);
        float trunkH = Random.Range(1.5f, 3f);
        trunk.transform.localPosition = new Vector3(0, trunkH / 2f, 0);
        trunk.transform.localScale    = new Vector3(0.25f, trunkH / 2f, 0.25f);
        trunk.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.25f, 0.1f));
        // keep trunk collider so mouse raycast can hit it

        // Canopy (layered spheres for a fluffy look)
        Color leafCol = new Color(
            Random.Range(0.1f, 0.25f),
            Random.Range(0.45f, 0.65f),
            Random.Range(0.05f, 0.2f));

        for (int layer = 0; layer < 3; layer++)
        {
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.transform.SetParent(root.transform);
            float s = Random.Range(1.8f, 2.8f) - layer * 0.3f;
            canopy.transform.localPosition = new Vector3(
                Random.Range(-0.2f, 0.2f),
                trunkH + layer * 1.0f,
                Random.Range(-0.2f, 0.2f));
            canopy.transform.localScale = new Vector3(s, s * 0.85f, s);
            canopy.GetComponent<Renderer>().material = MakeMat(leafCol);
            // keep collider on layer 0 (main canopy) for raycast hits; remove the rest
            if (layer > 0) Destroy(canopy.GetComponent<Collider>());
        }

        root.AddComponent<WindSway>();
        root.AddComponent<TreeShake>();
    }

    // ─────────────────────────────────────────────────────────
    // FLOWERS
    // ─────────────────────────────────────────────────────────
    void BuildFlowers(int count)
    {
        Color[] petalColors = {
            new Color(1f, 0.3f, 0.5f),   // pink
            new Color(1f, 0.9f, 0.2f),   // yellow
            new Color(0.9f, 0.4f, 1f),   // purple
            new Color(1f, 0.5f, 0.1f),   // orange
            new Color(1f, 1f, 1f),        // white
        };

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-35f, 35f);
            float z = Random.Range(-35f, 35f);
            MakeFlower(new Vector3(x, 0f, z), petalColors[Random.Range(0, petalColors.Length)]);
        }
    }

    void MakeFlower(Vector3 pos, Color petalCol)
    {
        GameObject root = new GameObject("Flower");
        root.transform.position = pos;

        // Stem
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.transform.SetParent(root.transform);
        float h = Random.Range(0.3f, 0.7f);
        stem.transform.localPosition = new Vector3(0, h / 2f, 0);
        stem.transform.localScale    = new Vector3(0.04f, h / 2f, 0.04f);
        stem.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.55f, 0.1f));
        Destroy(stem.GetComponent<Collider>());

        // Centre
        GameObject centre = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centre.transform.SetParent(root.transform);
        centre.transform.localPosition = new Vector3(0, h, 0);
        centre.transform.localScale    = new Vector3(0.12f, 0.12f, 0.12f);
        centre.GetComponent<Renderer>().material = MakeMat(new Color(1f, 0.85f, 0f));
        Destroy(centre.GetComponent<Collider>());

        // Petals
        int petalCount = Random.Range(5, 8);
        for (int p = 0; p < petalCount; p++)
        {
            GameObject petal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            petal.transform.SetParent(root.transform);
            float angle = (p / (float)petalCount) * 360f;
            float rad   = Mathf.Deg2Rad * angle;
            petal.transform.localPosition = new Vector3(
                Mathf.Cos(rad) * 0.13f, h, Mathf.Sin(rad) * 0.13f);
            petal.transform.localScale = new Vector3(0.1f, 0.04f, 0.14f);
            petal.transform.LookAt(root.transform.position + Vector3.up * h);
            petal.GetComponent<Renderer>().material = MakeMat(petalCol);
            Destroy(petal.GetComponent<Collider>());
        }

        // Add wind sway
        root.AddComponent<WindSway>();
    }

    // ─────────────────────────────────────────────────────────
    // PLAYGROUND
    // ─────────────────────────────────────────────────────────
    void BuildPlayground()
    {
        GameObject pg = new GameObject("Playground");
        pg.transform.position = new Vector3(12f, 0f, 5f);

        // Ground pad
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.transform.SetParent(pg.transform);
        pad.transform.localPosition = new Vector3(0, 0.02f, 0);
        pad.transform.localScale    = new Vector3(12f, 0.04f, 8f);
        pad.GetComponent<Renderer>().material = MakeMat(new Color(0.85f, 0.65f, 0.3f)); // sand
        Destroy(pad.GetComponent<Collider>());

        MakeSwingSet(pg.transform, new Vector3(-3f, 0f, 0f));
        MakeSlide(pg.transform,    new Vector3( 3f, 0f, 0f));
        MakeSeesaw(pg.transform,   new Vector3( 0f, 0f, 3f));
    }

    void MakeSwingSet(Transform parent, Vector3 localPos)
    {
        GameObject ss = new GameObject("SwingSet");
        ss.transform.SetParent(parent);
        ss.transform.localPosition = localPos;

        Color frameCol = MakeCol(0.7f, 0.3f, 0.1f); // orange-brown

        // Two A-frame legs (left)
        MakeBar(ss.transform, new Vector3(-1.5f, 1.5f, -0.8f), new Vector3(0.08f, 3f, 0.08f),
                Quaternion.Euler(15f, 0f, 0f), frameCol);
        MakeBar(ss.transform, new Vector3(-1.5f, 1.5f,  0.8f), new Vector3(0.08f, 3f, 0.08f),
                Quaternion.Euler(-15f, 0f, 0f), frameCol);
        // Two A-frame legs (right)
        MakeBar(ss.transform, new Vector3( 1.5f, 1.5f, -0.8f), new Vector3(0.08f, 3f, 0.08f),
                Quaternion.Euler(15f, 0f, 0f), frameCol);
        MakeBar(ss.transform, new Vector3( 1.5f, 1.5f,  0.8f), new Vector3(0.08f, 3f, 0.08f),
                Quaternion.Euler(-15f, 0f, 0f), frameCol);
        // Top bar
        MakeBar(ss.transform, new Vector3(0f, 3f, 0f), new Vector3(3.2f, 0.08f, 0.08f),
                Quaternion.identity, frameCol);

        // Two swings
        MakeSwing(ss.transform, new Vector3(-0.8f, 3f, 0f), 0f);
        MakeSwing(ss.transform, new Vector3( 0.8f, 3f, 0f), 1.5f);
    }

    void MakeSwing(Transform parent, Vector3 topPos, float timeOffset)
    {
        GameObject pivot = new GameObject("SwingPivot");
        pivot.transform.SetParent(parent);
        pivot.transform.localPosition = topPos;

        SwingAnimator anim = pivot.AddComponent<SwingAnimator>();
        anim.timeOffset = timeOffset;

        Color ropeCol = MakeCol(0.6f, 0.5f, 0.3f);

        // Ropes
        MakeBar(pivot.transform, new Vector3(-0.2f, -0.9f, 0f), new Vector3(0.03f, 1.8f, 0.03f),
                Quaternion.identity, ropeCol);
        MakeBar(pivot.transform, new Vector3( 0.2f, -0.9f, 0f), new Vector3(0.03f, 1.8f, 0.03f),
                Quaternion.identity, ropeCol);

        // Seat
        MakeBar(pivot.transform, new Vector3(0f, -1.8f, 0f), new Vector3(0.45f, 0.06f, 0.2f),
                Quaternion.identity, MakeCol(0.2f, 0.4f, 0.8f));
    }

    void MakeSlide(Transform parent, Vector3 localPos)
    {
        GameObject slide = new GameObject("Slide");
        slide.transform.SetParent(parent);
        slide.transform.localPosition = localPos;

        Color col = MakeCol(0.9f, 0.2f, 0.2f); // red

        // Platform
        MakeBar(slide.transform, new Vector3(0f, 2f, 0f), new Vector3(1f, 0.1f, 1f),
                Quaternion.identity, MakeCol(0.7f, 0.5f, 0.2f));

        // 4 legs
        foreach (var lp in new Vector3[] {
            new Vector3(-0.4f,1f,-0.4f), new Vector3(0.4f,1f,-0.4f),
            new Vector3(-0.4f,1f, 0.4f), new Vector3(0.4f,1f, 0.4f) })
        {
            MakeBar(slide.transform, lp, new Vector3(0.07f,2f,0.07f), Quaternion.identity,
                    MakeCol(0.6f,0.4f,0.1f));
        }

        // Slide ramp
        MakeBar(slide.transform, new Vector3(0f, 1f, 1.2f), new Vector3(0.9f, 0.08f, 2f),
                Quaternion.Euler(-30f, 0f, 0f), col);

        // Side rails
        MakeBar(slide.transform, new Vector3(-0.45f,1.15f,1.2f), new Vector3(0.05f,0.2f,2f),
                Quaternion.Euler(-30f,0f,0f), col);
        MakeBar(slide.transform, new Vector3( 0.45f,1.15f,1.2f), new Vector3(0.05f,0.2f,2f),
                Quaternion.Euler(-30f,0f,0f), col);

        // Ladder rungs
        for (int r = 0; r < 4; r++)
        {
            MakeBar(slide.transform,
                    new Vector3(0f, 0.4f + r * 0.45f, -0.5f),
                    new Vector3(0.7f, 0.05f, 0.05f),
                    Quaternion.identity, MakeCol(0.6f,0.4f,0.1f));
        }
    }

    void MakeSeesaw(Transform parent, Vector3 localPos)
    {
        GameObject ss = new GameObject("Seesaw");
        ss.transform.SetParent(parent);
        ss.transform.localPosition = localPos;

        // Base
        MakeBar(ss.transform, new Vector3(0f,0.3f,0f), new Vector3(0.2f,0.6f,0.2f),
                Quaternion.identity, MakeCol(0.5f,0.3f,0.1f));

        // Plank (on a pivot so it tilts)
        GameObject plank = MakeBar(ss.transform, new Vector3(0f,0.6f,0f),
                new Vector3(2.4f,0.08f,0.3f), Quaternion.identity, MakeCol(0.9f,0.5f,0.1f));
        plank.AddComponent<SeesawAnimator>();

        // Handles
        foreach (float side in new float[] { -1.1f, 1.1f })
        {
            MakeBar(ss.transform, new Vector3(side, 0.85f, 0f),
                    new Vector3(0.04f, 0.5f, 0.04f), Quaternion.identity, MakeCol(0.4f,0.2f,0.05f));
        }
    }

    // ─────────────────────────────────────────────────────────
    // BUTTERFLIES
    // ─────────────────────────────────────────────────────────
    void SpawnButterflies(int count)
    {
        Color[] wingCols = {
            new Color(1f, 0.5f, 0f),
            new Color(0.8f, 0.2f, 0.8f),
            new Color(0.2f, 0.6f, 1f),
            new Color(1f, 0.9f, 0.1f),
            new Color(1f, 0.3f, 0.3f),
        };

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-20f, 20f);
            float z = Random.Range(-20f, 20f);
            float y = Random.Range(0.5f, 2.5f);
            Color col = wingCols[Random.Range(0, wingCols.Length)];
            MakeButterfly(new Vector3(x, y, z), col);
        }
    }

    void MakeButterfly(Vector3 pos, Color col)
    {
        GameObject bf = new GameObject("Butterfly");
        bf.transform.position = pos;

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(bf.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale    = new Vector3(0.04f, 0.1f, 0.04f);
        body.GetComponent<Renderer>().material = MakeMat(new Color(0.1f, 0.05f, 0f));
        Destroy(body.GetComponent<Collider>());

        // Wings (flat cubes)
        GameObject wL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wL.name = "WingL";
        wL.transform.SetParent(bf.transform);
        wL.transform.localPosition = new Vector3(-0.12f, 0f, 0f);
        wL.transform.localScale    = new Vector3(0.18f, 0.01f, 0.14f);
        wL.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(wL.GetComponent<Collider>());

        GameObject wR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wR.name = "WingR";
        wR.transform.SetParent(bf.transform);
        wR.transform.localPosition = new Vector3(0.12f, 0f, 0f);
        wR.transform.localScale    = new Vector3(0.18f, 0.01f, 0.14f);
        wR.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(wR.GetComponent<Collider>());

        bf.AddComponent<ButterflyController>();
    }

    // ─────────────────────────────────────────────────────────
    // FENCE (around the field perimeter)
    // ─────────────────────────────────────────────────────────
    void BuildFence()
    {
        int posts = 32;
        float radius = 42f;
        Color wood = MakeCol(0.55f, 0.38f, 0.18f);

        for (int i = 0; i < posts; i++)
        {
            float angle = (i / (float)posts) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Post
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "FencePost";
            post.transform.position   = new Vector3(x, 0.5f, z);
            post.transform.localScale = new Vector3(0.12f, 1f, 0.12f);
            post.GetComponent<Renderer>().material = MakeMat(wood);
            Destroy(post.GetComponent<Collider>());

            // Rail to next post
            if (i < posts - 1)
            {
                float nextAngle = ((i + 1) / (float)posts) * Mathf.PI * 2f;
                float nx = Mathf.Cos(nextAngle) * radius;
                float nz = Mathf.Sin(nextAngle) * radius;
                Vector3 mid = new Vector3((x + nx) / 2f, 0.7f, (z + nz) / 2f);
                float dist  = Vector3.Distance(new Vector3(x, 0, z), new Vector3(nx, 0, nz));

                GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = "FenceRail";
                rail.transform.position   = mid;
                rail.transform.localScale = new Vector3(dist, 0.06f, 0.06f);
                rail.transform.LookAt(new Vector3(nx, 0.7f, nz));
                rail.GetComponent<Renderer>().material = MakeMat(wood);
                Destroy(rail.GetComponent<Collider>());
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    // POLLEN PARTICLES
    // ─────────────────────────────────────────────────────────
    void CreatePollenParticles()
    {
        GameObject go = new GameObject("PollenParticles");
        go.transform.position = new Vector3(0, 0.5f, 0);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop          = true;
        main.startLifetime = 8f;
        main.startSpeed    = 0.15f;
        main.startSize     = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor    = new Color(1f, 1f, 0.85f, 0.4f);
        main.maxParticles  = 200;

        var emission = ps.emission;
        emission.rateOverTime = 20f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius    = 20f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.1f);
        vel.y = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
    }

    // ─────────────────────────────────────────────────────────
    // CAMERA
    // ─────────────────────────────────────────────────────────
    void PositionCamera()
    {
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0f, 2f, -12f);
            Camera.main.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        }
    }

    // ─────────────────────────────────────────────────────────
    // COLLECTIBLES
    // ─────────────────────────────────────────────────────────
    void SpawnCollectibles(int count)
    {
        Color[] starColors = {
            new Color(1f, 0.9f, 0.1f),   // gold
            new Color(0.4f, 0.9f, 1f),   // cyan
            new Color(1f, 0.4f, 0.8f),   // pink
            new Color(0.5f, 1f, 0.4f),   // green
        };

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-28f, 28f);
            float z = Random.Range(-28f, 28f);
            Color col = starColors[Random.Range(0, starColors.Length)];
            MakeCollectibleOrb(new Vector3(x, 0.6f, z), col);
        }
    }

    void MakeCollectibleOrb(Vector3 pos, Color col)
    {
        GameObject orb = new GameObject("CollectibleOrb");
        orb.transform.position = pos;

        // Inner sphere
        GameObject inner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        inner.transform.SetParent(orb.transform);
        inner.transform.localPosition = Vector3.zero;
        inner.transform.localScale    = new Vector3(0.22f, 0.22f, 0.22f);
        inner.GetComponent<Renderer>().material = MakeMat(col);
        // keep collider on inner sphere so mouse raycast can hit it

        // Outer ring (flat torus-like disc using a scaled sphere)
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ring.transform.SetParent(orb.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale    = new Vector3(0.38f, 0.04f, 0.38f);
        Color ringCol = new Color(col.r * 0.8f, col.g * 0.8f, col.b * 0.8f);
        ring.GetComponent<Renderer>().material = MakeMat(ringCol);
        Destroy(ring.GetComponent<Collider>());

        orb.AddComponent<Collectible>();
    }

    void SetupCollectibleManager()
    {
        GameObject mgr = new GameObject("CollectibleManager");
        mgr.AddComponent<CollectibleManager>();
    }

    // ─────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────
    Material MakeMat(Color col)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader.name == "Hidden/InternalErrorShader")
            m = new Material(Shader.Find("Standard")); // fallback for non-URP
        m.color = col;
        return m;
    }

    Color MakeCol(float r, float g, float b) => new Color(r, g, b);

    GameObject MakeBar(Transform parent, Vector3 localPos, Vector3 localScale,
                       Quaternion localRot, Color col)
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.transform.SetParent(parent);
        bar.transform.localPosition = localPos;
        bar.transform.localScale    = localScale;
        bar.transform.localRotation = localRot;
        bar.GetComponent<Renderer>().material = MakeMat(col);
        Destroy(bar.GetComponent<Collider>());
        return bar;
    }
}
