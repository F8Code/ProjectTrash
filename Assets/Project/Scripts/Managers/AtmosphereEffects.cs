using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AtmosphereEffects : MonoBehaviour
{
    enum Effect
    {
        FlickerAll,
        FlickerOne,
        ShakeLight,
        ShakeMedium,
        ShakeHeavy,
        Fog,
        Smoke
    }

    const float MinInterval = 5f;
    const float MaxInterval = 15f;
    const float SfxVolume = 0.4f;
    const float SettleDuration = 1f;
    const float CrashDelay = 1f;

    static AtmosphereEffects _instance;

    static readonly int HandColorId = Shader.PropertyToID("_Color");

    readonly List<Light> _sceneLights = new();
    readonly Dictionary<Light, float> _lightIntensities = new();
    readonly MaterialPropertyBlock _handBlock = new();
    Renderer[] _handRenderers;

    public AudioClip _lightClip; 
    public AudioClip _lightsClip; 
    public AudioClip _machineryClip; 
    public AudioClip _truckClip; 
    public AudioClip _crashClip; 
    public AudioClip _fogClip; 
    public AudioClip _smokeClip;

    AudioSource _sfx; 

    Transform _camera;
    Vector3 _appliedShake;
    float _shakeTime;
    float _shakeDuration;
    float _shakeAmplitude;
    int _shakeSeed;

    Transform _dustRoot;
    readonly List<DustCloud> _dustClouds = new();
    Sprite[] _dustSprites;
    Effect? _lastNonLightEffect;

    class DustCloud
    {
        public Transform Transform;
        public SpriteRenderer Renderer;
        public Vector3 LocalVelocity;
        public Color Color;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TrySpawn(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TrySpawn(scene);

    static void TrySpawn(Scene scene)
    {
        if (scene.name != GameConstants.Scene.Game && scene.name != GameConstants.Scene.GameTimeBased)
            return;

        if (_instance != null)
            return;

        GameObject host = new("AtmosphereEffects");
        host.AddComponent<AtmosphereEffects>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        EnsureSfxSource();
        LoadClips();
        CacheLights();
        StartCoroutine(RunLoop());
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        RestoreLights();
        RestoreHand();
        ClearDustClouds();
        if (_camera != null)
            _camera.position -= _appliedShake;
        _appliedShake = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!TryGetCamera(out Transform camera))
            return;

        if (_camera != camera)
        {
            if (_camera != null)
                _camera.position -= _appliedShake;
            _appliedShake = Vector3.zero;
            _camera = camera;
            AttachDustRoot();
        }

        _camera.position -= _appliedShake;
        _appliedShake = ComputeShake();
        _camera.position += _appliedShake;
    }

    IEnumerator RunLoop()
    {
        yield return new WaitForSeconds(Random.Range(MinInterval, MaxInterval));

        while (!ShouldStopEffects())
        {
            yield return new WaitUntil(() => IsGameplayActive() || ShouldStopEffects());
            if (ShouldStopEffects())
                yield break;

            Effect effect = PickEffect();
            yield return Play(effect);
            yield return new WaitForSeconds(Random.Range(MinInterval, MaxInterval));
        }
    }

    static bool IsGameplayActive()
    {
        return GameManager.Instance != null && GameManager.Instance.CurrentState is GamePlayingState;
    }

    static bool ShouldStopEffects()
    {
        return GameManager.Instance == null || GameManager.Instance.CurrentState is GameOverState;
    }

    Effect PickEffect()
    {
        Effect picked;
        int guard = 0;
        do
        {
            picked = (Effect)Random.Range(0, 7);
            guard++;
        }
        while (guard < 24 && !IsLight(picked) && _lastNonLightEffect == picked);

        if (!IsLight(picked))
            _lastNonLightEffect = picked;

        return picked;
    }

    static bool IsLight(Effect effect) => effect is Effect.FlickerAll or Effect.FlickerOne;

    IEnumerator Play(Effect effect)
    {
        if (effect == Effect.ShakeHeavy)
            yield return new WaitForSeconds(CrashDelay);

        PlaySFX(effect);

        IEnumerator visual = effect switch
        {
            Effect.FlickerAll => FlickerLights(true, Random.Range(3f, 10f), 1.5f),
            Effect.FlickerOne => FlickerLights(false, Random.Range(4f, 12f), 2f),
            Effect.ShakeLight => Shake(0.01f, 10f, 0.3f),
            Effect.ShakeMedium => Shake(0.08f, 4f, 0.5f),
            Effect.ShakeHeavy => Shake(0.2f, 1.5f, 0.4f),
            Effect.Fog => PlayDust(fog: true),
            _ => PlayDust(fog: false)
        };

        yield return visual;
        StopSfx();
    }

    static float WithSettle(float duration) => Mathf.Max(duration, SettleDuration * 2f);

    static float SettleWeight(float elapsed, float duration)
    {
        float settle = Mathf.Min(SettleDuration, duration * 0.5f);
        if (elapsed <= settle)
            return Mathf.SmoothStep(0f, 1f, elapsed / settle);
        if (elapsed >= duration - settle)
            return Mathf.SmoothStep(0f, 1f, (duration - elapsed) / settle);
        return 1f;
    }

    void SetSfxWeight(float weight)
    {
        if (_sfx != null)
            _sfx.volume = SfxVolume * Mathf.Clamp01(weight);
    }

    void LoadClips()
    {
        _lightClip = LoadClip(_lightClip, "WAV_SingleLightSFX");
        _lightsClip = LoadClip(_lightsClip, "WAV_LightsSFX");
        _machineryClip = LoadClip(_machineryClip, "WAV_MachinerySFX");
        _truckClip = LoadClip(_truckClip, "WAV_TruckSFX");
        _crashClip = LoadClip(_crashClip, "WAV_CrashSFX");
        _fogClip = LoadClip(_fogClip, "WAV_FogSFX");
        _smokeClip = LoadClip(_smokeClip, "WAV_SmokeSFX");
    }

    static AudioClip LoadClip(AudioClip existing, string assetName)
    {
        if (existing != null)
            return existing;

        AudioClip clip = Resources.Load<AudioClip>($"AmbientSounds/{assetName}");
        if (clip != null)
            return clip;

        clip = Resources.Load<AudioClip>(assetName);
        if (clip != null)
            return clip;

#if UNITY_EDITOR
        string[] matches = UnityEditor.AssetDatabase.FindAssets($"{assetName} t:AudioClip");
        foreach (string guid in matches)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == assetName)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
#endif
        return null;
    }

    void EnsureSfxSource()
    {
        _sfx = gameObject.GetComponent<AudioSource>();
        if (_sfx == null)
            _sfx = gameObject.AddComponent<AudioSource>();

        _sfx.playOnAwake = false;
        _sfx.loop = false;
        _sfx.spatialBlend = 0f;
        _sfx.volume = SfxVolume;
    }

    void PlaySFX(Effect effect)
    {
        AudioClip clip = effect switch
        {
            Effect.FlickerAll => _lightsClip,
            Effect.FlickerOne => _lightClip,
            Effect.ShakeLight => _truckClip,
            Effect.ShakeMedium => _machineryClip,
            Effect.ShakeHeavy => _crashClip,
            Effect.Fog => _fogClip,
            Effect.Smoke => _smokeClip,
            _ => null
        };

        if (clip == null || _sfx == null)
            return;

        _sfx.Stop();
        _sfx.clip = clip;
        _sfx.volume = 0f;
        _sfx.Play();
    }

    void StopSfx()
    {
        if (_sfx == null)
            return;

        _sfx.Stop();
        _sfx.clip = null;
        _sfx.volume = 0f;
    }

    IEnumerator FlickerLights(bool all, float duration, float volume)
    {
        CacheLights();
        if (_sceneLights.Count == 0)
            yield break;

        duration = WithSettle(duration);
        List<Light> targets;
        if (all)
        {
            targets = _sceneLights;
        }
        else
        {
            List<Light> allowed = GetSingleFlickerLights();
            if (allowed.Count == 0)
                yield break;
            targets = new List<Light> { allowed[Random.Range(0, allowed.Count)] };
        }
        float seed = Random.Range(0f, 100f);
        float elapsed = 0f;

        while (elapsed < duration && !ShouldStopEffects())
        {
            if (IsGameplayActive())
            {
                elapsed += Time.deltaTime;
                float weight = SettleWeight(elapsed, duration);
                SetSfxWeight(weight * volume);

                for (int i = 0; i < targets.Count; i++)
                {
                    Light light = targets[i];
                    if (light == null || !_lightIntensities.TryGetValue(light, out float original))
                        continue;

                    float noise = Mathf.PerlinNoise(seed + i * 13.7f, Time.time * 14f);
                    bool on = noise > 0.38f;
                    float flickered = on ? original * Mathf.Lerp(0.15f, 1.15f, noise) : 0f;
                    light.intensity = Mathf.Lerp(original, flickered, weight);
                }

                // Hand uses an unlit cell shader, so scene light intensity never reaches it.
                float handNoise = Mathf.PerlinNoise(seed, Time.time * 14f);
                bool handOn = handNoise > 0.38f;
                float handMul = handOn ? Mathf.Lerp(0.15f, 1.15f, handNoise) : 0.08f;
                ApplyHandFlicker(Mathf.Lerp(1f, handMul, weight));
            }

            yield return null;
        }

        SetSfxWeight(0f);
        RestoreLights(targets);
        RestoreHand();
    }

    IEnumerator Shake(float amplitude, float duration, float sfxVol)
    {
        duration = WithSettle(duration);
        _shakeSeed = Random.Range(1, 9999);
        _shakeAmplitude = amplitude;
        _shakeDuration = duration;
        _shakeTime = 0f;

        while (_shakeTime < _shakeDuration && !ShouldStopEffects())
        {
            if (IsGameplayActive())
            {
                _shakeTime += Time.deltaTime;
                SetSfxWeight(SettleWeight(_shakeTime, _shakeDuration) * sfxVol);
            }

            yield return null;
        }

        SetSfxWeight(0f);
        _shakeAmplitude = 0f;
        _shakeDuration = 0f;
    }

    Vector3 ComputeShake()
    {
        if (_shakeAmplitude <= 0f || _shakeDuration <= 0f)
            return Vector3.zero;

        float strength = _shakeAmplitude * SettleWeight(_shakeTime, _shakeDuration);
        float t = Time.time * (18f + _shakeAmplitude * 40f);
        return new Vector3(
            (Mathf.PerlinNoise(_shakeSeed, t) - 0.5f) * 2f,
            (Mathf.PerlinNoise(_shakeSeed + 20f, t) - 0.5f) * 2f,
            (Mathf.PerlinNoise(_shakeSeed + 40f, t) - 0.5f) * 2f) * strength;
    }

    IEnumerator PlayDust(bool fog)
    {
        if (!TryGetCamera(out Transform camera))
            yield break;

        _camera = camera;
        AttachDustRoot();
        ClearDustClouds();

        Sprite[] sprites = LoadDustSprites();
        if (sprites == null || sprites.Length == 0)
            yield break;

        float duration = fog ? Random.Range(6f, 10f) : Random.Range(2.2f, 3.5f);
        float fadeIn = fog ? 1.2f : 0.35f;
        float fadeOut = fog ? 1.4f : 0.7f;
        float peakAlpha = fog ? 0.25f : 0.72f;
        Color color = fog ? new Color(0.82f, 0.86f, 0.9f) : new Color(0.04f, 0.04f, 0.04f);
        int count = fog ? 10 : 12;
        float sizeMin = fog ? 5.5f : 2.8f;
        float sizeMax = fog ? 11f : 5.5f;
        float drift = fog ? 0.04f : 0.18f;
        float sfxScale = fog ? 0.5f : 0.7f;

        for (int i = 0; i < count; i++)
        {
            GameObject cloudObject = new(fog ? "FogCloud" : "SmokeCloud");
            cloudObject.transform.SetParent(_dustRoot, false);
            cloudObject.transform.localPosition = new Vector3(
                Random.Range(-0.7f, 0.7f),
                Random.Range(-0.4f, 0.45f),
                Random.Range(0.45f, 0.95f));
            cloudObject.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            float size = Random.Range(sizeMin, sizeMax) * 0.22f;
            cloudObject.transform.localScale = new Vector3(size, size, 1f);

            SpriteRenderer renderer = cloudObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[Random.Range(0, sprites.Length)];
            renderer.color = new Color(color.r, color.g, color.b, 0f);
            renderer.sortingOrder = 40 + i;

            _dustClouds.Add(new DustCloud
            {
                Transform = cloudObject.transform,
                Renderer = renderer,
                LocalVelocity = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-0.25f, 0.35f),
                    Random.Range(-0.08f, 0.08f)) * drift,
                Color = color
            });
        }

        float elapsed = 0f;
        while (elapsed < duration && !ShouldStopEffects())
        {
            if (IsGameplayActive())
            {
                elapsed += Time.deltaTime;
                float weight = DustWeight(elapsed, duration, fadeIn, fadeOut);
                SetSfxWeight(weight * sfxScale);

                for (int i = 0; i < _dustClouds.Count; i++)
                {
                    DustCloud cloud = _dustClouds[i];
                    if (cloud.Transform == null)
                        continue;

                    cloud.Transform.localPosition += cloud.LocalVelocity * Time.deltaTime;
                    cloud.Renderer.color = new Color(cloud.Color.r, cloud.Color.g, cloud.Color.b, peakAlpha * weight);
                }
            }

            yield return null;
        }

        SetSfxWeight(0f);
        ClearDustClouds();
    }

    static float DustWeight(float elapsed, float duration, float fadeIn, float fadeOut)
    {
        if (elapsed < fadeIn)
            return Mathf.SmoothStep(0f, 1f, elapsed / fadeIn);
        if (elapsed > duration - fadeOut)
            return Mathf.SmoothStep(0f, 1f, (duration - elapsed) / fadeOut);
        return 1f;
    }

    void ClearDustClouds()
    {
        for (int i = 0; i < _dustClouds.Count; i++)
        {
            if (_dustClouds[i].Transform != null)
                Destroy(_dustClouds[i].Transform.gameObject);
        }

        _dustClouds.Clear();
    }

    void AttachDustRoot()
    {
        if (!TryGetCamera(out Transform camera))
            return;

        if (_dustRoot == null)
        {
            GameObject root = new("DustClouds");
            _dustRoot = root.transform;
        }

        _dustRoot.SetParent(camera, false);
        _dustRoot.localPosition = Vector3.zero;
        _dustRoot.localRotation = Quaternion.identity;
        _dustRoot.localScale = Vector3.one;
    }

    Sprite[] LoadDustSprites()
    {
        if (_dustSprites != null && _dustSprites.Length > 0)
            return _dustSprites;

        List<Sprite> sprites = new();
        Sprite[] resourceSprites = Resources.LoadAll<Sprite>("Dust01");
        if (resourceSprites != null)
            sprites.AddRange(resourceSprites);

#if UNITY_EDITOR
        if (sprites.Count == 0)
        {
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Project/Assets/VFX/SuccessFail/Dust01.png");
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                    sprites.Add(sprite);
            }
        }
#endif

        if (sprites.Count == 0)
        {
            Texture2D texture = Resources.Load<Texture2D>("Dust01");
#if UNITY_EDITOR
            texture ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Project/Assets/VFX/SuccessFail/Dust01.png");
#endif
            if (texture != null)
            {
                sprites.Add(Sprite.Create(texture, new Rect(0f, 128f, 128f, 128f), new Vector2(0.5f, 0.5f), 32f));
                sprites.Add(Sprite.Create(texture, new Rect(128f, 128f, 128f, 128f), new Vector2(0.5f, 0.5f), 32f));
                sprites.Add(Sprite.Create(texture, new Rect(0f, 0f, 128f, 128f), new Vector2(0.5f, 0.5f), 32f));
                sprites.Add(Sprite.Create(texture, new Rect(128f, 0f, 128f, 128f), new Vector2(0.5f, 0.5f), 32f));
            }
        }

        _dustSprites = sprites.ToArray();
        return _dustSprites;
    }

    void CacheLights()
    {
        _sceneLights.Clear();
        _lightIntensities.Clear();
        Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light == null || light.GetComponentInParent<TrashCan>() != null)
                continue;

            _sceneLights.Add(light);
            _lightIntensities[light] = light.intensity;
        }
    }

    List<Light> GetSingleFlickerLights()
    {
        List<Light> matches = new();
        foreach (Light light in _sceneLights)
        {
            if (light != null && IsSingleFlickerLight(light))
                matches.Add(light);
        }

        return matches;
    }

    static bool IsSingleFlickerLight(Light light)
    {
        string name = light.gameObject.name.Trim();
        if (name == "Player Spot Light" && HasParentNamed(light.transform, "Lights"))
            return true;
        if (name.StartsWith("Spot Light") && HasParentNamed(light.transform, "WhiteboardHUD"))
            return true;
        return false;
    }

    static bool HasParentNamed(Transform transform, string parentName)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name == parentName)
                return true;
            current = current.parent;
        }

        return false;
    }

    void RestoreLights(List<Light> targets = null)
    {
        IEnumerable<Light> lights = targets ?? _sceneLights;
        foreach (Light light in lights)
        {
            if (light != null && _lightIntensities.TryGetValue(light, out float original))
                light.intensity = original;
        }
    }

    void CacheHandRenderers()
    {
        if (_handRenderers != null && _handRenderers.Length > 0 && _handRenderers[0] != null)
            return;

        PlayerArm arm = PlayerManager.Instance != null
            ? PlayerManager.Instance.Arm
            : FindFirstObjectByType<PlayerArm>();
        if (arm == null)
            return;

        _handRenderers = arm.GetComponentsInChildren<Renderer>(true);
    }

    void ApplyHandFlicker(float multiplier)
    {
        CacheHandRenderers();
        if (_handRenderers == null)
            return;

        Color color = Color.white * Mathf.Clamp(multiplier, 0.05f, 1.15f);
        color.a = 1f;
        foreach (Renderer renderer in _handRenderers)
        {
            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(_handBlock);
            _handBlock.SetColor(HandColorId, color);
            renderer.SetPropertyBlock(_handBlock);
        }
    }

    void RestoreHand()
    {
        ApplyHandFlicker(1f);
    }

    bool TryGetCamera(out Transform camera)
    {
        PlayerCamera playerCamera = FindFirstObjectByType<PlayerCamera>();
        if (playerCamera != null)
        {
            Camera nested = playerCamera.GetComponent<Camera>()
                            ?? playerCamera.GetComponentInChildren<Camera>()
                            ?? playerCamera.GetComponentInParent<Camera>();
            camera = nested != null ? nested.transform : playerCamera.transform;
            return true;
        }

        Camera fallback = Camera.main;
        if (fallback == null)
            fallback = FindFirstObjectByType<Camera>();
        camera = fallback != null ? fallback.transform : null;
        return camera != null;
    }
}
