using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {


        public class ThrustControl
        {
            public const float TOP_ANGLE = 180;
            public const float BOTTOM_ANGLE = 90;
            const float MAX_SPEED = 0.1f;
            const float MAX_ACCELERATION = 0.002f;
            const float MAX_THRUST = 550;
            const float MIN_THRUST = 0;
            const float THRUST_ADJUSTMENT_DELTA = 1;
            const float THRUST_MAGNITUDE = 1000;
            const int DEBUG_PADDING = 22;

            List<IMyThrust> thrusters;
            IMyMotorStator rotor;
            IMyPistonBase piston;

            float thrust;
            float previousAngle;
            float currentAngle;
            float targetAngle;
            float distanceRemaining;
            float previousSpeed;
            float currentSpeed;
            float targetSpeed;
            float acceleration;
            int currentDirection;
            int directionChange;

            // Used to make a better display for rapidly changing numbers.
            float[] speedHistory;
            float[] accelerationHistory;
            float speedAvg;
            float accelerationAvg;

            public ThrustControl(ExpectedBlocks blocks)
            {
                thrusters = blocks.GetBlocks<IMyThrust>(CRANE_ARM_THRUSTER);
                rotor = blocks.GetBlock<IMyMotorStator>(CRANE_ARM_ROTOR);
                piston = blocks.GetBlock<IMyPistonBase>(CRANE_ARM_PISTON);
                thrust = thrusters[0].ThrustOverride / 1000;
                targetAngle = BOTTOM_ANGLE;
                previousAngle = ConvertAngle(rotor.Angle);
                speedHistory = new float[60];
                accelerationHistory = new float[120];
            }

            public float GetCraneArmAngle()
            {
                return currentAngle;
            }

            public void Adjust(float position)
            {
                targetAngle = position;
            }

            public void OverrideThrust(int newThrust)
            {
                thrust = newThrust;
            }

            public void Update()
            {
                // First set the values we need.
                currentAngle = ConvertAngle(rotor.Angle);
                currentSpeed = currentAngle - previousAngle;
                acceleration = currentSpeed - previousSpeed;
                distanceRemaining = targetAngle - currentAngle;
                if (Math.Abs(distanceRemaining) > 20)
                    targetSpeed = distanceRemaining > 0 ? MAX_SPEED : -MAX_SPEED;
                else
                    targetSpeed = MAX_SPEED * distanceRemaining / 20;


                // If the rotor needs to push the arm, do so.
                EngageRotor();

                // Adjust the piston to get it to the position it should be in.
                AdjustPiston();

                // Now determine how much to adjust thrust.
                AdjustThrust();

                // Apply thrust adjustment.
                thrusters.ForEach(thruster => thruster.ThrustOverride = thrust * THRUST_MAGNITUDE);

                // Calculate the speed and acceleration to display.
                CalcDisplayNumbers();

                // Lastly save the values we need for next update.
                previousAngle = currentAngle;
                previousSpeed = currentSpeed;
            }

            // Convert angles to degrees and keep them in range of 90 to 180.
            float ConvertAngle(float angle)
            {
                float degrees = angle / (float)Math.PI * 180;
                return degrees > 180 ? degrees - 270 : degrees + 90;
                //return (angle > 180 ? angle - 360 : angle) + 90;
            }

            void EngageRotor()
            {
                if (targetAngle == BOTTOM_ANGLE && currentAngle > TOP_ANGLE - 50)
                    rotor.TargetVelocityRPM = -.5f;
                else if (targetAngle == TOP_ANGLE && currentAngle > TOP_ANGLE)
                    rotor.TargetVelocityRPM = -.1f;
                else
                    rotor.TargetVelocityRPM = 0;
            }

            void AdjustPiston()
            {
                float targetLength = (90 - (currentAngle - 90)) / 90 * 10;
                targetLength = Math.Min(targetLength, 10);
                targetLength = Math.Max(targetLength, 0);
                piston.Velocity = targetLength - piston.CurrentPosition;
            }

            void AdjustThrust()
            {
                // Figure out direction.
                if (currentSpeed == 0)
                    currentDirection = 0;
                else
                    currentDirection = (int)Math.Round(currentSpeed / Math.Abs(currentSpeed), 0);
                directionChange = targetAngle > currentAngle ? 1 : -1;

                if (currentDirection != (int)Math.Round(targetSpeed / Math.Abs(targetSpeed), 0)) // Going the wrong direction?
                    directionChange = -currentDirection;
                else if (
                    acceleration > MAX_ACCELERATION * currentDirection  // Accelerating too much?
                    && Math.Abs(currentSpeed) > MAX_SPEED * .2
                )
                    directionChange = -currentDirection;
                else if (Math.Abs(currentSpeed) > Math.Abs(targetSpeed)) // Going too fast?
                    directionChange = -currentDirection;

                float adjustment = THRUST_ADJUSTMENT_DELTA;
                if (Math.Abs(distanceRemaining) < 2)
                    adjustment /= 2;
                if (Math.Abs(distanceRemaining) < 1)
                    adjustment /= 2;
                if (Math.Abs(distanceRemaining) < .05f)
                {
                    adjustment = 0;
                    currentDirection = 0;
                    directionChange = 0;
                    acceleration = 0;
                }

                // Figure out adjusted thrust.
                thrust += adjustment * directionChange;
                thrust = Math.Max(thrust, MIN_THRUST);
                thrust = Math.Min(thrust, MAX_THRUST);
            }

            void CalcDisplayNumbers()
            {
                // Avg speed calc:
                for (int i = 0; i < 59; i++)
                    speedHistory[i] = speedHistory[i + 1];
                speedHistory[59] = currentSpeed;
                speedAvg = 0;
                for (int i = 0; i < 60; i++)
                    speedAvg += speedHistory[i];
                speedAvg /= 60;
                // Avg acceleration calc:
                for (int i = 0; i < 119; i++)
                    accelerationHistory[i] = accelerationHistory[i + 1];
                accelerationHistory[119] = acceleration;
                accelerationAvg = 0;
                for (int i = 0; i < 120; i++)
                    accelerationAvg += accelerationHistory[i];
                accelerationAvg /= 120;
            }

            public string GetDebugText()
            {
                return "Crane Arm\n" +
                    FormatDebugLine("Current angle", Math.Round(currentAngle, 1)) +
                    FormatDebugLine("Target angle", targetAngle) +
                    FormatDebugLine("Distance", Math.Round(distanceRemaining, 1)) +
                    FormatDebugLine("Thrust", thrust, MAX_THRUST) +
                    FormatDebugLine("Current speed", speedAvg, MAX_SPEED) +
                    FormatDebugLine("Target speed", targetSpeed, MAX_SPEED) +
                    FormatDebugLine("Acceleration", accelerationAvg, MAX_ACCELERATION) +
                    FormatDebugLine("Current direction", currentDirection) +
                    FormatDebugLine("Direction change", directionChange);
            }

            string FormatDebugLine(string attribute, double value, double max = 0)
            {
                string valueDisplayed;
                if (max != 0)
                    valueDisplayed = Math.Round(value / max * 100, 0).ToString() + "%";
                else
                    valueDisplayed = value.ToString();
                return (attribute + " : ").PadLeft(DEBUG_PADDING) + valueDisplayed + "\n";
            }
        }
    }
}
