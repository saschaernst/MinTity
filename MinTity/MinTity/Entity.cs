using System;
using System.Collections.Generic;
using MinMVC;

namespace MinTity
{
	public class Entities
	{
		HashSet<Runner> runners = new HashSet<Runner>();
		Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
		LinkedList<Entity> cache = new LinkedList<Entity>();
		int nextId = 0;

		public Entity Create()
		{
			var entity = cache.Count > 0 ? GetFirst() : new Entity();
			entity.id = nextId++;
			entity.ManageEntity = ManageEntity;

			return entity;
		}

		Entity GetFirst()
		{
			var entity = cache.First.Value;
			cache.RemoveFirst();

			return entity;
		}

		public void Remove(Entity entity)
		{
			entity.RemoveAll();
			entities.Remove(entity.id);
			cache.AddLast(entity);
		}

		void ManageEntity(Entity entity)
		{
			runners.Each(runner => runner.ManageEntity(entity));
		}

		public void Run()
		{
			runners.Each(runner => runner.Run());
		}
	}

	public class Entity
	{
		List<Ability> abilities = new List<Ability>();

		public Action<Entity> ManageEntity;

		public int id { get; set; }

		public void Add(int id, Ability ability)
		{
			abilities[id] = ability;
			ManageEntity(this);
		}

		public bool Has(int id)
		{
			return abilities[id] != null;
		}

		public T Get<T>(int id) where T : Ability
		{
			return abilities[id] as T;
		}

		public void Remove(int id)
		{
			if (Has(id))
			{
				abilities.RemoveAt(id);
				ManageEntity(this);
			}
		}

		public void RemoveAll()
		{
			abilities.Clear();
			ManageEntity(this);
		}
	}

	public class Abilities
	{
		int nextId = 0;

		Dictionary<Type, int> abilityMap = new Dictionary<Type, int>();
		Dictionary<Type, Queue<Ability>> cache = new Dictionary<Type, Queue<Ability>>();

		public T Create<T>() where T : Ability, new()
		{
			var type = typeof(T);
			var queue = cache[type];
			var ability = queue.Count > 0 ? queue.Dequeue() as T : new T();
			ability.id = abilityMap.Retrieve(type, IncNextId);

			return ability;
		}

		int IncNextId()
		{
			return nextId++;
		}

		public void Remove<T>(T ability) where T : Ability
		{
			var type = typeof(T);
			var queue = cache.Retrieve(type);
			queue.Enqueue(ability);
		}
	}

	public class Ability
	{
		public int id { get; set; }

		public static Ability DEFAULT = new Ability();
	}

	public abstract class Runner
	{
		HashSet<Entity> entities = new HashSet<Entity>();
		HashSet<int> abilityIds = new HashSet<int>();

		public void AddId(int id)
		{
			abilityIds.Add(id);
		}

		public bool ManageEntity(Entity entity)
		{
			var isContained = entities.Contains(entity);
			var matchesAbilities = MatchesAbilities(entity);
			var isAdded = !isContained && matchesAbilities;
			var isRemoved = isContained && !matchesAbilities;

			if (isAdded)
			{
				entities.Add(entity);
			}
			else if (isRemoved)
			{
				entities.Remove(entity);
			}

			return isAdded || isRemoved;
		}

		bool MatchesAbilities(Entity entity)
		{
			foreach (var abilityId in abilityIds)
			{
				if (!entity.Has(abilityId))
				{
					return false;
				}

			}

			return true;
		}

		public void Run()
		{
			entities.Each(Handle);
		}

		protected abstract void Handle(Entity entity);
	}
}
