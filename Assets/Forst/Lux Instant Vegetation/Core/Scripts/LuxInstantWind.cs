using System.Collections;
using UnityEngine;

namespace LuxInstantVegetation
{

    [ExecuteAlways]
    [RequireComponent (typeof (WindZone))]
    public class LuxInstantWind : MonoBehaviour
    {
        [Header("Wind Multipliers")]
        [Space(8)]
        public float _MainWind = 1.0f;
        public float _Turbulence = 1.0f;
        public float _PulseMagnitude = 1.0f;

        [Space(8)]
        public bool _MapMainToTurbulence = true;
        public AnimationCurve _MainToTurbulence = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        [Header("Wind Fade Settings")]
        [Space(8)]
        public float _WindFadeDistance = 250.0f;

        private Transform _Trans;
        private WindZone _WindZone;
        private Vector3 _WindDirection;
        private float _WindStrength;
        private float _WindTurbulence;

        private static readonly int _LuxInstantWindEnabled = Shader.PropertyToID("_LuxInstantWindEnabled");
        private static readonly int _LuxInstantWind = Shader.PropertyToID("_LuxInstantWind");
        private static readonly int _LuxInstantTurbulence = Shader.PropertyToID("_LuxInstantTurbulence");
        private static readonly int _LuxInstantWindFadeParams = Shader.PropertyToID("_LuxInstantWindFadeParams");

        void OnEnable()
        {
            Init();
        }

        void OnDisable()
        {
            Shader.SetGlobalFloat(_LuxInstantWindEnabled, 0.0f);
        }

        void Init()
        {
            _Trans = this.transform;
            _WindZone = GetComponent<WindZone>();
            Shader.SetGlobalFloat(_LuxInstantWindEnabled, 1.0f);
            _MainToTurbulence.postWrapMode = WrapMode.ClampForever;
        }

        void Update ()
        {
    #if UNITY_EDITOR 
            if (!Application.isPlaying)
            {
                if (_Trans == null)
                {
                    Init();
                }
            }
    #endif

            _WindDirection = _Trans.forward;

            _WindStrength = _WindZone.windMain;
            var pulse = _WindZone.windPulseMagnitude * (1.0f + Mathf.Sin(Time.time * _WindZone.windPulseFrequency) + 1.0f + Mathf.Sin(Time.time * _WindZone.windPulseFrequency * 3.0f) ) * 0.5f;
            pulse *= _PulseMagnitude;
            _WindStrength += pulse;
            _WindStrength *=  _MainWind;

            _WindTurbulence = (!_MapMainToTurbulence) ? _WindStrength : _MainToTurbulence.Evaluate(_WindStrength);
            _WindTurbulence *= _WindZone.windTurbulence * _Turbulence;

            Shader.SetGlobalVector(_LuxInstantWind, new Vector4(_WindDirection.x, _WindDirection.y, _WindDirection.z, _WindStrength) );
            Shader.SetGlobalFloat(_LuxInstantTurbulence, _WindTurbulence);

            Shader.SetGlobalVector(_LuxInstantWindFadeParams, new Vector2(_WindFadeDistance * _WindFadeDistance, 1.0f / (_WindFadeDistance * _WindFadeDistance)));
            
        }
    }

}