using UnityEngine;
using Godus.Core;

namespace Godus.Player
{
    /// <summary>
    /// Data-driven class stats. Create one .asset per class in the Inspector.
    /// Balancing is done by editing these values — never touch gameplay code.
    /// </summary>
    [CreateAssetMenu(fileName = "ClassStats_Knight", menuName = "Godus/Class Stats")]
    public class ClassStats : ScriptableObject
    {
        [Header("Identity")]
        public string className = "Knight";
        public string classDescription = "Tanky, slow, high poise. Heavy melee.";

        [Header("Movement")]
        [Tooltip("Max horizontal speed in units/sec")]
        public float moveSpeed = 4f;
        [Tooltip("How fast the player accelerates to moveSpeed")]
        public float acceleration = 30f;
        [Tooltip("How fast the player decelerates when no input")]
        public float deceleration = 25f;

        [Header("Combat — Health")]
        public int maxHP = 120;
        [Tooltip("Frames of invulnerability after taking damage")]
        public float hurtIFrames = 0.5f;

        [Header("Combat — Attack")]
        public int baseAttackDamage = 25;
        [Tooltip("Seconds — full swing duration")]
        public float attackDuration = 0.45f;
        [Tooltip("Seconds into the swing when the hitbox becomes active")]
        public float attackHitWindow = 0.15f;
        [Tooltip("Radius of the melee hitbox")]
        public float attackRange = 1.8f;
        [Tooltip("Attack cooldown after swing ends")]
        public float attackCooldown = 0.3f;

        [Header("Combat — Dash (Knight variant)")]
        [Tooltip("Dash distance in units")]
        public float dashDistance = 3f;
        [Tooltip("Seconds — how long the dash lasts")]
        public float dashDuration = 0.18f;
        [Tooltip("Seconds — cooldown before next dash")]
        public float dashCooldown = 1.2f;
        [Tooltip("True = Knight dash-tackle gives brief poise/armor instead of full i-frames")]
        public bool dashGrantsPoise = true;
        [Tooltip("Damage reduction during dash poise (0-1, 0.5 = half damage)")]
        public float dashPoiseReduction = 0.5f;
        [Tooltip("Knockback resistance during dash poise (0-1, 1 = immune)")]
        public float dashPoiseKnockbackResist = 0.8f;

        [Header("Ground Detection")]
        [Tooltip("Layer mask for ground/walls (layer 1 = World)")]
        public LayerMask groundLayer = 1;
    }
}
