using System.Threading.Tasks;

namespace Fireworks
{
	public interface IEntityManager
	{
		Task Run(int msDelay);

		void Spawn(IEntity entity);
	}
}