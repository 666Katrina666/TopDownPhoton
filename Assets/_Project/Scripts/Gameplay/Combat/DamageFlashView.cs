using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using Core.Base;
using Fusion;

/// <summary>
/// Визуальный эффект вспышки цвета при получении урона через EventBus
/// </summary>
public class DamageFlashView : LoggableMonoBehaviour
{
    #region Inspector
    [FoldoutGroup("Settings"), LabelText("Renderers")]
    [SerializeField] private SpriteRenderer[] _renderers = System.Array.Empty<SpriteRenderer>();

    [FoldoutGroup("Settings"), LabelText("Hit Color")]
    [SerializeField] private Color _hitColor = new Color(1f, 0.25f, 0.25f, 1f);

    [FoldoutGroup("Settings"), LabelText("Flash Duration"), MinValue(0.01f)]
    [SerializeField] private float _flashDuration = 0.15f;

    [FoldoutGroup("Settings"), LabelText("Use MPB")]
    [SerializeField] private bool _useMaterialPropertyBlock = true;

    [FoldoutGroup("Settings"), LabelText("Curve")]
    [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    #endregion

    #region Runtime
    private Color[] _originalColors = System.Array.Empty<Color>();
    private Coroutine _flashRoutine;
    private MaterialPropertyBlock _mpb;
    private NetworkObject _networkObject;
    #endregion

    private void Awake()
    {
        _networkObject = GetComponentInParent<NetworkObject>();
        if (_renderers == null || _renderers.Length == 0)
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        }

        _originalColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalColors[i] = _renderers[i] != null ? _renderers[i].color : Color.white;
        }

        if (_useMaterialPropertyBlock)
        {
            _mpb = new MaterialPropertyBlock();
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<DamageTakenEvent>(OnDamageTaken);
        Log("Подписка на DamageTakenEvent");
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
        RestoreOriginal();
    }

    private void OnDamageTaken(DamageTakenEvent evt)
    {
        if (_networkObject == null || evt.Target != _networkObject)
        {
            return;
        }

        if (!isActiveAndEnabled)
        {
            return;
        }

        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            RestoreOriginal();
        }

        _flashRoutine = StartCoroutine(FlashCoroutine());
        Log("Hit flash triggered");
    }

    private IEnumerator FlashCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < _flashDuration)
        {
            float t = Mathf.Clamp01(elapsed / _flashDuration);
            float k = Mathf.Clamp01(_curve.Evaluate(t));
            ApplyColor(Color.LerpUnclamped(GetOriginalMixedColor(), _hitColor, k));
            elapsed += Time.deltaTime;
            yield return null;
        }

        RestoreOriginal();
        _flashRoutine = null;
    }

    private void ApplyColor(Color color)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r == null) continue;

            if (_useMaterialPropertyBlock)
            {
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_Color", color);
                r.SetPropertyBlock(_mpb);
            }
            else
            {
                r.color = color;
            }
        }
    }

    private void RestoreOriginal()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r == null) continue;

            if (_useMaterialPropertyBlock)
            {
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_Color", _originalColors[i]);
                r.SetPropertyBlock(_mpb);
            }
            else
            {
                r.color = _originalColors[i];
            }
        }
    }

    private Color GetOriginalMixedColor()
    {
        // Берём первый цвет как базовый, для унификации вспышки по всем рендерам
        return _originalColors.Length > 0 ? _originalColors[0] : Color.white;
    }
}


