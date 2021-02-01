using System;
using System.Collections.Generic;
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

        private const float DeltaTime = 0.02f;

        private bool initialized = false;
        private Vector3[] points;

        public override void OnDebugDraw()
        {
            if (!initialized)
                Initialize();

            if (Player is null)
                return;

            var gravity = Actor.Transform.Down * 981;
            var velocity = Player.GetJumpVelocity(Actor.Transform.Up, Actor.Transform.Forward * Player.Speed);
            points[0] = Actor.Position;

            for(int i = 1; i < points.Length; i++)
            {
                points[i] = points[i - 1] + velocity * DeltaTime;
                velocity += gravity * DeltaTime;
                gravity *= Quaternion.RotationAxis(Actor.Transform.Forward, GravityClockwiseDelta * Player.GravityRotationScale * DeltaTime);
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
    }
}
