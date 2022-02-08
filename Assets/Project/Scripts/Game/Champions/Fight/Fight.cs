public class Fight
{

	private ChampionController attacker, defender;
	private Card attackingCard, defendingCard;

	public ChampionController Attacker
	{
		get => attacker;
		set => attacker = value;
	}
	public ChampionController Defender
	{
		get => defender;
		set => defender = value;
	}

	public Card AttackingCard
	{
		get => attackingCard;
		set => attackingCard = value;
	}
	public Card DefendingCard
	{
		get => defendingCard;
		set => defendingCard = value;
	}

	public bool AttackCanStart => attacker is {} && defender is {} && attackingCard is {};

	public Fight(ChampionController attacker, ChampionController defender = null, Card attackingCard = null)
	{
		this.attacker = attacker;
		this.defender = defender;
		this.attackingCard = attackingCard;
		FightManager.fightInstance = this;
	}
}
