﻿using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;

namespace Game
{
    public class JumpTrajectoryPreview : Script
    {
        private float maxLength = 100;

        public PlayerScript Player;
        public float MaxLength
        {
            get => maxLength;
            set
            {
                maxLength = Mathf.Max(value, 0f);

                InitPointsArray();
            }
        }

        [Range(-1, 1)]
        public float GravityClockwiseDelta = 0f;

        public StartOption Start;
        public Vector3 StartVelocity;

        [HideInEditor]
        public Transform SimulatedTransform;

        private const float DeltaTime = 0.02f;

        private bool initialized = false;
        private Vector3[] points;
        private Vector3 velocity;

        public override void OnDebugDraw()
        {
            if (!initialized)
                Initialize();

            if (Player is null)
                return;

            var gravity = Actor.Transform.Down * 981;

            switch (Start)
            {
                case StartOption.Jump:
                    velocity = Player.GetJumpVelocity(Actor.Transform.Up, Actor.Transform.Forward * Player.Speed);
                    break;
                case StartOption.StartVelocity:
                    velocity = StartVelocity;
                    break;
                default: 
                    velocity = default;
                    break;
            }
            
            points[0] = Actor.Position;
            SimulatedTransform = Actor.Transform;

            for(int i = 1; i < points.Length; i++)
            {
                points[i] = points[i - 1] + velocity * DeltaTime;
                velocity += gravity * DeltaTime;
                //velocity += SimulatedTransform.Forward * Player.Speed * Player.AirControl * DeltaTime;                                      
                var deltaRotation = Quaternion.RotationAxis(SimulatedTransform.Forward, GravityClockwiseDelta * Player.GravityRotationScale * DeltaTime);
                gravity *= deltaRotation;
                SimulatedTransform.Orientation *= deltaRotation;
            }
            
            DebugDraw.DrawLines(points, Scene.Transform.GetWorld(), Color.Purple);
        }

        private void Initialize()
        {
            initialized = true;
            Player = Player ?? Scene.FindScript(typeof(PlayerScript)) as PlayerScript;
            InitPointsArray();
        }

        private void InitPointsArray()
        {
            int lenght = (int)(MaxLength / 0.2f);
            if (lenght % 2 != 0)
                lenght++;
            points = new Vector3[lenght];
        }

        public void CreateNewOnTheEdge()
        {
            var newActor = new EmptyActor { Name = "Trajectory", Parent = Actor, Transform = SimulatedTransform };
            var traj = newActor.AddScript<JumpTrajectoryPreview>();
            traj.StartVelocity = velocity;
            newActor.Position = points.LastOrDefault();
        }

        public enum StartOption
        {
            Jump,
            StartVelocity
        }
    }
}
