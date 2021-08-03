# Land Of Heroes

Land Of Heroes is a tactical, strategy-based card game for both veteran strategy game players and beginners alike. Land Of Heroes is easy-to-learn, but hard to master! Play as any champion, whether it be a loyal squire, a sly bandit, or a powerful mage! Use different cards to attack, heal, or other miscellaneous actions, and use your hero's abilities to gain an advantage in the battle!

## How to Play

#### Earn & Choose Champions
In Land Of Heroes, you can earn new champions by playing the game, and use the gold earned for just playing matches to buy new champions! While the physical statistics, abilities, play styles, and cost of every champion may be different, the physical attributes of the champion you play will *not* solely determine the result of your match. With experience, you can topple the best champions with even the bare minimum of abilities and effort. A champion's strength varies with the player that is controlling them, so don't get your hopes down when it looks like all is lost and the opponent has a significant advantage. in terms of raw physical strength. With the right play style and adaptation to your champion, anything is possible and the possibilities are limitless!


#### Use Your Cards
At the beginning of the game, every player's champion is dealt 4 cards. During a champion's Beginning Phase, they are dealt an additional 2 cards. Finally, at the end of your turn, if you have more than 6 cards, you must discard until you have 6 cards.<br>
Every suit (that is the "icon" shown on the card's sprite) has a different functionality. These allow for the game to be unique, even with a standard poker deck.

##### Spades
When played normally, spades start an attack against another target. However, you will still need to play another card as a "combat card", which will be calculated to the outcome of the attack. The suit will not matter for your "combat card", but you *must* use a spade to **start** an attack. Be warned though! Because of the nature of combat, it is possible for you to fail your attack and be counter-attacked instead! When you have chosen a target and a "combat card", combat will commense! The defender must also play a card as their "combat card", and the values of these cards will be compared.<br>
These are the possible outcomes of combat:
- The attacker's "combat card" has a larger value than the defender's "combat card", and the attack is successful (the defender is dealt damage).
- The defender's "combat card" has a larger value than the attacker's "combat card", and the attack is unsuccessful (the attacker is dealt damage).
- Both "combat cards" had the same value, and the attack is a stalemate (no one is dealt damage).
You may only play one spade per turn.

##### Hearts
When played normally, hearts can heal the user. Very self-explanatory, but this is arguably one of the more useful cards because they allow you to regain lost health and regain an advantage against the enemy team. Be warned though! There is a cap on how many hearts you may play per turn. Here is a simplified breakdown:<br>
Think of the amount of hearts you can play per turn as an integer value. This value by default is 3. Whenever you play a heart, this value will be decremented by a certain number.
- 2-7 of Hearts: Decrement by 1
- 8-10 of Hearts: Decrement by 2
- J, Q, K, & A of Hearts: Decrement by 3
This means the amount of hearts you can play is dynamic depending on what heart you play. However, the good side of this is that as the value of the heart increases, so does the amount of health you heal for.

##### Clubs
When played normally, clubs can be traded into the discard area for another card.

#### Diamonds
Finally, diamonds do miscellaneous actions when played normally. Each diamond has a unique miscellaneous action, but they tend to affect all living champions of a match, rather than affecting only the user.<br>
The full breakdown of what each diamond does can be seen in-game while hovering over a diamond.

#### Earn Gold & XP
Just like with any other progress-based game, you can earn experience and gold from playing matches! These can be used either to earn new champions and/or rewards, purchase new champions, cosmetics, maps, etc., or as a bragging right to your friends!

#### Most importantly, have fun!
Seriously! The project started as a hobby, because I wanted to create something that would be fun as a fail-safe for if everyone were bored of the same games and procedures. If you are not having fun, and it is *not* the game's fault, take a break and do some breathing exercises! Otherwise, if the issue *is* with the game, leave an issue on the GitHub repository stating your problems and dislikes that you would like to be addressed! (You can alternatively email me: spxctre@spxct.cf.) I frequently check both of those places, and I will respond as soon as possible (typically between 24-48 hours)!


## Download & Install
Go to `LandOfHeroesGame/Releases` on the GitHub repository and find the latest version of the build you are looking for! Alternatively, you can install a build of the game from itch.io, which I will be regularly updating as a mirror for the game's download!<br>
If you would like to help with the game's development, or simply wish to try out the latest changes that have not been built specifically as a version yet, you can alternatively clone the project and build the game from Unity, however this is a slippery slide that can lead to many errors (and is often not recommended unless you know what you're doing.)