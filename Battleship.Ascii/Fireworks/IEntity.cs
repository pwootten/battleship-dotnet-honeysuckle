using System;

namespace Fireworks
{
	public interface IEntity : IDisposable
	{
		void Draw();

		UpdateResult Update();

		Action RequestDeathWish(IEntityManager entityManager);
	}
}