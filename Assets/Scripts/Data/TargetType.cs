namespace KOA.Data
{
    /// <summary>
    /// Defines valid targets for an ability.
    /// Maps to existing AbilityTargetType but owned by the data layer.
    /// </summary>
    public enum TargetType
    {
        Self,           // Target self
        SingleEnemy,    // Target one enemy card
        SingleAlly,     // Target one friendly card (renamed from SingleFriendly for clarity)
        AllEnemies,     // Target all enemy cards
        AllAllies,      // Target all friendly cards
        EnemyPlayer,    // Target the opponent player directly
        BoardWide       // Affect all cards on the board
    }
}
