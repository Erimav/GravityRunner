using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class PlayerScript : Script
    {
        public float Speed = 500;
        public float MouseSensivity = 1;
        public Material ColorMaterial;
        public Actor CameraActor;
        public float AirControl = .3f;
        public float Acceleration;
        public float AirAcceleration;
        public float JumpForce = 1000;
        //public bool IsGrounded = true;

        private float currentSpeed;
        private Vector3 movement;
        private Vector3 velocity;
        private Vector2 rotate;
        private Vector2 localRotationAngles;
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

        public bool CanJump => IsGrounded;

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
            movement.X = Input.GetAxis("Horizontal");
            movement.Z = Input.GetAxis("Vertical");
            movement.Normalize();
            rotate.X = Input.GetAxis("Mouse Y");
            rotate.Y = Input.GetAxis("Mouse X");
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
            var transformedMovement = Actor.Transform.TransformDirection(movement);
            if (IsGrounded)
            {
                velocity = Vector3.Lerp(velocity, transformedMovement * Speed, Acceleration * Time.DeltaTime);
                Actor.Position += velocity * Time.DeltaTime;
            }
            else
            {
                velocity = Vector3.Lerp(velocity, transformedMovement * Speed * AirControl, AirAcceleration * Time.DeltaTime);
                rigidBody.AddForce(velocity, ForceMode.Acceleration);
            }

            //Rotating
            localRotationAngles += rotate * MouseSensivity * Time.DeltaTime;
            localRotationAngles.X = Mathf.Clamp(localRotationAngles.X, -90, 90);
            Vector3 gravity = Physics.Gravity;
            Actor.Orientation = Quaternion.RotationAxis(-gravity, localRotationAngles.Y * Mathf.DegreesToRadians);
            CameraActor.LocalOrientation = Quaternion.Euler(localRotationAngles.X, 0, 0);
        }

        public void Jump()
        {
            if (!CanJump)
                return;

            var jumpVector = Transform.Up;
            rigidBody.AddForce(jumpVector * JumpForce + velocity, ForceMode.VelocityChange);
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
