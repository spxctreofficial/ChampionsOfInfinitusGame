public class Parry
{

	private ChampionController parrying, parried;
	private Card parryingCard, defendingCard;

	public ChampionController Parrying
	{
		get => parrying;
		set => parrying = value;
	}
	public ChampionController Parried
	{
		get => parried;
		set => parried = value;
	}

	public Card ParryingCard
	{
		get => parryingCard;
		set => parryingCard = value;
	}
	public Card DefendingCard
	{
		get => defendingCard;
		set => defendingCard = value;
	}

	public bool ParryCanStart => parrying is {} && parried is {} && parryingCard is {};

	public Parry(ChampionController parrying, ChampionController parried = null, Card parryingCard = null)
	{
		this.parrying = parrying;
		this.parried = parried;
		this.parryingCard = parryingCard;
		FightManager.parryInstance = this;
	}
}
