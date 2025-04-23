using MonsterState;
public class MonsterState_Dead : IMonsterState
{
    public void Enter(Monster monster) { monster.Die(); }
    public void Update(Monster monster) { }
    public void Exit(Monster monster) { }
} 