﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OlegEngine;
using OlegEngine.Entity;

namespace WheelOfSteamGames.Entity
{
    class base_actor : BaseEntity 
    {
        public const string AnimationDir = "/animations/";
        public double TimePerFrame = 1d / 45d;
        public int CurrentFrame = 0;
        public string CurrentAnimation = "idle";
        public Dictionary<string, int[]> Animations = new Dictionary<string, int[]>();
        public bool IsTransitioning = false;
        public int CurrentTransitionIndex = 0;

        private string[] TransitionAnimations = new string[2];
        private double nextFrameTime = 0;
        public override void Init()
        {
            this.SetModel(Resource.GetMesh("character_plane.obj", true));
            this.Mat = new Material(Utilities.ErrorTex, "default");
            this.Mat.Properties.AlphaTest = true;
            this.Mat.Properties.NoCull = true;
        }

        public override void Think()
        {
            base.Think();

            //Change our frame in accordance to time
            if (nextFrameTime < Utilities.Time && Animations.ContainsKey(CurrentAnimation))
            {
                double delta = Utilities.Time - nextFrameTime;
                nextFrameTime = Utilities.Time + this.TimePerFrame;
                CurrentFrame += 1 + (int)Math.Floor(delta / this.TimePerFrame);

                if (this.IsTransitioning && CurrentFrame >= this.Animations[CurrentAnimation].Length && CurrentTransitionIndex < TransitionAnimations.Length-1)
                {
                     CurrentTransitionIndex += 1;
                    this.SetAnimation(TransitionAnimations[CurrentTransitionIndex]);

                    //If we've reached the last animation in our queued animation, stop transitioning
                    if (CurrentTransitionIndex <= TransitionAnimations.Length)
                        this.IsTransitioning = false;
                }

                CurrentFrame = CurrentFrame < this.Animations[this.CurrentAnimation].Length ? CurrentFrame : 0;
                this.Mat.Properties.BaseTexture = this.Animations[this.CurrentAnimation][CurrentFrame];
            }
        }

        /// <summary>
        /// Load all animations associated with a character into memory
        /// </summary>
        /// <param name="name">The 'character name,' otherwise known as the folder name, that holds the animation frames</param>
        public void LoadAnimations(string name)
        {
            try
            {
                string folder = AnimationDir + name + "/";
                string[] files = Directory.GetFiles(Resource.TextureDir + folder);

                foreach (string file in files)
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string AnimationName = filename.Remove(filename.Length - 4);

                    if (!Animations.ContainsKey(AnimationName)) LoadSingleAnimation(AnimationName, folder);
                }
            }
            catch (Exception e)
            {
                Utilities.Print("Failed to load animations for '{0}'! {1}", Utilities.PrintCode.ERROR, name, e.Message);
            }
        }

        /// <summary>
        /// Set the animation of the actor
        /// </summary>
        /// <param name="name">The name of the animation</param>
        public void SetAnimation(string name)
        {
            //Don't set the animation if it doesn't exist
            if (!Animations.ContainsKey(name)) return;

            this.CurrentAnimation = name;
            this.CurrentFrame = 0;
        }

        /// <summary>
        /// Queue up multiple animations, to be set in order when the first one finishes
        /// </summary>
        /// <param name="Animations"></param>
        public void SetTransitionAnimation(string From, string To)
        {
            TransitionAnimations[0] = From;
            TransitionAnimations[1] = To;

            this.IsTransitioning = true;
            this.CurrentTransitionIndex = 0;
            this.SetAnimation(From);
        }

        private void LoadSingleAnimation(string animation, string folder )
        {
            int CurrentFrame = 1;
            string file = string.Format( "{0}{1}{2:D4}.png", folder, animation, CurrentFrame );
            List<int> Frames = new List<int>();
            while (File.Exists(Resource.TextureDir + file))
            {
                Frames.Add(Utilities.LoadTexture(file));
                CurrentFrame++;
                file = string.Format("{0}{1}{2:D4}.png", folder, animation, CurrentFrame);
            }

            Animations.Add(animation, Frames.ToArray());
        }
    }
}
