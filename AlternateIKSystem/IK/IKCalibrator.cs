﻿using RootMotion.FinalIK;
using UnityEngine;

namespace NAK.AlternateIKSystem.IK;

internal static class IKCalibrator
{
    public static VRIK SetupVrIk(Animator animator)
    {
        if (animator.gameObject.TryGetComponent(out VRIK vrik))
            UnityEngine.Object.DestroyImmediate(vrik);

        vrik = animator.gameObject.AddComponent<VRIK>();
        vrik.AutoDetectReferences();

        if (!ModSettings.EntryUseVRIKToes.Value)
        {
            vrik.references.leftToes = null;
            vrik.references.rightToes = null;
        }

        vrik.solver.SetToReferences(vrik.references);

        GuessWristPalmAxis(vrik.references.leftHand, vrik.references.leftForearm, vrik.solver.leftArm);
        GuessWristPalmAxis(vrik.references.rightHand, vrik.references.rightForearm, vrik.solver.rightArm);

        SafePalmToThumbAxis(vrik.references.leftHand, vrik.references.leftForearm, vrik.solver.leftArm,
            animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal));
        SafePalmToThumbAxis(vrik.references.rightHand, vrik.references.rightForearm, vrik.solver.rightArm,
            animator.GetBoneTransform(HumanBodyBones.RightThumbProximal));

        AddTwistRelaxer(vrik.references.leftForearm, vrik, vrik.references.leftHand);
        AddTwistRelaxer(vrik.references.rightForearm, vrik, vrik.references.rightHand);

        //vrik.solver.leftArm.shoulderRotationMode = (IKSolverVR.Arm.ShoulderRotationMode)IkTweaksSettings.ShoulderMode;
        //vrik.solver.rightArm.shoulderRotationMode = (IKSolverVR.Arm.ShoulderRotationMode)IkTweaksSettings.ShoulderMode;

        // zero all weights controlled by BodyControl
        vrik.solver.locomotion.weight = 0f;
        vrik.solver.IKPositionWeight = 0f;

        vrik.solver.spine.pelvisPositionWeight = 0f;
        vrik.solver.spine.pelvisRotationWeight = 0f;
        vrik.solver.spine.positionWeight = 0f;
        vrik.solver.spine.rotationWeight = 0f;

        vrik.solver.leftLeg.positionWeight = 0f;
        vrik.solver.leftLeg.rotationWeight = 0f;
        vrik.solver.rightLeg.positionWeight = 0f;
        vrik.solver.rightLeg.rotationWeight = 0f;
        vrik.solver.leftArm.positionWeight = 0f;
        vrik.solver.leftArm.rotationWeight = 0f;
        vrik.solver.rightArm.positionWeight = 0f;
        vrik.solver.rightArm.rotationWeight = 0f;

        vrik.solver.leftLeg.bendGoalWeight = 0f;
        vrik.solver.rightLeg.bendGoalWeight = 0f;

        // these weights are fine
        vrik.solver.leftArm.shoulderRotationWeight = 0.8f;
        vrik.solver.rightArm.shoulderRotationWeight = 0.8f;

        vrik.solver.leftLeg.bendToTargetWeight = 0.75f;
        vrik.solver.rightLeg.bendToTargetWeight = 0.75f;

        // hack to prevent death
        vrik.fixTransforms = !animator.enabled;

        // purposefully initiating early
        vrik.solver.Initiate(vrik.transform);
        vrik.solver.Reset();

        return vrik;
    }

    private static void GuessWristPalmAxis(Transform hand, Transform forearm, IKSolverVR.Arm arm)
    {
        arm.wristToPalmAxis = VRIKCalibrator.GuessWristToPalmAxis(
            hand,
            forearm
        );
    }

    private static void SafePalmToThumbAxis(Transform hand, Transform forearm, IKSolverVR.Arm arm, Transform thumbBone = null)
    {
        if (hand.childCount == 0)
        {
            arm.palmToThumbAxis = Vector3.one;
            return;
        }

        arm.palmToThumbAxis = VRIKCalibrator.GuessPalmToThumbAxis(
            hand,
            forearm,
            thumbBone
        );
    }

    private static void AddTwistRelaxer(Transform forearm, VRIK ik, Transform hand)
    {
        if (forearm == null) return;
        TwistRelaxer twistRelaxer = forearm.gameObject.AddComponent<TwistRelaxer>();
        twistRelaxer.ik = ik;
        twistRelaxer.weight = 0.5f;
        twistRelaxer.child = hand;
        twistRelaxer.parentChildCrossfade = 0.8f;
    }

    public static void ConfigureDesktopVrIk(VRIK vrik)
    {
        // From DesktopVRIK
        // https://github.com/NotAKidOnSteam/NAK_CVR_Mods/blob/fca0a32257311f044d1a9d6e68269baa4a65a45c/DesktopVRIK/DesktopVRIKCalibrator.cs#L219C2-L247C103

        vrik.solver.spine.bodyPosStiffness = 1f;
        vrik.solver.spine.bodyRotStiffness = 0.2f;
        vrik.solver.spine.neckStiffness = 0.0001f;
        vrik.solver.spine.rotateChestByHands = 0f;

        vrik.solver.spine.minHeadHeight = 0f;
        vrik.solver.locomotion.angleThreshold = 30f;
        vrik.solver.locomotion.maxLegStretch = 1f;

        vrik.solver.spine.chestClampWeight = 0f;
        vrik.solver.spine.headClampWeight = 0.2f;

        vrik.solver.spine.maintainPelvisPosition = 0f;
        vrik.solver.spine.moveBodyBackWhenCrouching = 0f;

        vrik.solver.locomotion.velocityFactor = 0f;
        vrik.solver.locomotion.maxVelocity = 0f;
        vrik.solver.locomotion.rootSpeed = 1000f;

        vrik.solver.spine.positionWeight = 0f;
        vrik.solver.spine.rotationWeight = 1f;

        vrik.solver.spine.maxRootAngle = 180f;

        vrik.solver.plantFeet = true;
    }

    public static void ConfigureHalfBodyVrIk(VRIK vrik)
    {
        // From IKTweaks
        // https://github.com/knah/VRCMods/blob/a22bb73a5e40c75152c6e5db2a7a9afb13e42ba5/IKTweaks/FullBodyHandling.cs#L384C1-L394C71

        vrik.solver.spine.bodyPosStiffness = 1f;
        vrik.solver.spine.bodyRotStiffness = 0f;
        vrik.solver.spine.neckStiffness = 0.5f;
        vrik.solver.spine.rotateChestByHands = .25f;

        vrik.solver.spine.minHeadHeight = -100f;
        vrik.solver.locomotion.angleThreshold = 60f;
        vrik.solver.locomotion.maxLegStretch = 1f;

        vrik.solver.spine.chestClampWeight = 0f;
        vrik.solver.spine.headClampWeight = 0f;

        vrik.solver.spine.maintainPelvisPosition = 0f;
        vrik.solver.spine.moveBodyBackWhenCrouching = 0f;

        vrik.solver.locomotion.velocityFactor = 0.4f;
        vrik.solver.locomotion.maxVelocity = 0.4f;
        vrik.solver.locomotion.rootSpeed = 20f;

        vrik.solver.spine.positionWeight = 1f;
        vrik.solver.spine.rotationWeight = 1f;

        vrik.solver.spine.maxRootAngle = 0f;

        vrik.solver.plantFeet = false;
    }

    // TODO: figure out proper Desktop & VR organization
    public static void SetupHeadIKTargetDesktop(VRIK vrik)
    {
        // Lazy HeadIKTarget calibration
        if (vrik.solver.spine.headTarget == null)
            vrik.solver.spine.headTarget = new GameObject("Head IK Target").transform;
        vrik.solver.spine.headTarget.parent = vrik.references.head;
        vrik.solver.spine.headTarget.localPosition = Vector3.zero;
        vrik.solver.spine.headTarget.localRotation = Quaternion.identity;
    }
}