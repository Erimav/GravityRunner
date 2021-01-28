using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class Checkpoint : Script
    {
        public static Checkpoint Current { get; set; }

        /// <summary>
        /// An actor whose position and orientation will be applied on checkpoint loading
        /// </summary>
        public Actor DefaultPose;
        public Vector3 DefaultGravity = new Vector3(0, -981, 0);
        public Collider Trigger;

        public override void OnStart()
        {
            Current = Current ?? this;
            Trigger = Trigger ?? Actor.GetChild<Collider>();
            DefaultPose = DefaultPose ?? Actor;

            Trigger.TriggerEnter += actor =>
            {
                if (actor.LayerName == "Player")
                    Save();
            };
        }

        public void Save()
        {
            if (Current == this)
                return; // Already saved

            Current = this;
        }

        public void Load()
        {
            var player = Scene.FindActor("Player");
            player.Transform = DefaultPose.Transform;
            Physics.Gravity = DefaultGravity;
            player.GetScript<PlayerScript>().Reset();
        } 
    }
}
