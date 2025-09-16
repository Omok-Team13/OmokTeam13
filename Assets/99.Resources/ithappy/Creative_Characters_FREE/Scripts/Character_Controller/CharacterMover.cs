using System;
using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class CharacterMover : MonoBehaviour
    {
        [Header("Movement")]
        private float m_WalkSpeed = 9f;   
        private float m_RunSpeed = 14f;   
        private float m_RotateSpeed = 360f;
        private Space m_Space = Space.Self;
        private float m_JumpHeight = 3.2f;

        [Header("Input")]
        [SerializeField] private bool useArrowKeysOnly = true; // true면 화살표 키로만 이동 처리

        [Header("Animator")]
        [SerializeField] private string m_HorizontalID = "Hor";
        [SerializeField] private string m_VerticalID = "Vert";
        [SerializeField] private string m_StateID = "State";
        [SerializeField] private string m_JumpID = "IsJump";
        [SerializeField] private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

        private Transform m_Transform;
        private CharacterController m_Controller;
        private Animator m_Animator;

        private MovementHandler m_Movement;
        private AnimationHandler m_Animation;

        private Vector2 m_Axis;
        private Vector3 m_Target;
        private bool m_IsRun;
        private bool m_IsJump;

        private bool m_IsMoving;

        public Vector2 Axis => m_Axis;
        public Vector3 Target => m_Target;
        public bool IsRun => m_IsRun;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);

            // 기존 코드 의도대로 Inspector의 km/h 값을 m/s로 변환해 핸들러에 적용
            m_Movement?.SetStats(m_WalkSpeed / 3.6f, m_RunSpeed / 3.6f, m_RotateSpeed, m_JumpHeight, m_Space);
        }

        private void Awake()
        {
            m_Transform = transform;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();

            // Awake에서도 km/h -> m/s 변환하여 MovementHandler 생성
            float walk_mps = m_WalkSpeed / 3.6f;
            float run_mps = m_RunSpeed / 3.6f;
            m_Movement = new MovementHandler(m_Controller, m_Transform, walk_mps, run_mps, m_RotateSpeed, m_JumpHeight, m_Space);
            m_Animation = new AnimationHandler(m_Animator, m_HorizontalID, m_VerticalID, m_StateID, m_JumpID);

            // 기본 target은 앞쪽으로 설정 (필요시 외부에서 덮어쓰기 가능)
            m_Target = m_Transform.position + m_Transform.forward * 2f;
        }

        private void Update()
        {
            // 옵션: 화살표 키만 사용해서 내부에서 입력 처리
            if (useArrowKeysOnly)
            {
                HandleArrowKeyInput();
            }

            m_Movement.Move(Time.deltaTime, in m_Axis, in m_Target, m_IsRun, m_IsJump, m_IsMoving, out var animAxis, out var isAir);
            m_Animation.Animate(in animAxis, m_IsRun ? 1f : 0f, isAir, Time.deltaTime);

            // Reset one-frame jump flag (SetInput uses GetKeyDown style, here we consumed it)
            m_IsJump = false;
        }

        private void OnAnimatorIK()
        {
            m_Animation.AnimateIK(in m_Target, m_LookWeight);
        }

        /// <summary>
        /// 외부에서 입력을 줄 때 사용. (기존 API 유지)
        /// </summary>
        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
        {
            m_Axis = axis;
            m_Target = target;
            m_IsRun = isRun;
            m_IsJump = isJump;

            if (m_Axis.sqrMagnitude < Mathf.Epsilon)
            {
                m_Axis = Vector2.zero;
                m_IsMoving = false;
            }
            else
            {
                m_Axis = Vector3.ClampMagnitude(m_Axis, 1f);
                m_IsMoving = true;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > m_Controller.stepOffset)
            {
                m_Movement.SetSurface(hit.normal);
            }
        }

        #region Arrow Key Input
        // 화살표 키 입력을 받아 내부 상태를 갱신
        private void HandleArrowKeyInput()
        {
            float h = 0f;
            float v = 0f;

            if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) h = 1f;

            if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
            else if (Input.GetKey(KeyCode.DownArrow)) v = -1f;

            Vector2 axis = new Vector2(h, v);

            bool isRun = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool isJump = Input.GetKeyDown(KeyCode.Space);

            // Update internal states (동일한 방식으로 SetInput과 유사하게 동작)
            if (axis.sqrMagnitude < Mathf.Epsilon)
            {
                m_Axis = Vector2.zero;
                m_IsMoving = false;
            }
            else
            {
                m_Axis = Vector2.ClampMagnitude(axis, 1f);
                m_IsMoving = true;
            }

            // 간단하게 target을 입력 방향 기준으로 설정 (필요하면 카메라 기준 등으로 변경)
            if (m_Axis.sqrMagnitude > Mathf.Epsilon)
            {
                // target은 현재 위치에서 입력 방향으로 1유닛 떨어진 점
                m_Target = m_Transform.position + new Vector3(m_Axis.x, 0f, m_Axis.y);
            }

            m_IsRun = isRun;
            m_IsJump = isJump;
        }
        #endregion

        [Serializable]
        private struct LookWeight
        {
            public float weight;
            public float body;
            public float head;
            public float eyes;

            public LookWeight(float weight, float body, float head, float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }

        #region Handlers
        private class MovementHandler
        {
            private readonly CharacterController m_Controller;
            private readonly Transform m_Transform;

            private float m_WalkSpeed;   // m/s
            private float m_RunSpeed;    // m/s
            private float m_RotateSpeed; // degrees/sec
            private float m_JumpHeight;

            private Space m_Space;

            private readonly float m_Luft = 75f;
            private readonly float m_JumpReload = 1f;

            private float m_TargetAngle;
            private bool m_IsRotating = false;

            private Vector3 m_Normal;
            private Vector3 m_GravityAcelleration = Physics.gravity;

            private float m_jumpTimer;

            public MovementHandler(CharacterController controller, Transform transform, float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
            {
                m_Controller = controller;
                m_Transform = transform;

                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;
                m_JumpHeight = jumpHeight;

                m_Space = space;
            }

            public void SetStats(float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
            {
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;
                m_JumpHeight = jumpHeight;

                m_Space = space;
            }

            public void SetSurface(in Vector3 normal)
            {
                m_Normal = normal;
            }

            public void Move(float deltaTime, in Vector2 axis, in Vector3 target, bool isRun, bool isJump, bool isMoving, out Vector2 animAxis, out bool isAir)
            {
                // 1. 입력 방향 벡터 구하기
                Vector3 moveDir = new Vector3(axis.x, 0f, axis.y);

                // 2. 입력이 있으면 캐릭터를 그 방향으로 회전
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                    // rotate speed는 외부에서 세팅 가능하므로 덮어쓰지 않음
                    m_Transform.rotation = Quaternion.RotateTowards(
                        m_Transform.rotation,
                        targetRotation,
                        m_RotateSpeed * deltaTime
                    );
                }

                // 3. 실제 이동
                Vector3 norm = moveDir.normalized;
                Displace(deltaTime, in norm, isRun);

                // 4. 중력 처리
                CaculateGravity(isJump, deltaTime, out isAir);

                // 5. 애니메이션용 axis
                GenAnimationAxis(in moveDir, out animAxis);
            }

            private void ConvertMovement(in Vector2 axis, in Vector3 targetForward, out Vector3 movement)
            {
                Vector3 forward;
                Vector3 right;

                if (m_Space == Space.Self)
                {
                    forward = new Vector3(targetForward.x, 0f, targetForward.z).normalized;
                    right = Vector3.Cross(Vector3.up, forward).normalized;
                }
                else
                {
                    forward = Vector3.forward;
                    right = Vector3.right;
                }

                movement = axis.x * right + axis.y * forward;
                movement = Vector3.ProjectOnPlane(movement, m_Normal);
            }

            private void Displace(float deltaTime, in Vector3 movement, bool isRun)
            {
                Vector3 displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * movement;

                // 입력이 없으면 이동 벡터를 빨리 줄여서 관성 감소
                if (movement.sqrMagnitude < 0.001f)
                    displacement = Vector3.zero;

                displacement += m_GravityAcelleration;
                displacement *= deltaTime;

                m_Controller.Move(displacement);
            }

            private void CaculateGravity(bool isJump, float deltaTime, out bool isAir)
            {
                m_jumpTimer = Mathf.Max(m_jumpTimer - deltaTime, 0f);

                if (m_Controller.isGrounded)
                {
                    if (isJump && m_jumpTimer <= 0)
                    {
                        var gravity = Physics.gravity;
                        var length = gravity.magnitude;

                        m_GravityAcelleration += -(gravity / length) * Mathf.Sqrt(m_JumpHeight * 6f * length);
                        m_jumpTimer = m_JumpReload;
                        isAir = true;

                        return;
                    }

                    m_GravityAcelleration = Physics.gravity;
                    isAir = false;

                    return;
                }

                isAir = true;

                m_GravityAcelleration += Physics.gravity * deltaTime;
                return;
            }

            private void GenAnimationAxis(in Vector3 movement, out Vector2 animAxis)
            {
                if (m_Space == Space.Self)
                {
                    animAxis = new Vector2(Vector3.Dot(movement, m_Transform.right), Vector3.Dot(movement, m_Transform.forward));
                }
                else
                {
                    animAxis = new Vector2(Vector3.Dot(movement, Vector3.right), Vector3.Dot(movement, Vector3.forward));
                }
            }
        }

        private class AnimationHandler
        {
            private readonly Animator m_Animator;

            private readonly string m_HorizontalID;
            private readonly string m_VerticalID;
            private readonly string m_StateID;
            private readonly string m_JumpID;

            private readonly float k_InputFlow = 4.5f;

            private float m_FlowState;
            private Vector2 m_FlowAxis;

            public AnimationHandler(Animator animator, string horizontalID, string verticalID, string stateID, string jumpID)
            {
                m_Animator = animator;

                m_HorizontalID = horizontalID;
                m_VerticalID = verticalID;
                m_StateID = stateID;
                m_JumpID = jumpID;
            }

            public void Animate(in Vector2 axis, float state, bool isJump, float deltaTime)
            {
                m_Animator.SetFloat(m_HorizontalID, m_FlowAxis.x);
                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.y);

                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));
                m_Animator.SetBool(m_JumpID, isJump);

                // 관성 줄이기 → Lerp로 좀 더 빠르게 반응
                float smooth = k_InputFlow * 4f; // 기존보다 2배 빠르게
                m_FlowAxis = Vector2.Lerp(m_FlowAxis, axis, smooth * deltaTime);
                m_FlowState = Mathf.Lerp(m_FlowState, state, smooth * deltaTime);
            }

            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
            {
                m_Animator.SetLookAtPosition(target);
                m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
            }
        }
        #endregion
    }
}

//using System;
//using UnityEngine;

//namespace Controller
//{
//    [RequireComponent(typeof(CharacterController))]
//    [RequireComponent(typeof(Animator))]
//    [DisallowMultipleComponent]
//    public class CharacterMover : MonoBehaviour
//    {
//        [Header("Movement")]
//        [SerializeField]
//        private float m_WalkSpeed = 1f;
//        [SerializeField]
//        private float m_RunSpeed = 4f;
//        [SerializeField, Range(0f, 360f)]
//        private float m_RotateSpeed = 90f;
//        [SerializeField]
//        private Space m_Space = Space.Self;
//        [SerializeField]
//        private float m_JumpHeight = 5f;

//        [Header("Animator")]
//        [SerializeField]
//        private string m_HorizontalID = "Hor";
//        [SerializeField]
//        private string m_VerticalID = "Vert";
//        [SerializeField]
//        private string m_StateID = "State";
//        [SerializeField]
//        private string m_JumpID = "IsJump";
//        [SerializeField]
//        private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

//        private Transform m_Transform;
//        private CharacterController m_Controller;
//        private Animator m_Animator;

//        private MovementHandler m_Movement;
//        private AnimationHandler m_Animation;

//        private Vector2 m_Axis;
//        private Vector3 m_Target;
//        private bool m_IsRun;
//        private bool m_IsJump;

//        private bool m_IsMoving;

//        public Vector2 Axis => m_Axis;
//        public Vector3 Target => m_Target;
//        public bool IsRun => m_IsRun;

//        private void OnValidate()
//        {
//            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
//            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);

//            m_Movement?.SetStats(m_WalkSpeed / 3.6f, m_RunSpeed / 3.6f, m_RotateSpeed, m_JumpHeight, m_Space);
//        }

//        private void Awake()
//        {
//            m_Transform = transform;
//            m_Controller = GetComponent<CharacterController>();
//            m_Animator = GetComponent<Animator>();

//            m_Movement = new MovementHandler(m_Controller, m_Transform, m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_Space);
//            m_Animation = new AnimationHandler(m_Animator, m_HorizontalID,  m_VerticalID, m_StateID, m_JumpID);
//        }

//        private void Update()
//        {
//            m_Movement.Move(Time.deltaTime, in m_Axis, in m_Target, m_IsRun, m_IsJump, m_IsMoving, out var animAxis, out var isAir);
//            m_Animation.Animate(in animAxis, m_IsRun? 1f : 0f, isAir, Time.deltaTime);

//        }

//        private void OnAnimatorIK()
//        {
//            m_Animation.AnimateIK(in m_Target, m_LookWeight);
//        }

//        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
//        {
//            m_Axis = axis;
//            m_Target = target;
//            m_IsRun = isRun;
//            m_IsJump = isJump;

//            if (m_Axis.sqrMagnitude < Mathf.Epsilon)
//            {
//                m_Axis = Vector2.zero;
//                m_IsMoving = false;
//            }
//            else
//            {
//                m_Axis = Vector3.ClampMagnitude(m_Axis, 1f);
//                m_IsMoving = true;
//            }
//        }

//        private void OnControllerColliderHit(ControllerColliderHit hit)
//        {
//            if(hit.normal.y > m_Controller.stepOffset)
//            {
//                m_Movement.SetSurface(hit.normal);
//            }
//        }

//        [Serializable]
//        private struct LookWeight
//        {
//            public float weight;
//            public float body;
//            public float head;
//            public float eyes;

//            public LookWeight(float weight, float body, float head, float eyes)
//            {
//                this.weight = weight;
//                this.body = body;
//                this.head = head;
//                this.eyes = eyes;
//            }
//        }

//        #region Handlers
//        private class MovementHandler
//        {
//            private readonly CharacterController m_Controller;
//            private readonly Transform m_Transform;

//            private float m_WalkSpeed;
//            private float m_RunSpeed;
//            private float m_RotateSpeed;
//            private float m_JumpHeight;

//            private Space m_Space;

//            private readonly float m_Luft = 75f;
//            private readonly float m_JumpReload = 1f;

//            private float m_TargetAngle;
//            private bool m_IsRotating = false;

//            private Vector3 m_Normal;
//            private Vector3 m_GravityAcelleration = Physics.gravity;

//            private float m_jumpTimer;

//            public MovementHandler(CharacterController controller, Transform transform, float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
//            {
//                m_Controller = controller;
//                m_Transform = transform;

//                m_WalkSpeed = walkSpeed;
//                m_RunSpeed = runSpeed;
//                m_RotateSpeed = rotateSpeed;
//                m_JumpHeight = jumpHeight;

//                m_Space = space;
//            }

//            public void SetStats(float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
//            {
//                m_WalkSpeed = walkSpeed;
//                m_RunSpeed = runSpeed;
//                m_RotateSpeed = rotateSpeed;
//                m_JumpHeight = jumpHeight;

//                m_Space = space;
//            }

//            public void SetSurface(in Vector3 normal)
//            {
//                m_Normal = normal;
//            }

//            public void Move(float deltaTime, in Vector2 axis, in Vector3 target, bool isRun, bool isJump, bool isMoving, out Vector2 animAxis, out bool isAir)
//            {
//                var targetForward = Vector3.Normalize(target - m_Transform.position);

//                ConvertMovement(in axis, in targetForward, out var movement);
//                CaculateGravity(isJump, deltaTime, out isAir);
//                Displace(deltaTime, in movement, isRun);
//                Turn(in targetForward, isMoving);
//                UpdateRotation(deltaTime);

//                GenAnimationAxis(in movement, out animAxis);
//            }

//            private void ConvertMovement(in Vector2 axis, in Vector3 targetForward, out Vector3 movement)
//            {
//                Vector3 forward;
//                Vector3 right;

//                if (m_Space == Space.Self)
//                {
//                    forward = new Vector3(targetForward.x, 0f, targetForward.z).normalized;
//                    right = Vector3.Cross(Vector3.up, forward).normalized;
//                }
//                else
//                {
//                    forward = Vector3.forward;
//                    right = Vector3.right;
//                }

//                movement = axis.x * right + axis.y * forward;
//                movement = Vector3.ProjectOnPlane(movement, m_Normal);
//            }

//            private void Displace(float deltaTime, in Vector3 movement, bool isRun)
//            {
//                Vector3 displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * movement;
//                displacement += m_GravityAcelleration;
//                displacement *= deltaTime;

//                m_Controller.Move(displacement);
//            }

//            private void CaculateGravity(bool isJump, float deltaTime, out bool isAir)
//            {
//                m_jumpTimer = Mathf.Max(m_jumpTimer - deltaTime, 0f);

//                if (m_Controller.isGrounded)
//                {
//                    if (isJump && m_jumpTimer <= 0)
//                    {
//                        var gravity = Physics.gravity;
//                        var length = gravity.magnitude;

//                        m_GravityAcelleration += -(gravity / length) * Mathf.Sqrt(m_JumpHeight * 6f * length);
//                        m_jumpTimer = m_JumpReload;
//                        isAir = true;

//                        return;
//                    }

//                    m_GravityAcelleration = Physics.gravity;
//                    isAir = false;

//                    return;
//                }

//                isAir = true;

//                m_GravityAcelleration += Physics.gravity * deltaTime;
//                return;
//            }

//            private void GenAnimationAxis(in Vector3 movement, out Vector2 animAxis)
//            {
//                if(m_Space == Space.Self)
//                {
//                    animAxis = new Vector2(Vector3.Dot(movement, m_Transform.right), Vector3.Dot(movement, m_Transform.forward));
//                }
//                else
//                {
//                    animAxis = new Vector2(Vector3.Dot(movement, Vector3.right), Vector3.Dot(movement, Vector3.forward));
//                }
//            }

//            private void Turn(in Vector3 targetForward, bool isMoving)
//            {
//                var angle = Vector3.SignedAngle(m_Transform.forward, Vector3.ProjectOnPlane(targetForward, Vector3.up), Vector3.up);

//                if (!m_IsRotating)
//                {
//                    if (!isMoving && Mathf.Abs(angle) < m_Luft)
//                    {
//                        m_IsRotating = false;
//                        return;
//                    }

//                    m_IsRotating = true;
//                }

//                m_TargetAngle = angle;
//            }

//            private void UpdateRotation(float deltaTime)
//            {
//                if(!m_IsRotating)
//                {
//                    return;
//                }

//                var rotDelta = m_RotateSpeed * deltaTime;
//                if (rotDelta + Mathf.PI * 2f + Mathf.Epsilon >= Mathf.Abs(m_TargetAngle))
//                {
//                    rotDelta = m_TargetAngle;
//                    m_IsRotating = false;
//                }
//                else
//                {
//                    rotDelta *= Mathf.Sign(m_TargetAngle);
//                }

//                m_Transform.Rotate(Vector3.up, rotDelta);
//            }
//        }

//        private class AnimationHandler
//        {
//            private readonly Animator m_Animator;

//            private readonly string m_HorizontalID;
//            private readonly string m_VerticalID;
//            private readonly string m_StateID;
//            private readonly string m_JumpID;

//            private readonly float k_InputFlow = 4.5f;

//            private float m_FlowState;
//            private Vector2 m_FlowAxis;

//            public AnimationHandler(Animator animator, string horizontalID, string verticalID, string stateID, string jumpID)
//            {
//                m_Animator = animator;

//                m_HorizontalID = horizontalID;
//                m_VerticalID = verticalID;
//                m_StateID = stateID;
//                m_JumpID = jumpID;
//            }

//            public void Animate(in Vector2 axis, float state, bool isJump, float deltaTime)
//            {

//                m_Animator.SetFloat(m_HorizontalID, m_FlowAxis.x);
//                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.y);

//                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));
//                m_Animator.SetBool(m_JumpID, isJump);

//                m_FlowAxis = Vector2.ClampMagnitude(m_FlowAxis + k_InputFlow * deltaTime * (axis - m_FlowAxis).normalized, 1f);
//                m_FlowState = Mathf.Clamp01(m_FlowState + k_InputFlow * deltaTime * Mathf.Sign(state - m_FlowState));
//            }

//            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
//            {
//                m_Animator.SetLookAtPosition(target);
//                m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
//            }
//        }
//        #endregion
//    }
//}

//using System;
//using System.Collections;
//using UnityEngine;

//namespace Controller
//{
//    [RequireComponent(typeof(CharacterController))]
//    [RequireComponent(typeof(Animator))]
//    [DisallowMultipleComponent]
//    public class CharacterMover : MonoBehaviour
//    {
//        [Header("Movement")]
//        private float m_WalkSpeed = 2.5f;
//        private float m_RunSpeed = 3.5f;
//        private float m_RotateSpeed = 360f;
//        private Space m_Space = Space.Self;
//        private float m_JumpHeight = 3.2f;

//        [Header("Animator")]
//        [SerializeField]
//        private string m_HorizontalID = "Hor";
//        [SerializeField]
//        private string m_VerticalID = "Vert";
//        [SerializeField]
//        private string m_StateID = "State";
//        [SerializeField]
//        private string m_JumpID = "IsJump";
//        [SerializeField]
//        private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

//        private Transform m_Transform;
//        private CharacterController m_Controller;
//        private Animator m_Animator;

//        private MovementHandler m_Movement;
//        private AnimationHandler m_Animation;

//        private Vector2 m_Axis;
//        private Vector3 m_Target;
//        private bool m_IsRun;
//        private bool m_IsJump;

//        private bool m_IsMoving;

//        public Vector2 Axis => m_Axis;
//        public Vector3 Target => m_Target;
//        public bool IsRun => m_IsRun;

//        private void OnValidate()
//        {
//            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
//            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);

//            m_Movement?.SetStats(m_WalkSpeed / 3.6f, m_RunSpeed / 3.6f, m_RotateSpeed, m_JumpHeight, m_Space);
//        }

//        private void Awake()
//        {
//            m_Transform = transform;
//            m_Controller = GetComponent<CharacterController>();
//            m_Animator = GetComponent<Animator>();

//            m_Movement = new MovementHandler(m_Controller, m_Transform, m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_Space);
//            m_Animation = new AnimationHandler(m_Animator, m_HorizontalID, m_VerticalID, m_StateID, m_JumpID);
//        }

//        private void Update()
//        {
//            m_Movement.Move(Time.deltaTime, in m_Axis, in m_Target, m_IsRun, m_IsJump, m_IsMoving, out var animAxis, out var isAir);
//            m_Animation.Animate(in animAxis, m_IsRun ? 1f : 0f, isAir, Time.deltaTime);

//        }

//        private void OnAnimatorIK()
//        {
//            m_Animation.AnimateIK(in m_Target, m_LookWeight);
//        }

//        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
//        {
//            m_Axis = axis;
//            m_Target = target;
//            m_IsRun = isRun;
//            m_IsJump = isJump;

//            if (m_Axis.sqrMagnitude < Mathf.Epsilon)
//            {
//                m_Axis = Vector2.zero;
//                m_IsMoving = false;
//            }
//            else
//            {
//                m_Axis = Vector3.ClampMagnitude(m_Axis, 1f);
//                m_IsMoving = true;
//            }
//        }

//        private void OnControllerColliderHit(ControllerColliderHit hit)
//        {
//            if (hit.normal.y > m_Controller.stepOffset)
//            {
//                m_Movement.SetSurface(hit.normal);
//            }
//        }

//        [Serializable]
//        private struct LookWeight
//        {
//            public float weight;
//            public float body;
//            public float head;
//            public float eyes;

//            public LookWeight(float weight, float body, float head, float eyes)
//            {
//                this.weight = weight;
//                this.body = body;
//                this.head = head;
//                this.eyes = eyes;
//            }
//        }

//        #region Handlers
//        private class MovementHandler
//        {
//            private readonly CharacterController m_Controller;
//            private readonly Transform m_Transform;

//            private float m_WalkSpeed;
//            private float m_RunSpeed;
//            private float m_RotateSpeed;
//            private float m_JumpHeight;

//            private Space m_Space;

//            private readonly float m_Luft = 75f;
//            private readonly float m_JumpReload = 1f;

//            private float m_TargetAngle;
//            private bool m_IsRotating = false;

//            private Vector3 m_Normal;
//            private Vector3 m_GravityAcelleration = Physics.gravity;

//            private float m_jumpTimer;

//            public MovementHandler(CharacterController controller, Transform transform, float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
//            {
//                m_Controller = controller;
//                m_Transform = transform;

//                m_WalkSpeed = walkSpeed;
//                m_RunSpeed = runSpeed;
//                m_RotateSpeed = rotateSpeed;
//                m_JumpHeight = jumpHeight;

//                m_Space = space;
//            }

//            public void SetStats(float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
//            {
//                m_WalkSpeed = walkSpeed;
//                m_RunSpeed = runSpeed;
//                m_RotateSpeed = rotateSpeed;
//                m_JumpHeight = jumpHeight;

//                m_Space = space;
//            }

//            public void SetSurface(in Vector3 normal)
//            {
//                m_Normal = normal;
//            }

//            public void Move(float deltaTime, in Vector2 axis, in Vector3 target, bool isRun, bool isJump, bool isMoving, out Vector2 animAxis, out bool isAir)
//            {
//                // 1. 입력 방향 벡터 구하기
//                Vector3 moveDir = new Vector3(axis.x, 0f, axis.y);

//                // 2. 입력이 있으면 캐릭터를 그 방향으로 회전
//                if (moveDir.sqrMagnitude > 0.001f)
//                {
//                    Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
//                    m_RotateSpeed = 720f;
//                    m_Transform.rotation = Quaternion.RotateTowards(
//                        m_Transform.rotation,
//                        targetRotation,
//                        m_RotateSpeed * deltaTime
//                    );
//                }

//                // 3. 실제 이동
//                Vector3 norm = moveDir.normalized;
//                Displace(deltaTime, in norm, isRun);


//                // 4. 중력 처리
//                CaculateGravity(isJump, deltaTime, out isAir);

//                // 5. 애니메이션용 axis
//                GenAnimationAxis(in moveDir, out animAxis);
//            }


//            private void ConvertMovement(in Vector2 axis, in Vector3 targetForward, out Vector3 movement)
//            {
//                Vector3 forward;
//                Vector3 right;

//                if (m_Space == Space.Self)
//                {
//                    forward = new Vector3(targetForward.x, 0f, targetForward.z).normalized;
//                    right = Vector3.Cross(Vector3.up, forward).normalized;
//                }
//                else
//                {
//                    forward = Vector3.forward;
//                    right = Vector3.right;
//                }

//                movement = axis.x * right + axis.y * forward;
//                movement = Vector3.ProjectOnPlane(movement, m_Normal);
//            }

//            private void Displace(float deltaTime, in Vector3 movement, bool isRun)
//            {
//                Vector3 displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * movement;

//                // 입력이 없으면 이동 벡터를 빨리 줄여서 관성 감소
//                if (movement.sqrMagnitude < 0.001f)
//                    displacement = Vector3.zero;

//                displacement += m_GravityAcelleration;
//                displacement *= deltaTime;

//                m_Controller.Move(displacement);
//            }


//            private void CaculateGravity(bool isJump, float deltaTime, out bool isAir)
//            {
//                m_jumpTimer = Mathf.Max(m_jumpTimer - deltaTime, 0f);

//                if (m_Controller.isGrounded)
//                {
//                    if (isJump && m_jumpTimer <= 0)
//                    {
//                        var gravity = Physics.gravity;
//                        var length = gravity.magnitude;

//                        m_GravityAcelleration += -(gravity / length) * Mathf.Sqrt(m_JumpHeight * 6f * length);
//                        m_jumpTimer = m_JumpReload;
//                        isAir = true;

//                        return;
//                    }

//                    m_GravityAcelleration = Physics.gravity;
//                    isAir = false;

//                    return;
//                }

//                isAir = true;

//                m_GravityAcelleration += Physics.gravity * deltaTime;
//                return;
//            }

//            private void GenAnimationAxis(in Vector3 movement, out Vector2 animAxis)
//            {
//                if (m_Space == Space.Self)
//                {
//                    animAxis = new Vector2(Vector3.Dot(movement, m_Transform.right), Vector3.Dot(movement, m_Transform.forward));
//                }
//                else
//                {
//                    animAxis = new Vector2(Vector3.Dot(movement, Vector3.right), Vector3.Dot(movement, Vector3.forward));
//                }
//            }
//        }

//        private class AnimationHandler
//        {
//            private readonly Animator m_Animator;

//            private readonly string m_HorizontalID;
//            private readonly string m_VerticalID;
//            private readonly string m_StateID;
//            private readonly string m_JumpID;

//            private readonly float k_InputFlow = 4.5f;

//            private float m_FlowState;
//            private Vector2 m_FlowAxis;

//            public AnimationHandler(Animator animator, string horizontalID, string verticalID, string stateID, string jumpID)
//            {
//                m_Animator = animator;

//                m_HorizontalID = horizontalID;
//                m_VerticalID = verticalID;
//                m_StateID = stateID;
//                m_JumpID = jumpID;
//            }

//            public void Animate(in Vector2 axis, float state, bool isJump, float deltaTime)
//            {
//                m_Animator.SetFloat(m_HorizontalID, m_FlowAxis.x);
//                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.y);

//                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));
//                m_Animator.SetBool(m_JumpID, isJump);

//                // 관성 줄이기 → Lerp로 좀 더 빠르게 반응
//                float smooth = k_InputFlow * 4f; // 기존보다 2배 빠르게
//                m_FlowAxis = Vector2.Lerp(m_FlowAxis, axis, smooth * deltaTime);
//                m_FlowState = Mathf.Lerp(m_FlowState, state, smooth * deltaTime);
//            }


//            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
//            {
//                m_Animator.SetLookAtPosition(target);
//                m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
//            }
//        }
//        #endregion
//    }
//}
