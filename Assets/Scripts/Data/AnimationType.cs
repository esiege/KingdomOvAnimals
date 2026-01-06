namespace KOA.Data
{
    /// <summary>
    /// Defines the animation style used when an ability is activated.
    /// </summary>
    public enum AnimationType
    {
        None,           // No animation
        Melee,          // Card moves toward target and back
        Projectile,     // Spawns a projectile that travels to target
        AOE,            // Area of effect animation (explosion, wave, etc.)
        Buff,           // Positive effect on self/ally (sparkles, glow)
        Debuff,         // Negative effect on enemy (dark particles)
        Heal,           // Healing animation (green particles, hearts)
        Shield,         // Defensive animation (barrier appears)
        Summon          // Summoning animation for new cards
    }
}
