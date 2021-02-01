using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlaxEngine;

namespace Game
{
    public class PlayerScript : Script
    {
        public float Speed = 500;
        public float MouseSensivity = 1;
        public float MouseSmoothness = 10;
        public float AirControl = .8f;
        public float Acceleration;
        public float AirAcceleration;
        public float JumpForce = 500;
        public int MaxAirJumps = 0;
        public float GravityRotationScale = 1;
        public float GravityRotationSmoothTime = 1;
        public Material ColorMaterial;
        public Actor CameraActor;
        public Vector2 TargetRotationAngles;

        private int airJumps;
        private Vector3 movementInput;
        private Vector3 velocity;
        private Vector2 rotateInput;
        private Vector2 currentRotationAngles;
        private float gravityClockwiseAngleDelta;
        private float gravityVerticalAngleDelta;
        private float gravityClockwiseChangeVelocity;
        private float gravityVerticalChangeVelocity;
        private uint playerLayerMask = ~(1U << 1);

        private CapsuleCollider collider;
        private RigidBody rigidBody;
        private MaterialInstance blueMat;
        private MaterialInstance redMat;

        public bool IsGrounded
        {
            get
            {
                Vector3 down = Transform.Down;
                var center = Actor.Position - down * collider.Radius;
                return Physics.SphereCast(center, collider.Radius * 0.9f, down, maxDistance: collider.Radius / 2, playerLayerMask, hitTriggers: false);
            }
        }

        public bool CanJump => IsGrounded || airJumps > 0;

        public override void OnAwake()
        {
            base.OnAwake();
            collider = Actor.GetChild<CapsuleCollider>();
            rigidBody = Actor.As<RigidBody>();

            blueMat = ColorMaterial.CreateVirtualInstance();
            blueMat.SetParameterValue("Color", Color.Blue);
            redMat = ColorMaterial.CreateVirtualInstance();
            redMat.SetParameterValue("Color", Color.Red);
            currentRotationAngles = TargetRotationAngles;

            Screen.CursorVisible = false;
            Screen.CursorLock = CursorLockMode.Locked;
        }

        public override void OnUpdate()
        {
            GetInputs();

            //debug
            Actor.GetChild<StaticModel>().SetMaterial(0, IsGrounded ? blueMat : redMat);
        }

        private void GetInputs()
        {
            movementInput.X = Input.GetAxis("Horizontal");
            movementInput.Z = Input.GetAxis("Vertical");
            movementInput.Normalize();
            rotateInput.X = Input.GetAxis("Mouse Y");
            rotateInput.Y = Input.GetAxis("Mouse X");
            gravityClockwiseAngleDelta = Mathf.SmoothDamp(gravityClockwiseAngleDelta,
                  Convert.ToSingle(Input.GetAction("Gravity clockwise"))
                - Convert.ToSingle(Input.GetAction("Gravity counterclockwise")),
                  ref gravityClockwiseChangeVelocity,
                  GravityRotationSmoothTime);
            gravityVerticalAngleDelta = Mathf.SmoothDamp(
                gravityVerticalAngleDelta,
                Input.GetAxis("Gravity vertical"),
                ref gravityVerticalChangeVelocity,
                GravityRotationSmoothTime);

            if (Input.GetAction("Jump"))
                Jump();

            if (Input.GetAction("Reset gravity"))
                SetGravityToGroundNormal();

            if (Input.GetKeyDown(KeyboardKeys.C))
            {
                Screen.CursorVisible = true;
                Screen.CursorLock = CursorLockMode.None;
            }
        }

        public override void OnFixedUpdate()
        {
            //Moving
            var transformedMovement = Actor.Transform.TransformDirection(movementInput);
            if (IsGrounded)
            {
                velocity = Vector3.Lerp(velocity, transformedMovement * Speed, Acceleration * Time.DeltaTime);
                Actor.Position += velocity * Time.DeltaTime;
                airJumps = MaxAirJumps;
            }
            else
            {
                velocity = Vector3.Lerp(velocity, transformedMovement * Speed * AirControl, AirAcceleration * Time.DeltaTime);
                rigidBody.AddForce(velocity, ForceMode.Acceleration);
            }

            //Rotating
            TargetRotationAngles += rotateInput * MouseSensivity * Time.DeltaTime;
            TargetRotationAngles.X = Mathf.Clamp(TargetRotationAngles.X, -90, 90);
            var newRotationAngles = Vector2.Lerp(currentRotationAngles, TargetRotationAngles, MouseSmoothness * Time.DeltaTime);
            var deltaRotationAngles = newRotationAngles - currentRotationAngles;
            float angleRad = deltaRotationAngles.Y * Mathf.DegreesToRadians;
            var direction = Vector3.Cross(Physics.Gravity, Actor.Transform.Right);//GravityActor.Transform.TransformDirection(localDirection);
            var gravityRotationDelta = Quaternion.RotationAxis(-Physics.Gravity, angleRad);
            var orientation = Quaternion.LookRotation(direction * gravityRotationDelta, -Physics.Gravity);

            currentRotationAngles = newRotationAngles;
            Actor.Orientation = orientation;
            CameraActor.LocalOrientation = Quaternion.Euler(newRotationAngles.X, 0, 0);

            //Rotating gravity
            var deltaRotation = Quaternion.RotationAxis(Actor.Transform.Forward, -gravityClockwiseAngleDelta * GravityRotationScale * Time.DeltaTime) //Clockwise
                              * Quaternion.RotationAxis(Actor.Transform.Right, -gravityVerticalAngleDelta * GravityRotationScale * Time.DeltaTime); //Vertical
            Physics.Gravity *= deltaRotation;
            //Physics.Gravity = GravityActor.Transform.Down * 981f;
        }

        public void Jump()
        {
            if (!CanJump)
                return;

            Vector3 jumpVector;
            if (IsGrounded)
            {
                var normal = GetGoundNormal();
                jumpVector = normal != Vector3.Zero
                    ? normal
                    : Actor.Transform.Up;
            }
            else
                jumpVector = Actor.Transform.Up;

            rigidBody.LinearVelocity = GetJumpVelocity(jumpVector);

            if (!IsGrounded)
                airJumps--;
        }

        public Vector3 GetJumpVelocity(Vector3 jumpVector) => GetJumpVelocity(jumpVector, this.velocity);
        public Vector3 GetJumpVelocity(Vector3 jumpVector, Vector3 velocity) => jumpVector * JumpForce + velocity;

        public void SetGravityToGroundNormal()
        {
            var normal = GetGoundNormal();
            if (normal == Vector3.Zero)
                return;

            var gravity = -normal * Global.g;

            Task.Run(() => SmoothGravity(gravity));
            Debug.Log($"Gravity's been set to {gravity}");
        }

        public async Task SmoothGravity(Vector3 gravity)
        {
            var t = 0f;
            var startGravity = Physics.Gravity;

            //pompensating camera angle
            var right = Actor.Transform.Right;
            var sgProj = Vector3.ProjectOnPlane(startGravity, right);
            var gProj = Vector3.ProjectOnPlane(gravity, right);
            //var angle = Vector3.Angle(sgProj, gProj);
            var angle = Vector3.Angle(startGravity, gravity);
            Debug.Log($"{startGravity} | {gravity}");
            Debug.Log($"angle: {angle}");
            var startPitch = TargetRotationAngles.X;
            var endPitch = startPitch + angle;

            do
            {
                await Scripting.RunOnUpdate(() =>
                {
                    t += Time.DeltaTime;
                    var bufferGravity = Physics.Gravity;
                    Physics.Gravity = Vector3.SmoothStep(startGravity, gravity, t / GravityRotationSmoothTime);
                    TargetRotationAngles.X = Mathf.SmoothStep(startPitch, endPitch, t / GravityRotationSmoothTime);
                    
                });
            } while (t < GravityRotationSmoothTime);
        }

        private Vector3 GetGoundNormal()
        {
            if (!Physics.RayCast(Actor.Position, Actor.Transform.Down, out var hit, 100f, layerMask: playerLayerMask, hitTriggers: false))
                return Vector3.Zero;
            
            return hit.Normal;
        }

        public void Reset()
        {
            currentRotationAngles = TargetRotationAngles;
            TargetRotationAngles.X = 0;
            velocity = default;
            rigidBody.LinearVelocity = default;
            gravityClockwiseChangeVelocity = 0;
            gravityVerticalChangeVelocity = 0;
            gravityClockwiseAngleDelta = 0f;
            gravityVerticalAngleDelta = 0f;
        }

        //public override void OnDebugDraw()
        //{
        //    base.OnDebugDraw();
        //    Vector3 down = Transform.Down;
        //    var center = Actor.Position - down * collider.Radius;
        //    DebugDraw.DrawWireSphere(new BoundingSphere(center, collider.Radius * 1.05f), Color.Magenta);
        //    DebugDraw.DrawSphere(new BoundingSphere(center, 0.1f), Color.Magenta);
        //}
    }
}
