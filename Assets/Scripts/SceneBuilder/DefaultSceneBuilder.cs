using Director;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SceneBuilder
{
    public class DefaultSceneBuilder : SceneBuilder
    {
        private Transform _playerPosition;
        public Transform PlayersPosition { 
            get => _playerPosition;
            set 
            {
                _playerPosition = value;    
                UpdatePlayerLayout();
            } 
        }
        protected override void Build() { }

        public static DefaultSceneBuilder Construct() {
            GameObject gameObject = new GameObject(typeof(DefaultSceneBuilder).Name);
            return gameObject.AddComponent<DefaultSceneBuilder>();
        }

        public void OnEnable()
        {
            EntityClass.OnEntitySpawn += HandleEntityChange;
            EntityClass.OnEntityDeath += HandleEntityChange;
        }

        public void OnDisable()
        {

            EntityClass.OnEntitySpawn -= HandleEntityChange;
            EntityClass.OnEntityDeath -= HandleEntityChange;
        }

        private void HandleEntityChange(EntityClass entityClass)
        {
            UpdatePlayerLayout();
        }

        private void UpdatePlayerLayout()
        {
            if (PlayersPosition == null) return;
            List<EntityClass> players = new GetTeammates(EntityTeam.PlayerTeam).Query() ?? new();

            var positions = PositionsFrom(PlayersPosition.transform.position, players.Count);
            for (var i = 0; i < players.Count; i++)
            {
                players[i].SetReturnPosition(positions[i]);
            }
        }
        private static Vector3[] PositionsFrom(Vector2 centerCoordinate, int count)
        {
            var dx = -1f * Mathf.Sign(centerCoordinate.x);
            const float dy = 1f;

            var height = (count - 1) * dy;
            var top = centerCoordinate.y + height / 2f;

            var positions = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                positions[i] = new Vector3(centerCoordinate.x + (i % 2 == 0 ? 1f : -1f) * dx, top - i * dy);
            }

            return positions;
        }
    }
}
