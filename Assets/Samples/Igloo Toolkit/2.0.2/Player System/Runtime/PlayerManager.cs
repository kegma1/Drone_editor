using System;
using UnityEngine;
using Igloo.Controllers;
using Igloo.Common;

namespace Igloo.Common
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0090 // Use 'new(...)'

    /// <summary>
    /// Igloo Player Manager Class
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerManager : MonoBehaviour
    {
        /// <summary>
        /// The name of the Player
        /// </summary>
        public string playerName = "player";

        /// <summary>
        /// The first person camera of the player.
        /// </summary>
        public Camera m_Camera;

        /// <summary>
        /// If the player is being used.
        /// Internal event that sets the palyer specific components to be 
        /// enabled / disabled depending on set.
        /// </summary>
        public bool UsePlayer
        {
            get { return enabled; }
            set
            {
                GetComponent<CharacterController>().enabled = value;
                GetComponent<Rigidbody>().detectCollisions = !value;
                enabled = value;
            }
        }

        /// <summary>
        /// Igloo Head Manager
        /// </summary>
        public HeadManager headManager;

        /// <summary>
        /// Igloo Crosshair Component
        /// </summary>
        public Crosshair crosshair;

        /// <summary>
        /// Igloo Player Pointer Component
        /// </summary>
        public PlayerPointer pointer;

        /// <summary>
        /// Igloo VR Controller Component
        /// </summary>
        public VRController vrController;

        /// <summary>
        /// Rotation Mode Enum
        /// </summary>
        public enum ROTATION_MODE { IGLOO_360, IGLOO_NON_360, GAME }

        /// <summary>
        /// Current Rotation Mode Enum
        /// </summary>
        public ROTATION_MODE rotationMode = ROTATION_MODE.IGLOO_360;

        /// <summary>
        /// Movement Input Enum
        /// </summary>
        public enum MOVEMENT_INPUT { STANDARD, VRCONTROLLER };
        /// <summary>
        /// Current Movement Input Enum
        /// </summary>
        public MOVEMENT_INPUT movementInput = MOVEMENT_INPUT.STANDARD;

        /// <summary>
        /// Movement Mode Enum
        /// </summary>
        public enum MOVEMENT_MODE { WALKING, FLYING, FLYING_GHOST };
        /// <summary>
        /// Current Movement Mode Enum
        /// </summary>
        public MOVEMENT_MODE movementMode = MOVEMENT_MODE.WALKING;

        /// <summary>
        /// Cached movement mode enum
        /// </summary>
        private MOVEMENT_MODE cachedMovmentMode = MOVEMENT_MODE.WALKING;

        /// <summary>
        /// Cached rotation mode enum
        /// </summary>
        private ROTATION_MODE cachedRotationMode = ROTATION_MODE.IGLOO_360;

        /// <summary>
        /// Cached Gravity multiplyer float value
        /// </summary>
        private float cached_m_GravityMultiplier = 1.0f;

        /// <summary>
        /// Cached Stick to Ground Force float value
        /// </summary>
        private float cached_m_StickToGroundForce = 1.0f;

        /// <summary>
        /// Is the Player walking
        /// </summary>
        [Header("Character Controller Settings")]
        [SerializeField] private bool m_IsWalking = false;
        /// <summary>
        /// Players walk speed
        /// </summary>
        public float m_WalkSpeed = 5f;
        /// <summary>
        /// Players run speed
        /// </summary>
        public float m_RunSpeed = 10f;

        /// <summary>
        /// Is the Igloo Lift Axis available. 
        /// </summary>
        private bool isIglooLift = false;

        private bool isSetup = false;
        /// <summary>
        /// Players jump speed
        /// </summary>
        [SerializeField] private float m_JumpSpeed = 10f;
        /// <summary>
        /// Player stick to ground force
        /// </summary>
        [SerializeField] private float m_StickToGroundForce = 2f;
        /// <summary>
        /// Player gravity multiplyer value
        /// </summary>
        [SerializeField] private float m_GravityMultiplier = 1f;

        /// <summary>
        /// MouseLook component reference
        /// </summary>
        [SerializeField] private DefaultLook m_DefaultLook = null;

        /// <summary>
        /// VRControllerLook component reference
        /// </summary>
        [SerializeField] private VRControllerLook m_vrControllerLook = null;

        /// <summary>
        /// Value for Xbox button from Igloo Cast or Control
        /// </summary>
        internal bool m_OSCXDown, m_OSCADown, m_OSCBDown, m_OSCYDown;

        /// <summary>
        /// Stored value for D-Pad from Igloo Cast or Control
        /// </summary>
        private Vector2 m_OSCdPadValue;

        /// <summary>
        /// Is the Player Jumping.
        /// </summary>
        private bool m_Jump;

        /// <summary>
        /// Vector2 of the current Input
        /// </summary>
        private Vector2 m_Input;

        /// <summary>
        /// Vector3 for the players move direction
        /// </summary>
        private Vector3 m_MoveDir = Vector3.zero;

        /// <summary>
        /// Reference for the Players character controller component
        /// </summary>
        private CharacterController m_CharacterController;

        /// <summary>
        /// The physics collision flags for the player
        /// </summary>
        private CollisionFlags m_CollisionFlags;

        /// <summary>
        /// Bool, was the player previously on the ground.
        /// </summary>
        private bool m_PreviouslyGrounded;

        /// <summary>
        /// bool, is currently jumping.
        /// </summary>
        private bool m_Jumping;

        /// <summary>
        /// float, current lift input
        /// </summary>
        private float lift = 0.0f;

        /// <summary>
        /// Current Pressed duration of the VR Rotation Button
        /// </summary>
        private float m_currRotButtonTime = 0.0f;

        /// <summary>
        /// Activation time of the VR Rotation Button. 
        /// </summary>
        /// <remarks>
        /// Stops dead time errors with the VR rotation button 
        /// </remarks>
        private float m_VrRotActivateTime = 0.0f;

        /// <summary>
        /// Bool, Is the VRRotation Button held down
        /// </summary>
        private bool m_VrRotButtonHeld = false;

        /// <summary>
        /// Bool, is the pitch of the head being inverted
        /// </summary>
        public bool m_invertPitch = false;

        /// <summary>
        /// Igloo Retail Specific
        /// Bool, is the player in store designer mode.
        /// Stops the player moving vertially. 
        /// </summary>
        public bool m_storeDesignerMode = false;

        /// <summary>
        /// cached smoothing value for player movement and rotation smoothing.
        /// </summary>
        private float m_smoothTime = 10f;

        /// <summary>
        /// Sets the smoothing value across all Look classes.
        /// </summary>
        public float SmoothTime
        {
            get { return m_smoothTime; }
            set
            {
                m_DefaultLook.smoothTime = value;
                m_smoothTime = value;
            }
        }


        /// <summary>
        /// Vector2, cached VR Controller pad position 
        /// </summary>
        private Vector2 vrControllerPadPosition;

        /// <summary>
        /// Bool, Is the VR Controller Pad Button pressed
        /// </summary>
        private bool vrControllerPadButton = false;

        /// <summary>
        /// Bool, Is the VR Controller Grip Button held
        /// </summary>
        private bool vrControllerGripButton = false;

        /// <summary>
        /// Rigidbody ID adjustment for Optitrack controller
        /// </summary>
        private int optitrackControllerRigidBodyID = 2;

        /// <summary>
        /// Bool, Is the player Frozen.
        /// </summary>
        public bool PlayerFrozen { get; set; } = false;

        /// <summary>
        /// Sets the Player Settings within the IglooSettings XML
        /// </summary>
        /// <param name="ps">The Populated PlayerSettings class</param>
        public void SetSettings(PlayerSettings ps)
        {
            if (ps == null) return;
            playerName = ps.Name;
            movementMode = (MOVEMENT_MODE)ps.movementMode;
            movementInput = (MOVEMENT_INPUT)ps.movementInput;
            rotationMode = (ROTATION_MODE)ps.rotationMode;
            UsePlayer = ps.usePlayer;
            SmoothTime = ps.smoothTime;
            if (ps.runSpeed != 0) m_RunSpeed = ps.runSpeed;
            if (ps.walkSpeed != 0) m_WalkSpeed = ps.walkSpeed;
            SetCrosshairMode((Crosshair.CROSSHAIR_MODE)ps.crosshairHideMode);
            optitrackControllerRigidBodyID = ps.optitrackControllerRigidBodyID;
        }

        /// <summary>
        /// Returns the Player Settings from the IglooSettingsXML
        /// </summary>
        /// <returns>A populated PlayerSettings class</returns>
        public PlayerSettings GetSettings()
        {
            PlayerSettings ps = new PlayerSettings
            {
                Name = playerName,
                movementMode = (int)movementMode,
                rotationMode = (int)rotationMode,
                movementInput = (int)movementInput,
                usePlayer = UsePlayer,
                walkSpeed = m_WalkSpeed,
                runSpeed = m_RunSpeed,
                smoothTime = m_smoothTime,
                crosshairHideMode = (int)GetCrosshairMode(),
                isCrosshair3D = pointer.Draw3D,
                optitrackControllerRigidBodyID = optitrackControllerRigidBodyID, 
            };
            return ps;
        }

        /// <summary>
        /// Mono Start Function.
        /// Caches all the Player components and values.
        /// Assigns functions to NetworkManager delegations
        /// Enables/Disables child objects depending on RotationInput Enum
        /// </summary>
        public void Setup()
        {
            _ = this.gameObject.GetComponent<CharacterController>() ? m_CharacterController = this.gameObject.GetComponent<CharacterController>() : m_CharacterController = this.gameObject.AddComponent<CharacterController>();
            if (!this.gameObject.GetComponent<Rigidbody>()) this.gameObject.AddComponent<Rigidbody>();
            
            
            if (!headManager) headManager = GetComponentInChildren<HeadManager>();
            if (!m_Camera) m_Camera = headManager.CreateCamera();
            pointer = m_Camera.GetComponent<PlayerPointer>();
            GetComponent<CharacterController>().enabled = true;
            crosshair = CreateCrosshairObject(m_Camera.gameObject, 0.15f, new Vector3(0, 0, 2.8f), new Vector3(-90f, 0, 0), Crosshair.CROSSHAIR_MODE.SHOW);
            // If headtracking, setup VR controller look. Otherwise go with default. 
            if (HeadManager.instance.HeadTracking)
            {
                GameObject vrc = new GameObject("VR Controller");
                vrController = vrc.AddComponent<VRController>();
                LineRenderer lr = vrc.AddComponent<LineRenderer>();
                lr.startColor = Color.green;
                lr.endColor = Color.white;
                vrc.transform.SetParent(this.transform.parent);
                vrc.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                vrController.crosshair = CreateCrosshairObject(vrc, 0.15f, Vector3.zero, Vector3.zero, Crosshair.CROSSHAIR_MODE.HIDE).gameObject;

                m_vrControllerLook = new VRControllerLook();
                m_vrControllerLook.Init(transform, m_Camera.transform, playerName, m_smoothTime);
                m_vrControllerLook.vrControllerGO = vrController.gameObject;
                m_vrControllerLook.UpdateLastFrameY();

                /// Disable Player Pointer system.
                pointer.enabled = false;
                crosshair.gameObject.SetActive(false);
                vrController.gameObject.SetActive(true);
                /// Setup Optitrack or VRPN based on the headmanager's option.
                if (HeadManager.instance.headTrackingInput == HeadManager.HeadTrackingInput.Optitrack)
                {
                    OptitrackRigidBodyIgloo optitrack = vrController.gameObject.AddComponent<OptitrackRigidBodyIgloo>();
                    optitrack.Setup(optitrackControllerRigidBodyID, true, true, headManager.optitrackServerIPAddress, headManager.GetLocalIPAddress());
                } else if (HeadManager.instance.headTrackingInput == HeadManager.HeadTrackingInput.VRPN)
                {
                    Igloo.VRPN.TrackerSettings vrpn = vrController.gameObject.AddComponent<Igloo.VRPN.TrackerSettings>();
                    vrpn.Setup("Controller", 0, true, true, headManager.optitrackServerIPAddress);
                }
            }
            else
            {
                m_DefaultLook = new DefaultLook();
                m_DefaultLook.Init(transform, m_Camera.transform, playerName, m_smoothTime);
            }

            // Register to events
            
            NetworkManagerOSC.instance.OnPlayerWalkSpeed += SetWalkSpeed;
            NetworkManagerOSC.instance.OnPlayerRunSpeed += SetRunSpeed;
            NetworkManagerOSC.instance.OnPlayerRotationMode += SetRotationMode;
            NetworkManagerOSC.instance.OnPlayerMovementInput += SetMovementInput;
            NetworkManagerOSC.instance.OnPlayerMovementMode += SetMovementMode;
            NetworkManagerOSC.instance.OnPlayerSmoothTime += SetSmoothTime;
            NetworkManagerOSC.instance.OnVrPadPositionEvent += SetVRControllerPadPosition;
            NetworkManagerOSC.instance.OnVrButtonEvent += SetVRControllerButtons;
            NetworkManagerOSC.instance.OnPlayerDPadMovement += MovePlayerOSC;
            NetworkManagerOSC.instance.OnPlayerActionEvent += PlayerActionOSC;

            isIglooLift = IsAxisAvailable("IglooLift");
            UpdateMovementMode();
            isSetup = true;
        }

        private Crosshair CreateCrosshairObject(GameObject parent, float scale, Vector3 startPos, Vector3 startRot, Crosshair.CROSSHAIR_MODE startMode)
        {
            GameObject crosshair = new GameObject("World Space Crosshair");
            crosshair.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Plane.fbx");
            crosshair.AddComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/IglooCrosshair");
            if (crosshair.GetComponent<MeshRenderer>().material.mainTexture == null) crosshair.GetComponent<MeshRenderer>().material.mainTexture = (Texture2D)Resources.Load("Textures/CrossHairSprite");
            crosshair.transform.SetParent(parent.transform);
            crosshair.transform.localPosition = startPos;
            crosshair.transform.localRotation = Quaternion.Euler(startRot);
            crosshair.transform.localScale = new Vector3(scale, scale, scale);
            crosshair.AddComponent<Crosshair>().crosshairMode = startMode;
            return crosshair.GetComponent<Crosshair>();
        }

        /// <summary>
        /// Projects mouse position to ground.
        /// </summary>
        /// <param name="mousePos">The mouse position.</param>
        /// <returns>The mouse  projection on ground.</returns>
        public Vector3 ProjectToGround(Vector3 mousePos)
        {
            Ray ray = m_Camera.ScreenPointToRay(mousePos);

            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }
        #region OSCInputEvents

        private void SetWalkSpeed(string name, float value)
        {
            if (name == playerName) m_WalkSpeed = value;
        }
        private void SetRunSpeed(string name, float value)
        {
            if (name == playerName) m_RunSpeed = value;
        }
        private void SetRotationMode(string name, int value)
        {
            if (name == playerName) rotationMode = (ROTATION_MODE)value;
        }
        private void SetMovementInput(string name, int value)
        {
            if (name == playerName) movementInput = (MOVEMENT_INPUT)value;
        }
        private void SetMovementMode(string name, int value)
        {
            if (name == playerName) movementMode = (MOVEMENT_MODE)value;
        }
        private void SetSmoothTime(string name, float value)
        {
            if (name == playerName) SmoothTime = value;
        }
        private void SetVRControllerPadPosition(int deviceID, Vector2 movement)
        {
            vrControllerPadPosition = movement;
        }
        private void SetVRControllerButtons(int deviceID, int buttonID, bool state)
        {

            if (buttonID == 2) vrControllerPadButton = state;
            else if (buttonID == 3) vrControllerGripButton = state;

        }
        #endregion

        /// <summary>
        /// Mono Update Event. Called once per frame
        /// Updates Movement and Rotaton Mode
        /// Checks if Jump has been pressed, or if the player is still jumping.
        /// </summary>
        private void Update()
        {
            if (!isSetup|| PlayerFrozen) return;
            if (movementMode != cachedMovmentMode) UpdateMovementMode();
            if (rotationMode != cachedRotationMode) UpdateRotationMode();

            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = Input.GetButtonDown("Jump");
                m_Jump = m_OSCYDown;
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

            if (movementMode != cachedMovmentMode) UpdateMovementMode();
            if (rotationMode != cachedRotationMode) UpdateRotationMode();

        }

        /// <summary>
        /// Updates the Players Cached movement mode enum.
        /// Adjusts the gravity system based on the new MovementType
        /// </summary>
        public void UpdateMovementMode()
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            switch (movementMode)
            {
                case MOVEMENT_MODE.FLYING:
                    rigidbody.useGravity = false;
                    rigidbody.isKinematic = true;
                    if (cachedMovmentMode == MOVEMENT_MODE.WALKING)
                    {
                        cached_m_GravityMultiplier = m_GravityMultiplier;
                        cached_m_StickToGroundForce = m_StickToGroundForce;
                    }
                    m_StickToGroundForce = 0;
                    m_GravityMultiplier = 0;
                    break;

                case MOVEMENT_MODE.FLYING_GHOST:
                    rigidbody.useGravity = false;
                    rigidbody.isKinematic = false;
                    if (cachedMovmentMode == MOVEMENT_MODE.WALKING)
                    {
                        cached_m_GravityMultiplier = m_GravityMultiplier;
                        cached_m_StickToGroundForce = m_StickToGroundForce;
                    }
                    m_StickToGroundForce = 0;
                    m_GravityMultiplier = 0;
                    break;

                case MOVEMENT_MODE.WALKING:
                    rigidbody.useGravity = true;
                    rigidbody.isKinematic = true;
                    m_GravityMultiplier = cached_m_GravityMultiplier;
                    m_StickToGroundForce = cached_m_StickToGroundForce;
                    break;
            }
            cachedMovmentMode = movementMode;
        }

        /// <summary>
        /// On press of any action button within Igloo Cast or Control, these events will be called.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Xvalue"></param>
        /// <param name="Yvalue"></param>
        /// <param name="Avalue"></param>
        /// <param name="Bvalue"></param>
        private void PlayerActionOSC(string name, bool Xvalue, bool Yvalue, bool Avalue, bool Bvalue) {
            m_OSCADown = Avalue;
            m_OSCBDown = Bvalue;
            m_OSCXDown = Xvalue;
            m_OSCYDown = Yvalue;
        }


        /// <summary>
        /// On Press of dPad button within Igloo Cast or Control, this will move the player
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void MovePlayerOSC(string name, Vector2 value) {
            m_OSCdPadValue = value;
        }

        /// <summary>
        /// Updates the Player Rotation Mode.
        /// Set's everything to Zero before it's changed. 
        /// </summary>
        private void UpdateRotationMode()
        {
            this.transform.localEulerAngles = Vector3.zero;
            m_Camera.transform.parent.transform.localEulerAngles = Vector3.zero;
            m_Camera.transform.localEulerAngles = Vector3.zero;
            headManager.transform.localEulerAngles = Vector3.zero;

            cachedRotationMode = rotationMode;
        }

        /// <summary>
        /// Mono FixedUpdate Function.
        /// Sets the rotation and movement of the player once all calculations are complete.
        /// </summary>
        private void FixedUpdate()
        {
            if (movementMode != cachedMovmentMode) UpdateMovementMode();
            if (PlayerFrozen) return;
            float speed = 0f;
            GetInput(out float curspeed);

            if (curspeed > speed)
            {
                speed = Mathf.Lerp(speed, curspeed, m_WalkSpeed * Time.smoothDeltaTime);
            }
            else if (speed > curspeed)
            {
                speed = Mathf.Lerp(curspeed, speed, m_WalkSpeed * Time.smoothDeltaTime);
            }
            // always move along the camera forward as it is the direction that it being aimed at
            // unless using a VR controller in which case use it's forward vector
            Vector3 desiredMove;
            if (HeadManager.instance.HeadTracking)
            {
                desiredMove = vrController.transform.forward * m_Input.y + vrController.transform.right * m_Input.x;
            }
            else if (m_storeDesignerMode) // if in store designer mode, only move relative to global forward and right. 
            {
                desiredMove = Vector3.forward * m_Input.y + Vector3.right * m_Input.x;
            }
            else
            {
                desiredMove = (m_Camera.transform.forward * m_Input.y) + m_Camera.transform.right * m_Input.x;
            }

            m_MoveDir.x = desiredMove.x * speed;
            if (movementMode == MOVEMENT_MODE.FLYING || movementMode == MOVEMENT_MODE.FLYING_GHOST)
            {
                if (lift == 0) m_MoveDir.y = desiredMove.y * speed;
                else m_MoveDir.y = lift * speed;
            }
            m_MoveDir.z = desiredMove.z * speed;

            if (movementMode != MOVEMENT_MODE.FLYING_GHOST)
            {
                if (m_CharacterController.isGrounded)
                {
                    m_MoveDir.y = -m_StickToGroundForce;

                    if (m_Jump)
                    {
                        m_MoveDir.y = m_JumpSpeed;
                        m_Jump = false;
                        m_Jumping = true;
                    }
                }
                else
                {
                    m_MoveDir += m_GravityMultiplier * Time.smoothDeltaTime * Physics.gravity;
                }
            }

            if (!PlayerFrozen)
            {
                if (movementMode != MOVEMENT_MODE.FLYING_GHOST)
                    m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

                else if (movementMode == MOVEMENT_MODE.FLYING_GHOST)
                {
                    transform.Translate(m_MoveDir * Time.fixedDeltaTime, Space.World);
                }
            }
        }

        /// <summary>
        /// Used to Teleport the player if required. 
        /// </summary>
        /// <param name="floorNode">Moves the player to the location of the transform.</param>
        public void Teleport(Vector3 position)
        {
            gameObject.transform.position = position;
        }

        /// <summary>
        /// Gets the input vector based on the MovementInput type
        /// </summary>
        /// <param name="speed">float out. Player Speed</param>
        private void GetInput(out float speed)
        {
            float horizontal = m_OSCdPadValue.x;
            float vertical = m_OSCdPadValue.y;

            switch (movementInput)
            {
                case MOVEMENT_INPUT.STANDARD:
                    horizontal = Input.GetAxis("Horizontal");
                    vertical = Input.GetAxis("Vertical");
                    if(isIglooLift) lift = Input.GetAxis("IglooLift");
                    break;
                case MOVEMENT_INPUT.VRCONTROLLER:
                    if (vrControllerPadButton)
                    {
                        horizontal = vrControllerPadPosition.x;
                        vertical = vrControllerPadPosition.y;
                    }
                    else
                    {
                        horizontal = Input.GetAxis("Horizontal");
                        vertical = Input.GetAxis("Vertical");
                    }

                    if (Input.GetButton("Fire1"))
                    {
                        if (!m_VrRotButtonHeld)
                        {
                            m_currRotButtonTime += Time.deltaTime;

                            if (m_currRotButtonTime >= m_VrRotActivateTime)
                            {
                                m_VrRotButtonHeld = true;
                                //Reset m_lastFrameY 
                                m_vrControllerLook.UpdateLastFrameY();
                            }
                        }
                    }
                    else
                    {
                        m_VrRotButtonHeld = false;
                        m_currRotButtonTime = 0.0f;
                    }
                    break;
                default:
                    break;
            }
            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            //m_IsWalking = !Input.GetKey(KeyCode.LeftShift); // Igloo - removed
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton5) || Input.GetKey(KeyCode.JoystickButton4))
            {
                m_IsWalking = false;
            }
            else
                m_IsWalking = true;
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }
        }

        /// <summary>
        /// Rotates the players view based on the Rotation Mode Enum. 
        /// </summary>
        private void RotateView()
        {
            Transform yRotationTransform = m_Camera.transform.parent;
            Transform xRotationTransform = m_Camera.transform;

            switch (rotationMode)
            {
                case ROTATION_MODE.IGLOO_360:
                    xRotationTransform = m_Camera.transform;
                    yRotationTransform = headManager.transform;
                    break;
                case ROTATION_MODE.IGLOO_NON_360:
                    if (HeadManager.instance.HeadTracking)
                    {
                        xRotationTransform = this.transform;
                        yRotationTransform = this.transform;

                    }
                    else
                    {
                        xRotationTransform = m_Camera.transform;
                        yRotationTransform = headManager.transform;
                    }
                    break;
                case ROTATION_MODE.GAME:
                    xRotationTransform = headManager.transform;
                    yRotationTransform = this.transform;
                    break;
                default:
                    break;
            }

            

            if(HeadManager.instance.HeadTracking)
            { 
                if(m_VrRotButtonHeld)
                    m_vrControllerLook.LookRotation(yRotationTransform, xRotationTransform, m_invertPitch);
            } 
            else
            {
                m_DefaultLook.LookRotation(yRotationTransform, xRotationTransform, m_invertPitch);
            }
        }

        /// <summary>
        /// Called if the Character Controller Registers a Physics Hit
        /// </summary>
        /// <param name="hit"></param>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }

        /// <summary>
        /// Sets the crosshair to be active or hidden depending on the CrossHair Mode Input
        /// </summary>
        /// <param name="mode">Enum, Crosshair Mode Type</param>
        public void SetCrosshairMode(Crosshair.CROSSHAIR_MODE mode)
        {
            if (crosshair)
            {
                crosshair.SetMode(mode);
            }
        }

        /// <summary>
        /// Retuns the current CrossHair Mode Enum
        /// </summary>
        /// <returns>Crosshair Mode Enum</returns>
        private Crosshair.CROSSHAIR_MODE GetCrosshairMode()
        {
            Crosshair.CROSSHAIR_MODE mode = Crosshair.CROSSHAIR_MODE.HIDE;
            if (crosshair) mode = crosshair.crosshairMode;
            return mode;
        }

        /// <summary>
        /// Checks to see if an Input axis exists.
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        bool IsAxisAvailable(string axisName) {
            try {
                Input.GetAxis(axisName);
                return true;
            } catch (UnityException exc) {
                Debug.Log(exc.ToString());
                return false;
            }
        }
    }

    /// <summary>
    /// Base class for Igloo Look System. 
    /// Controls how the character rotation system works. 
    /// </summary>
    [Serializable]
    public class Look
    {
        public bool clampVerticalRotation = false;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth = true;
        public float smoothTime = 10f;
        public string m_playerName;

        public float yRot;
        public float xRot;

        protected Quaternion m_CharacterTargetRot;
        protected Quaternion m_CameraTargetRot;

        /// <summary>
        /// Initiates the Look System. 
        /// Caches initial rotations.
        /// Caches the player name.
        /// </summary>
        /// <param name="character">transform of the Player</param>
        /// <param name="camera">transform of the Camera</param>
        /// <param name="playerName">string. Players Name</param>
        /// <param name="smoothTime">float. SmoothTime</param>
        public virtual void Init(Transform character, Transform camera, string playerName, float smoothTime)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            m_playerName = playerName;
        }

        /// <summary>
        /// Base function to override, to calculate and apply the Look rotation.
        /// </summary>
        /// <param name="character">transform. The Character</param>
        /// <param name="camera">transform. The Camera</param>
        /// <param name="invertPitch">bool. Is the pitch Inverted</param>
        /// <param name="rotationMode">Enum Rotation Mode. current Rotation Mode</param>
        public virtual void LookRotation(Transform character, Transform camera, bool invertPitch)
        {

        }

    }

    [Serializable]
    public class DefaultLook : Look
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        private Vector2 oscInput = Vector2.zero;
        private Vector2 lastOsc = Vector2.zero;

        public override void Init(Transform character, Transform camera, string playerName, float _smoothTime)
        {
            base.Init(character, camera, playerName, _smoothTime);
            NetworkManagerOSC.instance.OnPlayerRotationWarper += SetRotation;
        }

        public override void LookRotation(Transform character, Transform camera, bool invertPitch)
        {
            if (oscInput == lastOsc)
            {
                yRot = (Input.GetAxis("Mouse X") + Input.GetAxis("Right Stick Y Axis")) * XSensitivity;
                xRot = (Input.GetAxis("Mouse Y") + Input.GetAxis("Right Stick X Axis")) * YSensitivity;

                if (invertPitch) xRot = -xRot;

                m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
                m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

                if (clampVerticalRotation) m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }
            else
            {
                yRot = oscInput.y;
                xRot = oscInput.x;
                m_CharacterTargetRot = Quaternion.Euler(0, yRot, 0f);
                m_CameraTargetRot = Quaternion.Euler(xRot, 0f, 0f);
            }

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.deltaTime);
            } else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            lastOsc = oscInput;
        }

        void SetRotation(string name, Vector3 rot)
        {
            oscInput = (Vector2)rot;
        }


        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }

    [Serializable]
    public class VRControllerLook : Look
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 1.5f;
        public float rightBoundDegrees = 180.0f;
        public float leftBoundDegrees = -180.0f;
        public GameObject vrControllerGO;

        public float m_lastFrameY = 0.0f;
        public void UpdateLastFrameY()
        {
            m_lastFrameY = vrControllerGO.transform.localEulerAngles.y;
        }
        public override void Init(Transform character, Transform camera, string playerName, float _smoothTime)
        {
            base.Init(character, camera, playerName, _smoothTime);

            if (vrControllerGO) m_lastFrameY = vrControllerGO.transform.localEulerAngles.y;

        }

        public override void LookRotation(Transform character, Transform camera, bool invertPitch)
        {
            yRot = 0.0f;

            Vector3 rot = vrControllerGO.transform.localEulerAngles;
            yRot = Mathf.DeltaAngle(m_lastFrameY, rot.y);

            if (Mathf.Abs(yRot) < 0.8f)
            {
                return;
            }


            m_lastFrameY = rot.y;
            m_CharacterTargetRot *= Quaternion.Euler(0f, -yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(0f, 0f, 0f);


            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
            }


        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}

