using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class Reseter : Script
    {
        public float Edge = 1000f;
        public Vector3 ResetPosition;
        private Actor player;

        public override void OnAwake()
        {
            player = Scene.FindActor("Player");
        }

        public override void OnLateUpdate()
        {
            var pos = Vector3.Abs(player.Position);
            if (pos.MaxValue > Edge)
                Reset();
        }

        public void Reset()
        {
            player.Position = ResetPosition;
            player.Orientation = Quaternion.Identity;
            Physics.Gravity = Vector3.Down * 981;
            player.As<RigidBody>().LinearVelocity = Vector3.Zero;
        }
    }
}
