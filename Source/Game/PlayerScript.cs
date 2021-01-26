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
        //public bool IsGrounded = true;

        private Vector3 movement;
        private Vector2 rotate;
        private CapsuleCollider collider;
        private MaterialInstance blueMat;
        private MaterialInstance redMat;

        public override void OnAwake()
        {
            base.OnAwake();
            collider = Actor.GetChild<CapsuleCollider>();

            blueMat = ColorMaterial.CreateVirtualInstance();
            blueMat.SetParameterValue("Color", Color.Blue);
            redMat = ColorMaterial.CreateVirtualInstance();
            redMat.SetParameterValue("Color", Color.Red);
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
            movement.Y = Input.GetAxis("Vertical");
            movement.Normalize();
            rotate.X = Input.GetAxis("Mouse X");
            rotate.Y = Input.GetAxis("Mouse Y");
        }

        public override void OnFixedUpdate()
        {
            if (IsGrounded)
            {
                Actor.Position += Actor.Transform.TransformDirection(movement) * Speed * Time.DeltaTime;
            }

            //var rotateQuaternion = Quaternion.Euler((Vector3)rotate);
            //Actor.Orientation *= rotateQuaternion;
        }

        private bool IsGrounded
        {
            get
            {
                Vector3 down = Transform.Down;
                var center = Actor.Position - down * collider.Radius;
                return Physics.SphereCast(center, collider.Radius * 1.05f, down);
            }
        }
    }
}
