using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;

namespace TrexRunner.Entities
{
    public class EntityManager
    {
        // props
        private readonly List<IGameEntity> _entities = new List<IGameEntity>();
        private readonly List<IGameEntity> _entitiesToAdd = new List<IGameEntity>();
        private readonly List<IGameEntity> _entitiesToRemove = new List<IGameEntity>();

        public IEnumerable<IGameEntity> Entities => new ReadOnlyCollection<IGameEntity>(_entities);         // reader of this prop cannot manipulate the list, dope? read more

        // methods
        public void Update(GameTime gameTime)
        {
            foreach (IGameEntity entity in _entities)
            {
                if (_entitiesToRemove.Contains(entity))
                    continue;

                entity.Update(gameTime);

            }

            foreach (IGameEntity entity in _entitiesToAdd)
            {
                _entities.Add(entity);
            }

            foreach (IGameEntity entity in _entitiesToRemove)
            {
                _entities.Remove(entity);
            }

            _entitiesToAdd.Clear();
            _entitiesToRemove.Clear();


        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (IGameEntity entity in _entities.OrderBy(e => e.DrawOrder))
            {
                entity.Draw(spriteBatch, gameTime); 

            }

        }

        public void AddEntity(IGameEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Ayayai, null cannot be added as an entity");

            //*************************************************************************************
            //:headsup:
            //dont add like so: 👇🏼
            //_entities.Add(entity);
            //is not smart as it would mess up the foreach iterator
            //instead we copy to a temp list and append to _entities when the iterator is done
            //*************************************************************************************
            _entitiesToAdd.Add(entity);

        }

        public void RemoveEntity(IGameEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Ayayai, null cannot be removed as an entity");

            _entitiesToRemove.Add(entity);

        }

        public void Clear()
        {
            _entitiesToRemove.AddRange(_entities);

        }

        public IEnumerable<T> GetEntitiesOfType<T>() where T : IGameEntity
        {
            return _entities.OfType<T>();
        }

    }
}
