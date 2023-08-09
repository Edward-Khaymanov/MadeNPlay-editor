using UnityEngine;

namespace MadeNPlayShared
{
    [CreateAssetMenu(fileName = "Team", menuName = "MadeNPlay/New Team")]
    public class GameTeamAsset : ScriptableObject
    {
        [SerializeField] private GameTeam _team;

        public GameTeam Team => _team;
    }
}