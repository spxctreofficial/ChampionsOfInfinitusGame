# Champions of Infinitus Official Changelog

This is the official Champions of Infinitus Changelog. Any changes to the game will be documented here for ease-of-use and convenient reference, allowing clients and players to understand the key differences between updates for those who do not understand C# or code in general.



## v0.3 Beta

This update is overhauls the card system, champions, sprites, and the game as a whole! Tons of changes, additions, and fixes were implemented to make the game better, more fun, and less infuriating to play.

- Every single card has been overhauled in the game!
	- New cards now have a simple, vector-based design.
	- New cards are now divided into two distinct colors: Light and Dark. These colors can affect card interactions with certain champions and other cards.
	- New cards are now divided into three distinct classifications that will determine how they are dealt: Universal, class-specific, and unique cards.
		- Universal cards are cards that any champion can be dealt. They are the most common, and generally have a universal task that they do.
		- Class-specific cards are cards that are only dealt to champions of a particular class. While they are less common, they are still considered universal "within their class".
		- Unique cards are cards that can only be dealt to specific champions. This means that they are unobtainable cards to any other champion and are considered the rarest cards.
	- Card-dealing has been overhauled. The algorithm for which the cards are dealt is complicated to explain, but do note that the dealing of cards is no longer perfectly random anymore.
- Champions have received a major overhaul in how their health are handled, how they use cards, and their abilities.
	- Champions now have single-digit health instead of triple-digit health. This change was made to make calculating health easier on the player.
	- Champions now have new mechanics that dictate what you can do on your turn!
		- Champions are now classified as different classes! Each different class will have a unique set of class-specific cards added on to each champion's unique champion-specific cards.
		- Champions now have stamina: playing cards require stamina from the champion. Champions refill their stamina at the start of their turn.
			- Stamina maxes out at 8.
		- Champions now use weapons to attack!
			- Weapons are items that can be equipped to a champion that allows the champion to attack and parry.
			- Weapons can lose durability. When they lose all their durability, they will break.
			- Each champion has a signature weapon. This signature weapon can be equipped with the "Equip Signature Weapon" card.
	- Technical: Removed champion information variables from the `ChampionController`. All access to these variables have been redirected to the ScriptableObject itself.
	- ChampionInfoPanel received some UI improvements and changes:
		- ChampionInfoPanel's zoom-in and zoom-out animation has been refined to resemble that of the ConfirmDialog's animation.
- The GUI have received numerous animation improvements!
	- Certain UI elements now have a nice fade-in and a shorter zoom-in effect that makes the animation pop out and look quite modern and smooth.
	- Fixed any UI-scaling issues that make the game look wonky at times.
	- Fixed an issue where tooltips would fade out randomly when hovering over certain objects.
		- Technical: This was fixed by disabling any invisible objects that blocked raycasts, and therefore resetting the tooltip.
	- Optimized the HDR glow's impact on performance.
- Added a Pause Menu to allow the player to quit the game.
- Added a Setting within the pause menu that allowed the player to adjust simple graphics and quality settings.
	- I am very aware that the settings menu is currently only accessible within the game.
- GameEndPanel received a couple of bug fixes:
	- Fixed an issue where clicking Collect before the rewards had been completely shown would result in the reward not being collected.
	- The GoldDisplay has been sped up if the change in gold is drastic.
- Technical: Prefab references have been moved to `PrefabManager`.

## v0.2.1 Beta

v0.2.1 is an incremental update to v0.2, adding some quality-of-life changes and additions and introducing bug fixes that addresses previous issues.

- Changes & Fixes:
	- AbilityController has been refactored and renamed to Ability.
	- Ability has been refactored and renamed to AbilityScriptableObject.
	- The selected SPADE to start an attack now glows white.
	- The selected combat card now glows red.
	- The end-of-game behavior has seen a complete overhaul.
		- Added a new prefab and script `GameEndPanel` that stores data and references relating to end-of-game behavior.
		- Code used to reward players and handle game-over screens have been optimized and refined.
		- No longer requires `transform.GetComponent<T>` calls to reference certain GameObjects in the scene, reducing complexity and increasing performance.
		- Fixed `totalHealthRemainingBonus` from not working by casting a division calculation to `float`.
	- The UI has received some graphical improvements:
		- All UI elements using UISprite has been replaced with a new custom sprite with rounded corners.
		- The NotificationDialog, ConfirmDialog, and DialogueSystem's color scheme has been darkened.
		- The NotificationDialog, ConfirmDialog, and DialogueSystem's font has been updated to Inter (the Unity Editor font) from Roboto and Futura Sans.
		- The PlayerActionTooltip and PhaseIndicator text elements have had their fonts updated to Inter.
		- The Main Menu text elements have had their fonts updated to Inter.
		- The pitch black background of the Main Menu has been changed in favor for a dark gradient background.
		- Many UI animations (namely SmartHover, Shop Panel transition, DialogueSystem tween-in) now uses `SetEaseOutQuart`, which makes the animation snappier and smoother.
	- The save file format has been changed from `.lohsave` to `.coisave`.

## v0.2 Beta

- Additions:
	- A comprehensive tutorial has been added to the game. This tutorial aims at teaching new players the basics of the game, helping them grasp the mechanics and skill required to advance in the game.
		- This tutorial utilizes the Dialogue System to explain to the player how the game works and the mechanics of the game. This is explained through the perspective of Sensei.
		- First the tutorial teaches the player how to use SPADEs to attack an opponent.
	- A ChampionInfoPanel has been added to the game. This gives a more verbose description of the champions and their respective abilities.
		- This replaces the traditional prompt when purchasing a champion from the shop.
		- Shown right-clicking when selecting a champion in Sandbox mode.
		- Shown clicking and holding on a champion in Sandbox mode during an active match.
	- The DialogueSystem has received some optimizations and improvements.
		- There is now a range of speeds that the caret (the typing of the characters) can use.
			- Unfiltered: This is the simplest, using a `0.05f` static rate per character.
			- Natural: A natural speed for expressing dialogue, with slightly longer pauses for punctuation.
			- Long Pause: This is a complicated speed, pausing longer on punctuation (excluding commas).
			- Fast: Just twice the speed of Natural.
	- Added three new maps to the game: Harbor, Mooncave and Sensei's Dojo.
	- Added a fully functional and flexible Advantage Feed to the Card Template.
	- Attacks can now be cancelled. (FINALLY)
		-  Currently, the implementation is quite sloppy. We hope to change this with all of the other buttons in a later patch, but as long as it works now.
- Changes & Fixes:
	- All audio-related resources have been compressed, resulting in a dramatic reduction in file size.
		- Previously, the audio files were all in `wav` format, which meant that it was uncompressed audio.
		- These `wav` files have been compressed to `mp3` files running at a 320kbps bitrate, which is a compressed high-quality format that will result in barely any audio quality loss.
		- Certain themes have been reworked or changed completely.
	- The LandOfHeroesGame logo has been completely revamped.
	- The descriptions of champions have been slightly improved and corrected.
	- AbilityPanel has been removed from the game.
	- These abilities have been patched and fixed:
		- A giant vulnerability with Bojutsu has been patched. Bojutsu had the ability to change a value of a card at runtime, and have that value save.
		- An UI-related issue with Stealth and targeting has been fixed (Stealth would briefly appear when that champion was targeting other champions.)
		- An UI-related issue regarding all abilities where some longer-named abilities may clip off the sides in the Ability Feed Entry. This has been remedied with an auto-sized Ability Feed Entry.
	- Fixed a big issue where the player wasn't being set correctly as a player in the config.
	- Fixed a big issue where the player wasn't being correctly rewarded gold at the end of a match of Competitive 2v2.
	- Fixed an issue where cards discarded upon death would make the discard area larger.
	- Fixed an issue where cards had incorrect values.
	- Fixed an issue where cards had incorrect sprites.
	- Fixed an issue where cards dealt by a 2 of Diamonds or 4 of Diamonds would be dealt to dead champions.
	- Fixed an issue where the nemesis system was being ignored.
	- The shop menu now clips properly with edges of its viewport.
	- The PlayerActionTooltip now correctly disappears after being used.
	- The camera shakes have been fine-tuned.
- Gameplay Changes:
	- Hoplomachus has been slightly reworked.
		- Hoplomachus' Hoplite Tradition ability has been removed from the game.
		- Hoplomachus' Hoplite Shield has been slightly buffed, increasing chance of activation from 20% to 33%.
		- These changes were made to change the Hoplomachus' play-style. Whereas the Hoplomachus used to be able to heal for a significant amount if the opponent drew high-value cards, it didn't seem to fit with his faction and lore. Instead, we removed the ability to heal, and instead allowed the Hoplomachus to be able to resist attacks better by buffing his Hoplite Shield.
	- Regime Captain has been reworked in favor of a buff.
		- Regime Captain no longer has Quick Assist.
		- Regime Captain now has a new ability: Persistence.
		- Persistence: Whenever Regime Captain is attacked, the "persistence meter" increments by 5. Whenever the Regime Captain attacks successfully, the amount in the "persistence meter" will be added onto the damage and the "persistence meter" will reset.
		- This change was made to make the Regime Captain more in-line with the Regime faction, and to make Regime Captain a more useful and active champion.
- Miscellaneous Notes:
	- The main Sandbox classes have been abstracted, meaning that their base class are now separate from the component that is added to the scene.
		- GameController has been abstracted.
		- CardLogicController has been abstracted.
		- StatisticManager has been abstracted.
		- UI Buttons have been given abstract/virtual methods (although the classes themselves remain fully functional.)
	- AbilityController has been given several new insertion points for ability checks, particularly to do with combat result.
	- A `FindCardInfo` static function has been added to the CardIndex class. This method allows an easy way to find any CardScriptableObject in the CardIndex with the given suit and card value.
	- The `GetCard` function has been slightly improved, no longer relying on the transform of the hand (instead using the list implemented into the hand itself to log its cards.)
	- New functions have been added to the `Hand` class to improve the game.
	- StateFeedEntry has been drastically improved, and ChampionAbilityFeed has been made as generic as possible.
	- The project now uses explicit types instead of implicit types (`var`) to define variables.
	- The project now replaces all empty strings (`""`) with references to `string.Empty`.
	- The project has been optimized, and resolved of compile and shader errors.

## v0.1.3 Beta

v0.1.3 is a beta release that adds new features and improvements to the game, primarily focusing on the UI elements and UI animations of the game. It also addresses some major bugs that have been found while play-testing with a friend.

- Additions:
	- Added a dynamic and flexible Dialogue System to pave the way for Campaign and Tutorial mode.
		- Includes custom SFXs and animations.
		- The typing animation has a variable speed, dependent on whether the character is white-space or punctuation.
		- The skip button allows the current sentence to be automatically finished.
		- The continue button will advance to the next sentence.
		- This dialogue system is currently used to introduce new users to the UI of the game. In the future, this dialogue system will be used throughout the game to advance the plot of the story.
	- Serialized files storing data are now hidden by default.
		- Serialized files will always hide themselves after saving.
		- This change was made to conceal the save files just a bit. This certainly will not stop people from changing the files however.
- Changes & Fixes:
	- The window slide animation's easing effect has been changed from EaseInOutQuad to EaseOutQuad (the difference is that easing is no longer applied at the beginning of the game.)
	- The ChampionShopButton and the GoldDisplay prefabs have been renovated to include Layout Groups and Layout Elements in order to keep the aesthetic clean and self-manageable (where previously would break after a certain amount of characters were placed in the textboxes.)
	- Vsync has been turned back on to remove tearing artifacts (since this isn't even a FPS or AAA title).
	- Fixed an issue where the discard area wasn't being pruned effectively, so it would gradually grow until it reached the edge of the screen.
		- Implementations to fix this were to prune during the Beginning Phase and when the gamble button was clicked.
	- Various UI issues have been resolved:
		- Fixed an UI issue where an error tooltip was shown when discarding a card after gambling during defense.
		- Fixed an UI issue where a "DISCARDED" feed wouldn't show up when a bot discarded at the end of their turn.
		- Fixed an UI issue where the (now deprecated) PlayerActionTooltip would linger after defending.
	- The bot targeting system when using a SPADE has been slightly changed:
		- Bots are now more likely to target overall.
	- A player's nemesis will now always reset to "None" after the nemesis has died.
	- Fixed an issue where the serialization of the gold amount at the end of a Sandbox match didn't work.
	- Fixed issues regarding the resolution of the game, and other gimmicks that have been found to resize the game.
- Miscellaneous Notes:
	- The context menu for creating new ScriptableObjects for the game has been altered.
	- The DataManager saves new variables to a new file for first-run variables: variables that allow the game to know if it is the first time the game has been launched by that user.

## v0.1.2 Beta

v0.1.2 is a minor beta release that improves theoretical stability and updates some UI elements for the sake of appearance.

- Bug Fixes:
	- Champions without abilities now correctly display "None".
- Stability Updates:
	- Theoretical stability improvement by switching all LeanTween `LTDescr` references to `int` references of the unique ID. Since LeanTween's implementation of unique IDs can be changed on the fly, some UI animations may fail to finish before they are accidentally cancelled (due to a misreference of the tween event's unique ID.) By storing that unique ID, we have a reference to the unique ID of that tween event, and nothing else, theoretically improving animation stability and removing any animation glitches.
- Miscellaneous Notes:
	- Added a Notification Dialog Prefab that can be used to indicate or notify the player of certain objects.

## v0.1.1 Beta

v0.1.1 is a minor beta release that addresses some bugs and issues that arose when play-testing v0.1, specifically with play-testing lower-difficulty Sandbox matches. No major additions have been added.

- Bug Fixes:
	- In a scenario where the player clicks a discarded card (happens more often than you think), the game would produce a NullReferenceException. This has been fixed.
		- If the player starts an attack, and then clicks a discarded card, they will select that discarded card as their "combat card". This has also been fixed.
	- In a rare scenario where a bot is looking for a target, they would target a nemesis (even if they were dead.) This loophole bug has been fixed.
- Miscellaneous Notes:
	- Moved "Main Canvas" of the Main Menu to World Space, fixing the camera shake that was quite weird previously. (Although the values of the "focus shake" were slightly altered too.)
	- Added a "CLICK TO START" prompt in the Main Menu, in case anyone was confused about the blur.
	- Various error tooltips have been added along with the bug fixes addressed in this minor update.

## v0.1 Beta

This is the first official (beta) release of Land of Heroes! Since this is the first official version, I will document all of the notable additions and point-of-interest mechanics in the changelog. Keep in mind that a lot of things have already been added, and I may have missed a thing or two.

- Current scenes: Main Menu and Sandbox Scene.
- Main Menu:
	- Buttons to quit or play a match of Sandbox.
	- Button to enter the Shop.
	- Currently, you can purchase champions for the shop (a default is given if you have insufficient funds.) This will be changed and improved in the future as I progress and add new content into the game.
	- A confirmation to purchase will be prompted, so accidental clicks will not result in accidental purchases and waste of money.
	- You cannot purchase duplicate items.<br>
	- Upon start-up, the Main Menu will be unfocused. A shake-effect on the camera and the logo will be applied and the menu will be focused on a click (for dramatic effect.)
- Sandbox:
	- Sandbox mode currently has two *offline* gamemodes: Competitive 2v2 and FFA (Free-For-All).
		- In Competitive 2v2, you fight against two other opponents with an ally.
		- In FFA, you fight alone against  two other opponents (however, the two opponents are not on the same team either, hence the name "FFA".)
	- Additionally, Sandbox mode currently has four difficulty levels:
		- Noob: Noob.
		- Novice: A beginner-friendly difficulty that allows the player to get away with grave mistakes, and generally allow any inexperienced player to casually win.
		- Warrior: This difficulty amps the game up. The bots are semi-experienced with the mechanics of the game, and a semi-experienced player should require some logic in order to casually win.
		- Champion: OHHHHHH YEAHHHHHHHH! The way the game was meant to be played. The bots have advanced coherence, can make important decisions that may save their own (or an ally's) life, and understand the proper way to use a card to it's maximum potential. An experienced player must use skill, advanced logic, and knowledge of their own champion in order to achieve victorious. Bonus loot and gold awarded from this difficulty.
	- At the beginning of the game, you are dealt 4 cards. At the beginning of your turn, you are dealt 2 cards.
		- Currently, the player will always be the player with the first turn. This is to change in the near future.
	- During a turn, there are three phases:
		- Beginning Phase: The phase where you draw 2 cards (and check for any abilities automatically activated during the Beginning Phase.)
		- Action Phase: The phase you play your cards and activate certain abilities (and check for any abilities automatically activated during the Action Phase.)
		- End Phase: The phase where you discard additional cards (and check for any abilities automatically activated during the End Phase.)
			- Since you're only allowed to have a maximum of 6 cards, you will be forced to discard until you have 6 cards in your hand during the End Phase.
			- This doesn't matter however if it's not your End Phase..., yet. If you have more than 6 cards, you can keep them until the End Phase. This rule is only enforced *during **your** End Phase.*
			- Discarded cards will be labeled "DISCARDED".
		- After these three phases, the game will check for the next player's turn.
		- Glow effects will indicate which player the current turn belongs to.
	- Every card will have a slightly/vastly different function.
		- Spades: Initializes an attack when played normally.
			- Upon initializing an attack, the player will be prompted to select a target.
				- The target will glow red when they are selected.
			- Additionally, the player will also be prompted to select a "combat card."
				- The "combat card" will glow white when it is selected.
				- After the prerequisites are selected and the attack is confirmed, the defender will be prompted to select a "combat card" as well.
				- Alternatively, you can "gamble" a "combat card", which will randomly generate a card for you as your "combat card". The downside of this is that you have no idea what that card is until after the attack is finished (as the "combat card" will be flipped), and it is extremely easy to lose an attack using this method.
			- The "combat cards" will be compared, and the larger of the two will deal damage to the other player. If the "combat cards" have the same value (tie), no damage will be dealt.
				- Combat Advantage is the modifier to a "combat card's" value. A positive Combat Advantage will give the "combat card" a positive advantage during combat. A value of 13 with a +1 Combat Advantage will have a value of 14 during combat. This is also true for negative Combat Advantages.
				- The camera and the champion's avatar will shake when that champion takes damage.
				- Blood will splatter from champions when they take damage. The more damage taken, the more blood splatter.
		- Hearts: Heal for a certain amount when played normally.
			- Bonus: Healing (by any means) has an unique sound effect.
		- Clubs: Trades for another card when played normally.
		- Diamonds: Miscellaneous actions when played normally.
	- The player can choose their own champion. Every champion has a different skill-set, list of abilities, physical attributes, and avatar. Players can also purchase new champions from the shop in the Main Menu, and every champion has a different cost (although it is quite balanced and can typically be grouped into particular price groups.) In the near future, some champions may have additional voice lines (given I have the resources to record these voice lines.)
		- There are currently seven playable champions in the game; more will be added down the line:
			- Arya (The Paladin): Large health pool allows for tanking damage, and punishes other players when healing.
			- Ambush Trooper: **Stealth** allows for the Ambush Trooper to avoid being targeted for attacks if the Ambush Trooper has not played a card in the last turn.
			- Regime Soldier: The default champion. Average health and damage with no special abilities.
			- Regime Captain: An upgraded Regime Soldier. Can be dealt additional cards for the amount of champions with the same faction.
			- Apprentice: **Bojutsu** gives attackers -1 Combat Advantage when using "combat cards" with large values against the Apprentice. **Quick Heal** also allows for a chance to rapidly heal a significant amount.
			- Castlefel Rebel: **Strategic Maneuver** allows the Castlefel Rebel to draw back cards when using high-value "combat cards" to defend. Not to mention, Castlefel Rebel is also one of the cheapest champions currently in the game.
			- Hoplomachus: **Hoplite Shield** gives the Hoplomachus a chance to cut incoming melee/ranged damage in half.
		- Bots randomly choose their champion. The higher the difficulty, the smarter they will be when choosing their champion.
		- Certain abilities that are not automatically activated may ask for your permission prior to activating. This will show up via a confirmation prompt near the bottom-thirds of your screen.
		- When abilities are activated, they will pop up as a small, red feed on their respective champion's avatar. Currently, the process of doing the same but with a persistent state the champion is supposedly in is operational, but isn't implemented just yet.
		- Champions will start to drip blood after reaching a certain health percentage. Furthermore, champions will start to bleed after reaching a critical health percentage.
		- Bonus: All champions are hand-drawn by the creator. Some of the drawings may be terrible; others may be fantastic. Hopefully in the future we could colorize or remake the sprites of the champion avatars, but this will have to do for now.
	- Hovering over certain objects will provide a more detailed, verbose, and accurate description of the particular object that the player is hovering over. This is done via a tooltip system. For example, if the player hovers over a champion in-game, it will show their current health, attack damage, amount of cards in their hand, and abilities.
		- When the game prevents the player from doing a particular action, an error tooltip will appear. This tooltip, unlike regular tooltips, do not update their position to the mouse in real-time, and will fade away after a certain duration.
	- The player will be awarded in-game currency (gold) when they win or lose a match of Sandbox. The amount rewarded will be dependent on the difficulty of the match. Gold can be used to purchase champions and other items (coming soon) in the shop on the Main Menu.
		- Owned gold and owned champions are serialized and stored in JSON. The file format that it saves to is a custom file extension: `.lohsave` <br>
		This file extension can be modified with a text editor, and is saved under a `Saves` folder under the application's path.
- Compatibility:
	- The project currently supports Windows, MacOS, Linux, iOS (additional testing needed), and Android. You can build the project yourself using a compatible version of Unity to an operating system of your choice.
	- As of writing, the current version of Unity the project supports is `Unity 2020.3.11f1`.
	- As of writing, the current IDEs the project supports is JetBrains Rider, Visual Studio 2019 (recommended), and Visual Studio Code.
- Miscellaneous Notes:
	- Champions are created by creating new Champion ScriptableObjects. These ScriptableObjects are then loaded onto a template to be used.
	- Cards are created by creating new Card ScriptableObjects. These ScriptableObjects are then loaded onto a template whenever cards are dealt, ready for use.
