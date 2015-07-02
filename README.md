# Ire
McMahon Swiss Draw Program based on Rating

Ire is a draw program. It is designed to work with the European Go Ratings (GoR). That means that it uses those ratings to make pairings. It will create either a Swiss or a McMahon format tournament. It is not designed to do anything else, therefore using it to do anything else is something that you do at your own risk. It doesn't have a fancy graphical user interface. This is because its author has no idea how to program one, wasn't really interested in learning how to, and was in fact only really interested in coding pairing rules. Thus it works with a combination of command line and basic tsv / csv file. So yeah, you have to do a bit of cut and paste work, but hopefully that isn’t particularly taxing.

In the first version of the program you have the following functionality
Basic pairing algorithm is fold pairing
Colours and Handicaps are not balanced or minimised
You can set a top group and a bottom group

Swiss Rules it is based upon are those of FIDE, with some small changes intially:
The number of rounds to be played is declared beforehand
Two players shall not play each other more than once
Should the total number of players be (or become) odd, one player is unpaired. He receives a bye: no colour and as many points as are rewarded for a win, unless the regulations of the tournament state otherwise (We set 0.5 for BYE)
A player who, for whatever reason, has received any number of points without playing, shall not receive a bye.
In general, players are paired to others with the same score
OMITTED: For each player the difference of the number of black and the number of white games shall not be greater than 2 or less than –2. 
Each system may have exceptions to this rule in the last round of a tournament.
OMITTED: No player will receive the same colour three times in a row.
Each system may have exceptions to this rule in the last round of a tournament.
OMITTED: In general, a player is given a colour as many times as he is given the other colour.
OMITTED: In general, a player is given the colour other than that he was given the previous round.   
The pairing rules must be such transparent that the person who is in charge for the pairing can explain them
