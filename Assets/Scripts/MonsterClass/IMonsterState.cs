namespace MonsterState
{
    public interface IMonsterState
    {
        void Enter(Monster monster);
        void Update(Monster monster);
        void Exit(Monster monster);
    }
} 