#if IGLOO_UI
using Igloo.UI;
#endif
using UnityEngine;

namespace Igloo.Common
{
#pragma warning disable IDE0090 // Use New()...
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
    public class PlayerPointer : Singleton<PlayerPointer>
    {
        /// <summary>
        /// If True, Player Pointer is initialised
        /// </summary>
        bool isInit = false;

        /// <summary>
        /// If True, Igloo is in 3D mode
        /// </summary>
        public bool Draw3D = false;

        /// <summary>
        /// The last Raycast Hit of the crosshair
        /// </summary>
        public RaycastHit hit;

        /// <summary>
        /// The Igloo Crosshair Object
        /// </summary>
        public GameObject crosshair;

        /// <summary>
        /// Initial Crosshair Position
        /// </summary>
        Vector3 initPos;

        /// <summary>
        /// Initial Crosshair Rotation
        /// </summary>
        Vector3 initRot;

        /// <summary>
        /// Initial Crosshair Scale
        /// </summary>
        Vector3 initScal;

        /// <summary>
        /// If Truem, The raycast hit something 
        /// </summary>
        bool hasHit = false;


        /// <summary>
        /// When the player's crosshair is over the Igloo UI Canvas, the crosshair will switch to this color
        /// </summary>
        [SerializeField] private Color _onIglooCanvasColor = Color.black;

        /// <summary>
        /// When the player's crosshair is over anything other than the Igloo UI Canvas, 
        /// the crosshair will be this colour.
        /// </summary>
        [SerializeField] private Color _offIglooCanvasColor = Color.white;

        /// <summary>
        /// Logs the initial transform of the crosshair
        /// </summary>
        public void Init()
        {
#if IGLOO_PLAYER
            if(crosshair == null) crosshair = IglooManager.instance.PlayerManager.crosshair.gameObject;
            initPos = crosshair.transform.localPosition;
            initRot = crosshair.transform.localEulerAngles;
            initScal = crosshair.transform.localScale;
#endif
            isInit = true;
        }

        /// <summary>
        /// Adjusts the crosshair to be 3D mode or not.
        /// </summary>
        /// <param name="state">bool, If True: 3D mode active</param>
        public void SetDraw3D(bool state)
        {
            if (!isInit) Init();
            if (state)
            {
                if (crosshair)
                {
                    initPos = crosshair.transform.localPosition;
                    initRot = crosshair.transform.localEulerAngles;
                    initScal = crosshair.transform.localScale;
                }
            }
            else if (!state)
            {
                if (crosshair)
                {
                    crosshair.transform.localPosition = initPos;
                    crosshair.transform.localScale = initScal;
                    crosshair.transform.localEulerAngles = initRot;
                }

            }
            Draw3D = state;
        }

        /// <summary>
        /// Sets the UI to be visible
        /// </summary>
        /// <param name="state">Bool, If True: UI is Visible</param>
        public void SetUIActive(bool state)
        {
            if (!isInit) Init();

            if (state && !Draw3D)
            {
                crosshair.transform.localScale = initScal;
            }
            if (!state && !Draw3D)
            {
                if (crosshair)
                {
                    crosshair.transform.localPosition = initPos;
                    crosshair.transform.localScale = initScal;
                    crosshair.transform.localEulerAngles = initRot;
                }
            }
        }

        /// <summary>
        /// Mono Update Function
        /// Casts a ray forward of the pointer
        /// Positions the crosshair
        /// Invokes the Hit or Miss functions
        /// </summary>
        public virtual void Update() {
            if (!isInit) return;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
#if IGLOO_UI
                // Detect screen collision
                if (hit.transform.GetComponent<DrawUI>()) {
                    crosshair.transform.position = hit.point;
                    ScreenHit(hit.textureCoord);
                    crosshair.GetComponent<Renderer>().material.color = _onIglooCanvasColor;
                } else {
                    ScreenMiss();
                    crosshair.GetComponent<Renderer>().material.color = _offIglooCanvasColor;
                }
#endif
                // Crosshair positioning system
                crosshair.transform.rotation = Quaternion.FromToRotation(crosshair.transform.up, hit.normal) * crosshair.transform.rotation;
                if (!hasHit)
                {
                    crosshair.transform.localScale = initScal;
                    hasHit = true;
                }
            }
            else
            {
                ScreenMiss();
                // Crosshair positioning system
                crosshair.transform.localPosition = initPos;
                crosshair.transform.localScale = initScal;
                crosshair.transform.localEulerAngles = initRot;
                hasHit = false;
            }
        }


        public bool CastRay(LayerMask layerMask, out RaycastHit hit)
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ScreenHitPosition OnScreenHitPosition;
        public delegate void ScreenHitPosition(Vector2 pos);

        public ScreenMissCallback OnScreenMiss;
        public delegate void ScreenMissCallback();
        private void ScreenHit(Vector2 pos)
        {
            OnScreenHitPosition?.Invoke(pos);
        }
        private void ScreenMiss()
        {
            if (OnScreenHitPosition != null) OnScreenMiss();
        }
    }
}

