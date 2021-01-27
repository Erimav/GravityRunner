using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class PlayerScript : Script
    {
        public float Speed = 500;
        public float MouseSensivity = 1;
        public float AirControl = .3f;
        public float Acceleration;
        public float AirAcceleration;
        public float JumpForce = 1000;
        public int MaxAirJumps = 0;
        public float GravityRotationScale = 1;
        public Material ColorMaterial;
        public Actor CameraActor;
        public Actor GravityActor;
        public Vector2 LocalRotationAngles;

        private int airJumps;
        private Vector3 movementInput;
        private Vector3 velocity;
        private Vector2 rotateInput;
        private float gravityClockwiseAngleDelta;
        private float gravityVerticalAngleDelta;

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
                var layerMask = ~(1U << 1);
                return Physics.SphereCast(center, collider.Radius * 0.95f, down, maxDistance: collider.Radius / 2, layerMask);
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
            gravityClockwiseAngleDelta
                = Convert.ToSingle(Input.GetAction("Gravity clockwise"))
                - Convert.ToSingle(Input.GetAction("Gravity counterclockwise"));
            gravityVerticalAngleDelta = Input.GetAxis("Gravity vertical");

            if (Input.GetAction("Jump"))
                Jump();

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
            LocalRotationAngles += rotateInput * MouseSensivity * Time.DeltaTime;
            LocalRotationAngles.X = Mathf.Clamp(LocalRotationAngles.X, -90, 90);
            var angleRad = -LocalRotationAngles.Y * Mathf.DegreesToRadians;
            var localDirection = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            var direction = GravityActor.Transform.TransformDirection(localDirection);
            var orientation = Quaternion.LookRotation(direction, -Physics.Gravity);
            Actor.Orientation = orientation;
            CameraActor.LocalOrientation = Quaternion.Euler(LocalRotationAngles.X, 0, 0);

            //Rotating gravity

            var deltaRotation = Quaternion.RotationAxis(Actor.Transform.Forward, -gravityClockwiseAngleDelta * GravityRotationScale * Time.DeltaTime) //Clockwise
                              * Quaternion.RotationAxis(Actor.Transform.Right, -gravityVerticalAngleDelta * GravityRotationScale * Time.DeltaTime); //Vertical
            GravityActor.Orientation *= deltaRotation;
            Physics.Gravity = GravityActor.Transform.Down * 981f;
        }

        public void Jump()
        {
            if (!CanJump)
                return;

            var jumpVector = Actor.Transform.Up;

            if (IsGrounded)
                rigidBody.AddForce(jumpVector * JumpForce + velocity, ForceMode.VelocityChange);
            else
            {
                rigidBody.LinearVelocity = jumpVector * JumpForce + velocity;
                airJumps--;
            }
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
